using System.Text;

namespace Interview
{
    /// <summary>
    /// Приводит «почти JSON» (обычно это вывод ИИ, скопированный из чата) к виду,
    /// который принимает строгий <c>JsonUtility</c>:
    ///   * убирает BOM в начале файла;
    ///   * снимает markdown-обёртку (три обратных кавычки с языком и закрывающая тройка);
    ///   * удаляет блочные и строчные комментарии в стиле JavaScript;
    ///   * удаляет висячие запятые перед «}» и «]».
    ///
    /// Всё внутри строковых литералов остаётся нетронутым, поэтому текст с «//» или «, }» не портится.
    /// Класс не зависит от UnityEngine, поэтому его легко покрыть обычным юнит-тестом.
    /// </summary>
    internal static class JsonSanitizer
    {
        /// <summary>Возвращает JSON, пригодный для <c>JsonUtility.FromJson</c>.</summary>
        public static string Sanitize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            string text = json[0] == '\uFEFF' ? json.Substring(1) : json;
            text = StripCodeFences(text.Trim());

            var builder = new StringBuilder(text.Length);
            bool inString = false;
            bool escaping = false;

            for (int i = 0; i < text.Length; i++)
            {
                char current = text[i];

                if (inString)
                {
                    builder.Append(current);

                    if (escaping)
                    {
                        escaping = false;
                    }
                    else if (current == '\\')
                    {
                        escaping = true;
                    }
                    else if (current == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (current == '"')
                {
                    inString = true;
                    builder.Append(current);
                    continue;
                }

                if (current == '/' && i + 1 < text.Length && text[i + 1] == '/')
                {
                    i += 2;

                    while (i < text.Length && text[i] != '\n')
                    {
                        i++;
                    }

                    builder.Append(' ');
                    continue;
                }

                if (current == '/' && i + 1 < text.Length && text[i + 1] == '*')
                {
                    i += 2;

                    while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/'))
                    {
                        i++;
                    }

                    i++;
                    builder.Append(' ');
                    continue;
                }

                if (current == ',' && IsTrailingComma(text, i + 1))
                {
                    continue;
                }

                builder.Append(current);
            }

            return builder.ToString().Trim();
        }

        private static bool IsTrailingComma(string text, int index)
        {
            int i = index;

            while (i < text.Length && char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            return i < text.Length && (text[i] == '}' || text[i] == ']');
        }

        /// <summary>Убирает markdown-обёртку ```json ... ``` (если ИИ её добавил).</summary>
        private static string StripCodeFences(string text)
        {
            if (!text.StartsWith("```"))
            {
                return text;
            }

            int firstLineEnd = text.IndexOf('\n');

            if (firstLineEnd < 0)
            {
                return text;
            }

            string body = text.Substring(firstLineEnd + 1);
            int fenceStart = body.LastIndexOf("```", System.StringComparison.Ordinal);

            return fenceStart >= 0 ? body.Substring(0, fenceStart) : body;
        }
    }
}