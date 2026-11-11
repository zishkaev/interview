using System.Text.RegularExpressions;
using UnityEngine;

namespace Interview
{
    /// <summary>Общие подписи и цвета для чата и итогового отчёта.</summary>
    public static class InterviewText
    {
        public static readonly Color StrongColor = new Color32(0x2E, 0x9E, 0x5B, 0xFF);
        public static readonly Color MediumColor = new Color32(0xE0, 0x8E, 0x1B, 0xFF);
        public static readonly Color WeakColor = new Color32(0xD6, 0x45, 0x45, 0xFF);

        private static readonly Regex StagePrefix = new Regex(@"^\s*Этап\s*\d+\s*\.\s*");
        private static readonly Regex Parentheses = new Regex(@"\s*\([^)]*\)\s*$");

        /// <summary>«Этап 2. Профессиональные компетенции (кейс-задача)» → «Профессиональные компетенции».</summary>
        public static string StageName(QuestionData question)
        {
            if (question == null || string.IsNullOrEmpty(question.category))
            {
                return string.Empty;
            }

            string name = StagePrefix.Replace(question.category, string.Empty);
            name = Parentheses.Replace(name, string.Empty);
            return name.Trim();
        }

        /// <summary>Тип задания из скобок: «кейс-задача», или пусто.</summary>
        public static string StageKind(QuestionData question)
        {
            if (question == null || string.IsNullOrEmpty(question.category))
            {
                return string.Empty;
            }

            Match match = Regex.Match(question.category, @"\(([^)]*)\)\s*$");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public static string QualityLabel(Quality quality)
        {
            switch (quality)
            {
                case Quality.strong:
                    return "Сильный ответ";
                case Quality.medium:
                    return "Средний ответ";
                default:
                    return "Слабый ответ";
            }
        }

        public static Color QualityColor(Quality quality)
        {
            switch (quality)
            {
                case Quality.strong:
                    return StrongColor;
                case Quality.medium:
                    return MediumColor;
                default:
                    return WeakColor;
            }
        }

        public static string Hex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        /// <summary>Короткий вердикт по проценту.</summary>
        public static string Verdict(int percent)
        {
            if (percent >= 85)
            {
                return "Отличный результат! Вы хорошо готовы к собеседованию.";
            }

            if (percent >= 60)
            {
                return "Хороший результат! Есть потенциал для роста.";
            }

            if (percent >= 35)
            {
                return "Неплохая база, но есть заметные зоны роста.";
            }

            return "Есть над чем поработать — изучите рекомендации и попробуйте ещё раз.";
        }
    }
}
