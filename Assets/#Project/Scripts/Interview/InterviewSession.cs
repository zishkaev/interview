using System;
using Unity.Mathematics;
using UnityEngine;

namespace Interview
{
    /// <summary>Результат выбора варианта ответа.</summary>
    [Serializable]
    public struct AnswerResult
    {
        /// <summary>Удалось ли принять выбор.</summary>
        public bool isValid;

        /// <summary>Индекс выбранного варианта в текущем вопросе.</summary>
        public int answerIndex;

        /// <summary>Текст выбранного варианта.</summary>
        public string answerText;

        /// <summary>Правильный ли выбранный вариант.</summary>
        public Quality quality;

        /// <summary>Комментарий к выбранному варианту.</summary>
        public string comment;

        /// <summary>Выбор отклонён, потому что на вопрос уже отвечали.</summary>
        public bool alreadyAnswered;
    }

    /// <summary>
    /// Рантайм-сессия интервью: держит загруженный банк вопросов, текущий вопрос,
    /// выдаёт комментарий к выбранному варианту и считает статистику.
    ///
    /// Использование:
    ///   1. Повесить компонент на объект в сцене.
    ///   2. Перетащить в поле Data File JSON-ассет с вопросами.
    ///   3. Слушать события QuestionStarted / Answered / Completed
    ///      либо опрашивать CurrentQuestion / SelectAnswer / MoveNext.
    /// </summary>
    [AddComponentMenu("Interview/Interview Session")]
    [DisallowMultipleComponent]
    public class InterviewSession : MonoBehaviour
    {
        [Header("Источник данных (JSON)")]
        [Tooltip("JSON-ассет с вопросами. Имеет приоритет над путём в Resources.")]
        [SerializeField] private TextAsset dataFile;

        [Tooltip("Путь в Resources без расширения, если Data File не задан, например Interview/csharp_basics")]
        [SerializeField] private string resourcesPath = "Interview/csharp_basics";

        [Tooltip("Загрузить данные автоматически в Start().")]
        [SerializeField] private bool loadOnStart = true;

        [Header("Порядок")]
        [Tooltip("Перемешать вопросы.")]
        [SerializeField] private bool shuffleQuestions;

        [Tooltip("Перемешать варианты ответа внутри каждого вопроса.")]
        [SerializeField] private bool shuffleAnswers;

        [Tooltip("Зерно генератора. 0 — новый случайный порядок при каждом запуске.")]
        [SerializeField] private int randomSeed;

        [Header("Правила")]
        [Tooltip("Разрешить менять ответ после первого выбора.")]
        [SerializeField] private bool allowAnswerChange;

        /// <summary>Данные загружены (вызывается один раз при Load).</summary>
        public event Action<InterviewData> Loaded;

        /// <summary>Начался новый вопрос.</summary>
        public event Action<QuestionData> QuestionStarted;

        /// <summary>Игрок выбрал вариант ответа (комментарий лежит внутри результата).</summary>
        public event Action<AnswerResult> Answered;

        /// <summary>Вопросы закончились.</summary>
        public event Action Completed;

        /// <summary>Исходные загруженные данные (в исходном порядке).</summary>
        public InterviewData Data { get; private set; }

        /// <summary>Индекс текущего вопроса, -1 до начала.</summary>
        public int CurrentIndex { get; private set; }

        /// <summary>Результат последнего выбора.</summary>
        public AnswerResult LastResult { get; private set; }

        private QuestionData[] runtimeQuestions;
        private bool[] answered;
        private Quality[] correctAnswers;
        private System.Random random;

        /// <summary>Количество вопросов в сессии.</summary>
        public int QuestionCount
        {
            get { return runtimeQuestions == null ? 0 : runtimeQuestions.Length; }
        }

        /// <summary>Загружены ли данные.</summary>
        public bool IsLoaded
        {
            get { return QuestionCount > 0; }
        }

