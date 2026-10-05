using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Interview
{
    /// <summary>
    /// Загрузка банка вопросов из JSON-ассетов.
    /// Структура JSON совпадает с классами из <see cref="InterviewData"/>:
    /// title, description, questions[] → answers[] (text, isCorrect, comment).
    ///
    /// Точки входа: <see cref="FromTextAsset"/>, <see cref="FromResources"/>, <see cref="FromFile"/>,
    /// <see cref="FromJson"/>. Все они разбирают JSON, нормализуют данные и валидируют их.
    /// </summary>
    public static class InterviewLoader
    {
        /// <summary>Префикс всех сообщений в консоли Unity.</summary>
        public const string LogPrefix = "[Interview]";

        /// <summary>Загружает данные из ассета, назначенного в инспекторе.</summary>
        public static InterviewData FromTextAsset(TextAsset asset)
        {
            if (asset == null)
            {
                Debug.LogError(LogPrefix + " не задан TextAsset с вопросами.");
                return null;
            }

            return FromJson(asset.text, asset.name);
        }

        /// <summary>Загружает данные из папки Resources (путь без расширения, например "Interview/csharp_basics").</summary>
        public static InterviewData FromResources(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                Debug.LogError(LogPrefix + " не задан путь Resources.");
                return null;
            }

            TextAsset asset = Resources.Load<TextAsset>(resourcePath.Trim());

            if (asset == null)
            {
                Debug.LogError(LogPrefix + " в Resources не найден TextAsset «" + resourcePath + "».");
                return null;
            }

            return FromJson(asset.text, asset.name);
        }

        /// <summary>Загружает данные из файла на диске (удобно во время разработки).</summary>
        public static InterviewData FromFile(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                Debug.LogError(LogPrefix + " не задан путь к файлу.");
                return null;
            }

            if (!File.Exists(absolutePath))
            {
                Debug.LogError(LogPrefix + " файл не найден: " + absolutePath);
                return null;
            }

            try
            {
                // File.ReadAllText сам распознаёт BOM, поэтому UTF-8/UTF-8+BOM читаются корректно.
                string content = File.ReadAllText(absolutePath);
                return FromJson(content, Path.GetFileName(absolutePath));
            }
            catch (Exception exception)
            {
                Debug.LogError(LogPrefix + " не удалось прочитать файл «" + absolutePath + "»: " + exception.Message);
                return null;
            }
        }

        /// <summary>Разбирает JSON, нормализует данные и проверяет их на типичные ошибки.</summary>
        /// <param name="json">Содержимое JSON-ассета.</param>
        /// <param name="sourceName">Имя источника для сообщений (обычно имя ассета).</param>
        public static InterviewData FromJson(string json, string sourceName = null)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError(LogPrefix + " " + DisplayName(sourceName) + "JSON пустой.");
                return null;
            }

            // Убираем BOM, markdown-обёртку, комментарии и висячие запятые:
            // так JSON, скопированный из ответа ИИ, грузится без ручной правки.
            json = JsonSanitizer.Sanitize(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError(LogPrefix + " " + DisplayName(sourceName) + "после очистки не осталось JSON.");
                return null;
            }

            try
            {
                InterviewData data = JsonUtility.FromJson<InterviewData>(json);

                if (data == null)
                {
                    Debug.LogError(LogPrefix + " " + DisplayName(sourceName) + "не удалось разобрать JSON.");
                    return null;
                }

                Normalize(data);
                Validate(data, sourceName);
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogError(LogPrefix + " " + DisplayName(sourceName) + "ошибка разбора JSON: " + exception.Message);
                return null;
            }
        }

        /// <summary>
        /// Проверяет данные на типичные ошибки автора (пустой вопрос, ни одного правильного ответа,
        /// дубли id, отсутствующий комментарий и т.п.) и пишет их в консоль Unity.
        /// </summary>
        /// <returns>true, если ошибок не найдено.</returns>
        public static bool Validate(InterviewData data, string sourceName = null, bool logIssues = true)
        {
            if (data == null)
            {
                return false;
            }

            string prefix = LogPrefix + " " + DisplayName(sourceName);

            if (data.questions == null || data.questions.Length == 0)
            {
                Log(logIssues, prefix + "в наборе нет ни одного вопроса.");
                return false;
            }

            bool isValid = true;
            var seenIds = new HashSet<string>();

            for (int i = 0; i < data.questions.Length; i++)
            {
                QuestionData question = data.questions[i];
                string where = "вопрос #" + (i + 1);

                if (question == null)
                {
                    Log(logIssues, prefix + where + " пустой.");
                    isValid = false;
                    continue;
                }

                if (string.IsNullOrEmpty(question.text))
                {
                    Log(logIssues, prefix + where + " без текста.");
                    isValid = false;
                }

                if (!string.IsNullOrEmpty(question.id) && !seenIds.Add(question.id))
                {
                    Log(logIssues, prefix + where + ": дублирующийся id «" + question.id + "».");
                    isValid = false;
                }

                if (question.answers == null || question.answers.Length == 0)
                {
                    Log(logIssues, prefix + where + " («" + Shorten(question.text) + "») без вариантов ответа.");
                    isValid = false;
                    continue;
                }

                int correctCount = 0;
                int answersWithoutComment = 0;

                for (int a = 0; a < question.answers.Length; a++)
                {
                    AnswerData answer = question.answers[a];

                    if (answer == null || string.IsNullOrEmpty(answer.text))
                    {
                        Log(logIssues, prefix + where + ": вариант #" + (a + 1) + " без текста.");
                        isValid = false;
                        continue;
                    }

                    if (answer.quality == Quality.strong)
                    {
                        correctCount++;
                    }

                    if (string.IsNullOrEmpty(answer.comment))
                    {
                        answersWithoutComment++;
                    }
                }

                if (correctCount == 0)
                {
                    Log(logIssues, prefix + where + " («" + Shorten(question.text) + "»): нет ни одного правильного варианта.");
                    isValid = false;
                }
                else if (correctCount > 1)
                {
                    Log(logIssues, prefix + where + ": правильных вариантов больше одного (" + correctCount + ") — ок для мультивыбора.");
                }

                if (answersWithoutComment > 0)
                {
                    Log(logIssues, prefix + where + ": у " + answersWithoutComment + " вариант(ов) нет комментария.");
                }
            }

            if (isValid)
            {
                Debug.Log(prefix + "данные в порядке: " + data + ".");
            }

            return isValid;
        }

        /// <summary>Защищает рантайм-код от null-ов в разобранных данных.</summary>
        private static void Normalize(InterviewData data)
        {
            if (data == null || data.questions == null)
            {
                return;
            }

            for (int i = 0; i < data.questions.Length; i++)
            {
                QuestionData question = data.questions[i];

                if (question == null)
                {
                    continue;
                }

                question.text = question.text ?? string.Empty;

                if (question.answers == null)
                {
                    question.answers = new AnswerData[0];
                    continue;
                }

                for (int a = 0; a < question.answers.Length; a++)
                {
                    AnswerData answer = question.answers[a];

                    if (answer == null)
                    {
                        continue;
                    }

                    answer.text = answer.text ?? string.Empty;
                    answer.comment = answer.comment ?? string.Empty;
                }
            }
        }

        private static void Log(bool enabled, string message)
        {
            if (enabled)
            {
                Debug.LogWarning(message);
            }
        }

        private static string DisplayName(string sourceName)
        {
            return string.IsNullOrEmpty(sourceName) ? "«<без имени>»: " : "«" + sourceName + "»: ";
        }

        private static string Shorten(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Length <= 40 ? text : text.Substring(0, 37) + "...";
        }
    }
}