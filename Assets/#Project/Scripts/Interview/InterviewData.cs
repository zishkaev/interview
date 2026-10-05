using System;

namespace Interview
{
    public enum Quality
    {
        weak,
        medium,
        strong
    }
    /// <summary>
    /// Один вариант ответа: текст, признак правильности и комментарий,
    /// который показывается игроку после выбора этого варианта.
    /// </summary>
    [Serializable]
    public class AnswerData
    {
        /// <summary>Текст варианта ответа.</summary>
        public string text;

        /// <summary>Является ли вариант правильным.</summary>
        public Quality quality;

        /// <summary>Комментарий к варианту (почему верно / почему неверно). Может быть пустым.</summary>
        public string comment;

        public AnswerData()
        {
        }

        public AnswerData(string text, Quality quality, string comment)
        {
            this.text = text;
            this.quality = quality;
            this.comment = comment;
        }

        public override string ToString()
        {
            return (quality == Quality.strong ? "+ " : "- ") + text;
        }
    }

    /// <summary>
    /// Один вопрос интервью: текст вопроса и набор вариантов ответа
    /// (у каждого варианта свой комментарий).
    /// </summary>
    [Serializable]
    public class QuestionData
    {
        /// <summary>Уникальный идентификатор вопроса (для сохранений и статистики). Может быть пустым.</summary>
        public string id;

        /// <summary>Раздел / тема, к которой относится вопрос.</summary>
        public string category;

        /// <summary>Текст вопроса.</summary>
        public string text;

        /// <summary>Варианты ответа.</summary>
        public AnswerData[] answers;

        public QuestionData()
        {
        }

        public QuestionData(string id, string category, string text, AnswerData[] answers)
        {
            this.id = id;
            this.category = category;
            this.text = text;
            this.answers = answers;
        }

        /// <summary>Количество вариантов ответа.</summary>
        public int AnswersCount
        {
            get { return answers == null ? 0 : answers.Length; }
        }

        /// <summary>Есть ли среди вариантов хотя бы один правильный.</summary>
        public bool HasCorrectAnswer
        {
            get
            {
                if (answers == null)
                {
                    return false;
                }

                for (int i = 0; i < answers.Length; i++)
                {
                    if (answers[i] != null && answers[i].quality == Quality.strong)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(id) ? text : id + ": " + text;
        }
    }

    /// <summary>
    /// Содержимое одного файла-ассета (.json или .txt): название набора
    /// и список вопросов.
    /// </summary>
    [Serializable]
    public class InterviewData
    {
        /// <summary>Название набора вопросов.</summary>
        public string title;

        /// <summary>Описание набора (необязательно).</summary>
        public string description;

        /// <summary>Вопросы набора.</summary>
        public QuestionData[] questions;

        public InterviewData()
        {
        }

        public InterviewData(string title, string description, QuestionData[] questions)
        {
            this.title = title;
            this.description = description;
            this.questions = questions;
        }

        /// <summary>Количество вопросов в наборе.</summary>
        public int QuestionsCount
        {
            get { return questions == null ? 0 : questions.Length; }
        }

        /// <summary>Возвращает вопрос по идентификатору или null, если он не найден.</summary>
        public QuestionData GetQuestionById(string questionId)
        {
            if (string.IsNullOrEmpty(questionId) || questions == null)
            {
                return null;
            }

            for (int i = 0; i < questions.Length; i++)
            {
                if (questions[i] != null && questions[i].id == questionId)
                {
                    return questions[i];
                }
            }

            return null;
        }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(title) ? "InterviewData" : title) + " (" + QuestionsCount + " вопросов)";
        }
    }
}