        /// <summary>Текущий вопрос или null.</summary>
        public QuestionData CurrentQuestion
        {
            get
            {
                if (runtimeQuestions == null || CurrentIndex < 0 || CurrentIndex >= runtimeQuestions.Length)
                {
                    return null;
                }

                return runtimeQuestions[CurrentIndex];
            }
        }

        /// <summary>Отвечал ли игрок на текущий вопрос.</summary>
        public bool HasAnsweredCurrent
        {
            get { return IsAnswered(CurrentIndex); }
        }

        /// <summary>Все вопросы пройдены.</summary>
        public bool IsCompleted
        {
            get { return IsLoaded && CurrentIndex >= QuestionCount - 1 && HasAnsweredCurrent; }
        }

        /// <summary>Сколько вопросов получило ответ.</summary>
        public int AnsweredCount
        {
            get { return CountFlags(answered, true); }
        }

        /// <summary>Количество правильных ответов.</summary>
        public int CorrectCount
        {
            get { return CountFlags(correctAnswers, Quality.strong); }
        }

        /// <summary>Количество неправильных ответов.</summary>
        public int WrongCount
        {
            get { return AnsweredCount - CorrectCount; }
        }

        /// <summary>Доля правильных ответов от числа данных ответов (0..1).</summary>
        public float Score
        {
            get
            {
                int answeredCount = AnsweredCount;
                return answeredCount == 0 ? 0f : (float)CorrectCount / answeredCount;
            }
        }

        private void Start()
        {
            if (loadOnStart)
            {
                Load();
            }
        }

        /// <summary>Загружает данные из Data File или из Resources и запускает сессию.</summary>
        public bool Load()
        {
            InterviewData loaded = dataFile != null
                ? InterviewLoader.FromTextAsset(dataFile)
                : InterviewLoader.FromResources(resourcesPath);

            if (loaded == null)
            {
                Debug.LogError(InterviewLoader.LogPrefix + " " + name + ": не удалось загрузить данные интервью.");
                return false;
            }

            return Load(loaded);
        }

        /// <summary>Запускает сессию на уже разобранных данных.</summary>
        public bool Load(InterviewData data)
        {
            if (data == null || data.questions == null || data.questions.Length == 0)
            {
                Debug.LogError(InterviewLoader.LogPrefix + " " + name + ": нет вопросов для запуска сессии.");
                return false;
            }

            Data = data;
            runtimeQuestions = (QuestionData[])data.questions.Clone();
            random = new System.Random(randomSeed == 0 ? Environment.TickCount : randomSeed);

            if (shuffleQuestions)
            {
                Shuffle(runtimeQuestions);
            }

            if (shuffleAnswers)
            {
                for (int i = 0; i < runtimeQuestions.Length; i++)
                {
                    runtimeQuestions[i] = WithShuffledAnswers(runtimeQuestions[i]);
                }
            }

            answered = new bool[runtimeQuestions.Length];
            correctAnswers = new Quality[runtimeQuestions.Length];
            CurrentIndex = -1;
            LastResult = default;

            if (Loaded != null)
            {
                Loaded(Data);
            }

            MoveNext();
            return true;
        }

        /// <summary>Начинает сессию заново теми же данными.</summary>
        public void Restart()
        {
            if (Data == null)
            {
                Debug.LogWarning(InterviewLoader.LogPrefix + " " + name + ": сессия ещё не загружена.");
                return;
            }

            Load(Data);
        }

        /// <summary>Переходит к следующему вопросу. Возвращает false, если вопросы закончились.</summary>
        public bool MoveNext()
        {
            if (!IsLoaded)
            {
                return false;
            }

            if (CurrentIndex + 1 >= runtimeQuestions.Length)
            {
                if (Completed != null)
                {
                    Completed();
                }

                return false;
            }

            CurrentIndex++;
            LastResult = default;

            QuestionData question = CurrentQuestion;

            if (question != null && QuestionStarted != null)
            {
                QuestionStarted(question);
            }

            return true;
        }

