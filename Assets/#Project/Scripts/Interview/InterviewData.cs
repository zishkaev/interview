using System;

namespace Interview
{
    /// <summary>Качество варианта ответа (сильный / средний / слабый).</summary>
    public enum Quality
    {
        weak = 0,
        medium = 1,
        strong = 2
    }

    public static class QualityUtil
    {
        /// <summary>
        /// Разбирает строку из JSON. JsonUtility не умеет читать enum из строки,
        /// поэтому в JSON поле хранится строкой и переводится в enum здесь.
        /// Понимает: strong/medium/weak, A/B/C, сильный/средний/слабый, 2/1/0.
        /// </summary>
        public static bool TryParse(string value, out Quality quality)
        {
            quality = Quality.weak;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "strong":
                case "a":
                case "а":
                case "сильный":
                case "2":
                    quality = Quality.strong;
                    return true;
                case "medium":
                case "b":
                case "в":
                case "средний":
                case "1":
                    quality = Quality.medium;
                    return true;
                case "weak":
                case "c":
                case "с":
                case "слабый":
                case "0":
                    quality = Quality.weak;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Баллы за ответ: strong = 2, medium = 1, weak = 0.</summary>
        public static int Points(Quality quality)
        {
            return (int)quality;
        }

        public const int MaxPoints = 2;
    }

    /// <summary>
    /// Один вариант ответа: текст, качество и комментарий,
    /// который показывается игроку после выбора этого варианта.
    /// </summary>
    [Serializable]
    public class AnswerData
    {
        /// <summary>Текст варианта ответа.</summary>
        public string text;

        /// <summary>Качество в JSON: "strong" | "medium" | "weak". Для кода используйте <see cref="Level"/>.</summary>
        public string quality;

        /// <summary>Обратная связь системы на этот вариант.</summary>
        public string comment;

        public AnswerData()
        {
        }

        public AnswerData(string text, Quality level, string comment)
        {
            this.text = text;
            this.quality = level.ToString();
            this.comment = comment;
        }

        /// <summary>Разобранное качество ответа (нераспознанное значение считается weak).</summary>
        public Quality Level
        {
            get
            {
                QualityUtil.TryParse(quality, out Quality level);
                return level;
            }
        }

        public override string ToString()
        {
            return "[" + Level + "] " + text;
        }
    }

    /// <summary>Один этап интервью: вопрос рекрутера и варианты ответа.</summary>
    [Serializable]
    public class QuestionData
    {
        /// <summary>Уникальный идентификатор вопроса внутри вакансии.</summary>
        public string id;

        /// <summary>Название этапа, например «Этап 2. Профессиональные компетенции (кейс-задача)».</summary>
        public string category;

        /// <summary>Цель этапа (что оценивается).</summary>
        public string goal;

        /// <summary>Текст вопроса рекрутера.</summary>
        public string text;

        /// <summary>Варианты ответа.</summary>
        public AnswerData[] answers;

        public QuestionData()
        {
        }

        public QuestionData(string id, string category, string goal, string text, AnswerData[] answers)
        {
            this.id = id;
            this.category = category;
            this.goal = goal;
            this.text = text;
            this.answers = answers;
        }

        public int AnswersCount
        {
            get { return answers == null ? 0 : answers.Length; }
        }

        /// <summary>Есть ли среди вариантов хотя бы один сильный.</summary>
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
                    if (answers[i] != null && answers[i].Level == Quality.strong)
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
    /// Одна вакансия (один сценарий собеседования). Лежит отдельным JSON
    /// в Resources/Interview/Vacancies. Справочники уровней, факультетов и компаний —
    /// в Resources/Interview/catalog.json (см. <see cref="CatalogData"/>).
    /// </summary>
    [Serializable]
    public class InterviewData
    {
        /// <summary>Идентификатор вакансии (совпадает с именем файла).</summary>
        public string id;

        /// <summary>Id компании из каталога.</summary>
        public string companyId;

        /// <summary>Название позиции.</summary>
        public string position;

        /// <summary>Заголовок «Компания — Позиция» (для логов и старого UI).</summary>
        public string title;

        /// <summary>Ключевая задача тренажёра — краткое описание вакансии.</summary>
        public string description;

        /// <summary>Целевая аудитория из сценария.</summary>
        public string targetAudience;

        /// <summary>Короткая рекомендация по курсу для карточки вакансии («Выпускной курс»).</summary>
        public string courseRecommendation;

        /// <summary>Кто ведёт диалог («Представитель кадровой службы»).</summary>
        public string recruiterRole;

        /// <summary>Id факультетов из каталога, для которых доступна вакансия. Уровень образования берётся из факультета.</summary>
        public string[] faculties;

        /// <summary>Рекомендации курсов и программ ДПО по итогам собеседования.</summary>
        public string recommendations;

        /// <summary>Этапы собеседования.</summary>
        public QuestionData[] questions;

        public InterviewData()
        {
        }

        public int QuestionsCount
        {
            get { return questions == null ? 0 : questions.Length; }
        }

        public bool HasFaculty(string facultyId)
        {
            return faculties != null && Array.IndexOf(faculties, facultyId) >= 0;
        }

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
            return (string.IsNullOrEmpty(title) ? (string.IsNullOrEmpty(id) ? "InterviewData" : id) : title) + " (" + QuestionsCount + " вопросов)";
        }
    }
}
