using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Interview;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Импорт сценариев собеседований из .docx (папка Assets/#Project/Docs) в JSON вакансий
/// (Assets/Resources/Interview/Vacancies). Меню: Tools → Interview → Импорт сценариев из DOCX.
///
/// Содержимое (этапы, вопросы, ответы, обратная связь, рекомендации) всегда берётся из docx.
/// Привязка (companyId, faculties, courseRecommendation) берётся из таблицы ниже только при первом
/// импорте — если в JSON её уже поправили руками, ручные значения сохраняются.
/// </summary>
public static class InterviewDocxImporter
{
    private const string DocsFolder = "Assets/#Project/Docs";
    private const string OutputFolder = "Assets/Resources/Interview/Vacancies";

    private struct Mapping
    {
        public string fileKey;
        public string id;
        public string companyId;
        public string[] faculties;
        public string course;

        public Mapping(string fileKey, string id, string companyId, string course, params string[] faculties)
        {
            this.fileKey = fileKey;
            this.id = id;
            this.companyId = companyId;
            this.course = course;
            this.faculties = faculties;
        }
    }

    // Ключ — фрагмент имени файла (пробелы схлопываются). Факультеты — id из catalog.json.
    private static readonly Mapping[] Mappings =
    {
        new Mapping("Альфа-Банк Диалог_Стажер-аналитик", "alfa_analyst_intern", "alfa", "Выпускной курс", "fin", "fitbda"),
        new Mapping("Альфа-Банк Диалог_Стажер_структурное", "alfa_structured_finance_intern", "alfa", "Выпускники", "fin"),
        new Mapping("Аналитический центр", "acrf_analyst_intern", "acrf", "Последний курс и выпускники", "feb", "vshu"),
        new Mapping("Банк России", "cbr_financial_markets_intern", "cbr", "Студенты вузов", "fin"),
        new Mapping("Банк Синара", "sinara_management_reporting_analyst", "sinara", "Опыт от 1–2 лет", "ftaba", "fin"),
        new Mapping("ВК Диалог", "vk_product_analyst", "vk", "Опыт в продуктовой аналитике", "fin", "fitbda"),
        new Mapping("ВТБ Диалог_Стажер_риск", "vtb_risk_management_intern", "vtb", "Выпускники 2025–2027", "fin"),
        new Mapping("ВТБ Диалог_Стажер_финансовый", "vtb_financial_analyst_intern", "vtb", "Выпускники 2025–2027", "fin"),
        new Mapping("КЭПТ", "kept_corporate_tax_intern", "kept", "Последние курсы и выпускники", "ftaba"),
        new Mapping("Московская биржа", "moex_invest_education_intern", "moex", "Студенты и выпускники", "fin"),
        new Mapping("Правительство Москвы", "mosgov_events_intern", "mosgov", "Выпускники вузов", "fsnmk", "vshu"),
        new Mapping("Промсвязьбанк", "psb_senior_analyst", "psb", "Опыт от 1 года", "fin"),
        new Mapping("Росатом", "rosatom_data_analyst", "rosatom", "Последний курс, опыт до 0,5 года", "fitbda"),
        new Mapping("Ростелеком Диалог_Стажер_департамент_стратегии", "rostelecom_strategy_intern", "rostelecom", "Студенты вузов", "feb"),
        new Mapping("Ростелеком Диалог_Стажер_блок_аналитики", "rostelecom_analytics_intern", "rostelecom", "Студенты и выпускники", "fin"),
        new Mapping("Сбер", "sber_subsidiaries_finance_intern", "sber", "Магистратура", "fin"),
        new Mapping("Туризм.РФ", "tourism_project_coordinator", "tourism", "Специалисты с высшим образованием", "fin", "vshu"),
        new Mapping("ФБК Диалог_Стажер_аудит", "fbk_hightech_audit_intern", "fbk", "Студенты и выпускники", "ftaba"),
        new Mapping("ФБК Диалог_Стажер_оценка", "fbk_valuation_intern", "fbk", "Старшие курсы и выпускники", "fin"),
        new Mapping("Федеральное казначейство", "treasury_gov_finance_specialist", "treasury", "Выпускники вузов и колледжей", "fin", "mfc"),
    };

    private static readonly Regex StageRegex = new Regex(@"^Этап\s+(\d+)\s*\.\s*(.+)$");
    private static readonly Regex AnswerRegex = new Regex(@"^(?:Вариант\s+)?([ABCАВС])\s*\(\s*(сильный|средний|слабый)\s+ответ\s*\)\s*:\s*(.+)$", RegexOptions.IgnoreCase);
    private static readonly Regex FeedbackRegex = new Regex(@"^Ответ\s+([ABCАВС])\s*:\s*(.+)$");
    private static readonly Regex SpeechRegex = new Regex(@"^([^:«»]{3,60}):\s*(«.+)$");

    [MenuItem("Tools/Interview/Импорт сценариев из DOCX")]
    public static void ImportAll()
    {
        Directory.CreateDirectory(OutputFolder);

        int imported = 0;
        int failed = 0;
        var usedMappings = new HashSet<string>();

        foreach (string path in Directory.GetFiles(DocsFolder, "*.docx"))
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (fileName.StartsWith("~$") || fileName.IndexOf("Диалог", StringComparison.Ordinal) < 0)
            {
                continue;
            }

            try
            {
                Mapping? mapping = FindMapping(fileName);

                if (mapping == null)
                {
                    Debug.LogWarning(InterviewLoader.LogPrefix + " нет привязки для «" + fileName + "» — добавьте её в InterviewDocxImporter.Mappings.");
                    failed++;
                    continue;
                }

                usedMappings.Add(mapping.Value.id);
                InterviewData data = Parse(ReadParagraphs(path), mapping.Value, fileName);
                MergeManualFields(data);

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(Path.Combine(OutputFolder, data.id + ".json"), json, new UTF8Encoding(false));

                if (InterviewLoader.Validate(InterviewLoader.FromJson(json, data.id), data.id, true))
                {
                    imported++;
                }
                else
                {
                    failed++;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(InterviewLoader.LogPrefix + " ошибка импорта «" + fileName + "»: " + exception);
                failed++;
            }
        }

        foreach (Mapping mapping in Mappings)
        {
            if (!usedMappings.Contains(mapping.id))
            {
                Debug.LogWarning(InterviewLoader.LogPrefix + " для привязки «" + mapping.fileKey + "» не найден docx.");
            }
        }

        AssetDatabase.Refresh();
        InterviewCatalog.Reload();
        Debug.Log(InterviewLoader.LogPrefix + " импорт завершён: успешно " + imported + ", с ошибками " + failed + ". Папка: " + OutputFolder);
    }

    private static Mapping? FindMapping(string fileName)
    {
        string normalized = CollapseSpaces(fileName);

        foreach (Mapping mapping in Mappings)
        {
            if (normalized.IndexOf(CollapseSpaces(mapping.fileKey), StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return mapping;
            }
        }

        return null;
    }

    /// <summary>Абзацы документа как строки (текст всех run-ов абзаца склеен).</summary>
    private static List<string> ReadParagraphs(string path)
    {
        string xml;

        using (FileStream stream = File.OpenRead(path))
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
        {
            ZipArchiveEntry entry = zip.GetEntry("word/document.xml");

            using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
            {
                xml = reader.ReadToEnd();
            }
        }

        xml = Regex.Replace(xml, @"<w:tab\s*/>", " ");
        xml = Regex.Replace(xml, @"<w:br\s*/>", " ");
        xml = xml.Replace("</w:p>", "\n");
        xml = Regex.Replace(xml, @"<[^>]+>", string.Empty);
        xml = WebUtility.HtmlDecode(xml);

        var lines = new List<string>();

        foreach (string raw in xml.Split('\n'))
        {
            string line = CollapseSpaces(raw.Replace(' ', ' ')).Trim();

            if (line.Length > 0)
            {
                lines.Add(line);
            }
        }

        return lines;
    }

    private static InterviewData Parse(List<string> lines, Mapping mapping, string fileName)
    {
        var data = new InterviewData
        {
            id = mapping.id,
            companyId = mapping.companyId,
            faculties = mapping.faculties,
            courseRecommendation = mapping.course
        };

        var questions = new List<QuestionData>();
        var answers = new List<AnswerData>();
        var letters = new List<char>();
        QuestionData current = null;
        bool finished = false;
        string pendingSpeaker = null;

        void FlushQuestion()
        {
            if (current != null)
            {
                current.answers = answers.ToArray();
                questions.Add(current);
            }

            current = null;
            answers = new List<AnswerData>();
            letters = new List<char>();
        }

        foreach (string line in lines)
        {
            if (line.StartsWith("Сценарий диалогового тренажёра", StringComparison.Ordinal) || line.StartsWith("Сценарий диалогового тренажера", StringComparison.Ordinal))
            {
                int index = line.IndexOf("вакансии", StringComparison.Ordinal);
                data.position = Unquote(index >= 0 ? line.Substring(index + "вакансии".Length) : line);
                continue;
            }

            if (line.StartsWith("Целевая аудитория:", StringComparison.Ordinal))
            {
                data.targetAudience = Capitalize(AfterColon(line));
                continue;
            }

            if (line.StartsWith("Ключевая задача тренажёра:", StringComparison.Ordinal) || line.StartsWith("Ключевая задача тренажера:", StringComparison.Ordinal))
            {
                data.description = Capitalize(AfterColon(line));
                continue;
            }

            if (line.StartsWith("Завершение собеседования", StringComparison.Ordinal))
            {
                FlushQuestion();
                finished = true;
                continue;
            }

            if (line.IndexOf("Рекомендации по прохождению курсов", StringComparison.Ordinal) >= 0)
            {
                data.recommendations = ExtractRecommendations(line);
                continue;
            }

            if (finished)
            {
                continue;
            }

            Match stage = StageRegex.Match(line);

            if (stage.Success)
            {
                FlushQuestion();
                current = new QuestionData { id = "stage" + stage.Groups[1].Value, category = line, goal = string.Empty, text = string.Empty };
                continue;
            }

            if (current == null)
            {
                continue;
            }

            if (line.StartsWith("Цель", StringComparison.Ordinal) && line.IndexOf(':') > 0 && string.IsNullOrEmpty(current.goal))
            {
                current.goal = Capitalize(AfterColon(line));
                continue;
            }

            Match answer = AnswerRegex.Match(line);

            if (answer.Success)
            {
                letters.Add(NormalizeLetter(answer.Groups[1].Value));
                answers.Add(new AnswerData(Unquote(answer.Groups[3].Value), ToQuality(answer.Groups[2].Value), string.Empty));
                continue;
            }

            Match feedback = FeedbackRegex.Match(line);

            if (feedback.Success)
            {
                int index = letters.IndexOf(NormalizeLetter(feedback.Groups[1].Value));

                if (index >= 0)
                {
                    answers[index].comment = Unquote(feedback.Groups[2].Value);
                }
                else
                {
                    Debug.LogWarning(InterviewLoader.LogPrefix + " «" + fileName + "», " + current.category + ": обратная связь для варианта без ответа: " + line);
                }

                continue;
            }

            if (string.IsNullOrEmpty(current.text) && answers.Count == 0)
            {
                Match speech = SpeechRegex.Match(line);

                if (speech.Success && !line.StartsWith("Вариант", StringComparison.Ordinal))
                {
                    data.recruiterRole = speech.Groups[1].Value.Trim();
                    current.text = Unquote(speech.Groups[2].Value);
                }
                else if (line.EndsWith(":", StringComparison.Ordinal) && line.Length <= 60 && !line.StartsWith("ДИАЛОГ", StringComparison.OrdinalIgnoreCase) && !line.StartsWith("Вариант", StringComparison.Ordinal))
                {
                    // Реплика разбита на два абзаца: «Представитель кадровой службы:» и «текст».
                    pendingSpeaker = line.TrimEnd(':').Trim();
                }
                else if (line.StartsWith("«", StringComparison.Ordinal) && pendingSpeaker != null)
                {
                    data.recruiterRole = pendingSpeaker;
                    current.text = Unquote(line);
                    pendingSpeaker = null;
                }
            }
        }

        FlushQuestion();
        data.questions = questions.ToArray();

        CompanyHint(data);
        return data;
    }

    private static void CompanyHint(InterviewData data)
    {
        string company = data.companyId;
        CompanyData catalogCompany = InterviewCatalog.GetCompany(data.companyId);

        if (catalogCompany != null)
        {
            company = string.IsNullOrEmpty(catalogCompany.shortTitle) ? catalogCompany.title : catalogCompany.shortTitle;
        }

        data.title = company + " — " + data.position;
    }

    /// <summary>Не затираем ручные правки привязки в уже существующем JSON.</summary>
    private static void MergeManualFields(InterviewData data)
    {
        string path = Path.Combine(OutputFolder, data.id + ".json");

        if (!File.Exists(path))
        {
            return;
        }

        InterviewData existing = JsonUtility.FromJson<InterviewData>(JsonSanitizer.Sanitize(File.ReadAllText(path)));

        if (existing == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(existing.companyId))
        {
            data.companyId = existing.companyId;
        }

        if (existing.faculties != null && existing.faculties.Length > 0)
        {
            data.faculties = existing.faculties;
        }

        if (!string.IsNullOrEmpty(existing.courseRecommendation))
        {
            data.courseRecommendation = existing.courseRecommendation;
        }

        CompanyHint(data);
    }

    private static string ExtractRecommendations(string line)
    {
        int start = line.IndexOf("(например,", StringComparison.Ordinal);

        if (start >= 0)
        {
            int end = line.LastIndexOf(')');
            string inner = end > start ? line.Substring(start + "(например,".Length, end - start - "(например,".Length) : line.Substring(start + "(например,".Length);
            return Capitalize(inner.Trim().TrimEnd('.', ';'));
        }

        return Capitalize(AfterColon(line).TrimStart('-', ' ').TrimEnd('.', ';'));
    }

    private static Quality ToQuality(string word)
    {
        switch (word.ToLowerInvariant())
        {
            case "сильный":
                return Quality.strong;
            case "средний":
                return Quality.medium;
            default:
                return Quality.weak;
        }
    }

    private static char NormalizeLetter(string letter)
    {
        switch (letter.ToUpperInvariant())
        {
            case "А":
                return 'A';
            case "В":
                return 'B';
            case "С":
                return 'C';
            default:
                return letter.ToUpperInvariant()[0];
        }
    }

    private static string AfterColon(string line)
    {
        int index = line.IndexOf(':');
        return index >= 0 ? line.Substring(index + 1).Trim() : line.Trim();
    }

    /// <summary>Убирает внешние «ёлочки» (и висящую точку после закрывающей).</summary>
    private static string Unquote(string text)
    {
        text = text.Trim();

        if (text.EndsWith("».", StringComparison.Ordinal))
        {
            text = text.Substring(0, text.Length - 1);
        }

        while (text.StartsWith("«", StringComparison.Ordinal))
        {
            text = text.Substring(1).TrimStart();
        }

        while (text.EndsWith("»", StringComparison.Ordinal) && CountChar(text, '»') > CountChar(text, '«'))
        {
            text = text.Substring(0, text.Length - 1).TrimEnd();
        }

        return text.Trim();
    }

    private static int CountChar(string text, char value)
    {
        int count = 0;

        foreach (char c in text)
        {
            if (c == value)
            {
                count++;
            }
        }

        return count;
    }

    private static string Capitalize(string text)
    {
        text = text.Trim();
        return text.Length == 0 ? text : char.ToUpper(text[0]) + text.Substring(1);
    }

    private static string CollapseSpaces(string text)
    {
        return Regex.Replace(text, @"\s+", " ");
    }
}