        /// <summary>Переходит к вопросу с указанным индексом (например, при загрузке сохранения).</summary>
        public bool GoTo(int index)
        {
            if (!IsLoaded || index < 0 || index >= runtimeQuestions.Length)
            {
                return false;
            }

            CurrentIndex = index;
            LastResult = default;

            QuestionData question = CurrentQuestion;

            if (question != null && QuestionStarted != null)
            {
                QuestionStarted(question);
            }

            return true;
        }

        /// <summary>
        /// Игрок выбрал вариант ответа текущего вопроса.
        /// Возвращает результат с текстом ответа, признаком правильности и комментарием.
        /// </summary>
        public AnswerResult SelectAnswer(int answerIndex)
        {
            var result = new AnswerResult { answerIndex = answerIndex };
            QuestionData question = CurrentQuestion;

            if (question == null || question.answers == null || answerIndex < 0 || answerIndex >= question.answers.Length)
            {
                Debug.LogWarning(InterviewLoader.LogPrefix + " " + name + ": некорректный индекс ответа " + answerIndex + ".");
                return result;
            }

            AnswerData answer = question.answers[answerIndex];
            result.answerText = answer.text;
            result.quality = answer.quality;
            result.comment = answer.comment;

            if (answered[CurrentIndex] && !allowAnswerChange)
            {
                result.alreadyAnswered = true;
                return result;
            }

            answered[CurrentIndex] = true;
            correctAnswers[CurrentIndex] = answer.quality;

            result.isValid = true;
            LastResult = result;

            if (Answered != null)
            {
                Answered(result);
            }

            return result;
        }

        /// <summary>Тексты вариантов текущего вопроса — удобно раскидать по кнопкам UI.</summary>
        public string[] GetCurrentAnswerTexts()
        {
            QuestionData question = CurrentQuestion;

            if (question == null || question.answers == null)
            {
                return new string[0];
            }

            var texts = new string[question.answers.Length];

            for (int i = 0; i < question.answers.Length; i++)
            {
                texts[i] = question.answers[i] == null ? string.Empty : question.answers[i].text;
            }

            return texts;
        }

        /// <summary>Комментарий к варианту текущего вопроса (без необходимости отвечать).</summary>
        public string GetCurrentAnswerComment(int answerIndex)
        {
            QuestionData question = CurrentQuestion;

            if (question == null || question.answers == null || answerIndex < 0 || answerIndex >= question.answers.Length)
            {
                return string.Empty;
            }

            AnswerData answer = question.answers[answerIndex];
            return answer == null ? string.Empty : answer.comment;
        }

        /// <summary>Правильный ли вариант с указанным индексом.</summary>
        public bool IsCorrectAnswer(int answerIndex)
        {
            QuestionData question = CurrentQuestion;

            if (question == null || question.answers == null || answerIndex < 0 || answerIndex >= question.answers.Length)
            {
                return false;
            }

            AnswerData answer = question.answers[answerIndex];
            return answer != null && answer.quality == Quality.strong;
        }

        /// <summary>Отвечал ли игрок на вопрос с указанным индексом.</summary>
        public bool IsAnswered(int index)
        {
            return answered != null && index >= 0 && index < answered.Length && answered[index];
        }

        private static int CountFlags(bool[] flags, bool value)
        {
            if (flags == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i] == value)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountFlags(Quality[] flags, Quality value)
        {
            if (flags == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i] == value)
                {
                    count++;
                }
            }

            return count;
        }

        private void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temporary = array[i];
                array[i] = array[j];
                array[j] = temporary;
            }
        }

        private QuestionData WithShuffledAnswers(QuestionData question)
        {
            if (question == null || question.answers == null || question.answers.Length == 0)
            {
                return question;
            }

            var answers = (AnswerData[])question.answers.Clone();
            Shuffle(answers);
            return new QuestionData(question.id, question.category, question.text, answers);
        }
    }
}