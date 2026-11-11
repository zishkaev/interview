using System.Collections.Generic;
using System.Text;
using Interview;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Итоговый отчёт (шаг 8 ТЗ): общий балл 0–100, разбор по этапам,
/// сильные стороны и зоны роста (выдержки из обратной связи на ответы),
/// рекомендации курсов и программ ДПО.
/// </summary>
public class ReportUI : MonoBehaviour
{
    public InterviewSession session;

    [Header("Шапка")]
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI scoreText;
    public Image scoreRing;
    public TextMeshProUGUI verdictText;

    [Header("Разбор по этапам")]
    public StageCardView[] stageCards;

    [Header("Сильные стороны / зоны роста")]
    public CardView strengthsCard;
    public CardView growthCard;

    [Header("Рекомендации")]
    public CardView recommendationTemplate;
    public RectTransform recommendationsRoot;

    [Header("Прокрутка")]
    public RectTransform layoutRoot;
    public ScrollRect scrollRect;

    private readonly List<GameObject> spawned = new List<GameObject>();

    private void Awake()
    {
        if (recommendationTemplate)
        {
            recommendationTemplate.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Build();
    }

    public void Build()
    {
        if (session == null || session.Data == null)
        {
            Debug.LogWarning(InterviewLoader.LogPrefix + " отчёт: сессия не загружена.");
            return;
        }

        InterviewData vacancy = session.Data;
        CompanyData company = InterviewCatalog.GetCompany(vacancy.companyId);
        int percent = session.Percent;

        // ---- шапка ----
        if (subtitleText)
        {
            string companyName = company != null ? company.title : string.Empty;
            subtitleText.text = (string.IsNullOrEmpty(companyName) ? string.Empty : companyName + " · ") + vacancy.DisplayPosition +
                                "\nРекрутер благодарит вас за прохождение. Система оценила качество ответов и сформировала итоговый отчёт.";
        }

        if (scoreText)
        {
            scoreText.text = percent.ToString();
        }

        if (scoreRing)
        {
            scoreRing.type = Image.Type.Filled;
            scoreRing.fillMethod = Image.FillMethod.Radial360;
            scoreRing.fillOrigin = (int)Image.Origin360.Top;
            scoreRing.fillClockwise = true;
            scoreRing.fillAmount = percent / 100f;
        }

        if (verdictText)
        {
            verdictText.text = InterviewText.Verdict(percent) + "\nОбщий балл — сумма баллов за этапы: " + PointsSum() +
                               " = " + session.Points + " из " + session.MaxPoints + " (сильный ответ — 2, средний — 1, слабый — 0).";
        }

        // ---- этапы ----
        for (int i = 0; i < stageCards.Length; i++)
        {
            bool visible = i < session.QuestionCount;
            stageCards[i].gameObject.SetActive(visible);

            if (visible)
            {
                stageCards[i].Set(session.GetQuestion(i), session.GetResult(i));
            }
        }

        // ---- сильные стороны / зоны роста ----
        var strengths = new StringBuilder();
        var growth = new StringBuilder();

        for (int i = 0; i < session.QuestionCount; i++)
        {
            QuestionData question = session.GetQuestion(i);
            AnswerResult result = session.GetResult(i);
            string stage = "<b>" + InterviewText.StageName(question) + "</b>";

            if (!result.isValid)
            {
                AppendLine(growth, stage + " — вопрос остался без ответа.");
            }
            else if (result.quality == Quality.strong)
            {
                AppendLine(strengths, stage + " — " + result.comment);
            }
            else
            {
                string label = "<color=" + InterviewText.Hex(InterviewText.QualityColor(result.quality)) + ">" +
                               InterviewText.QualityLabel(result.quality).ToLowerInvariant() + "</color>";
                AppendLine(growth, stage + " (" + label + ") — " + result.comment);
            }
        }

        if (strengthsCard)
        {
            strengthsCard.Set("Сильные стороны", strengths.Length > 0
                ? strengths.ToString()
                : "Пока ни один этап не пройден на сильный ответ. Посмотрите зоны роста и попробуйте ещё раз.");
        }

        if (growthCard)
        {
            growthCard.Set("Зоны роста", growth.Length > 0
                ? growth.ToString()
                : "Все этапы пройдены на сильный ответ — так держать!");
        }

        // ---- рекомендации ----
        BuildRecommendations(vacancy);

        RefreshLayout();
    }

    private void BuildRecommendations(InterviewData vacancy)
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            Destroy(spawned[i]);
        }

        spawned.Clear();

        if (recommendationTemplate == null || recommendationsRoot == null)
        {
            return;
        }

        // Самопрезентация и стресс-кейс — первый и третий этапы сценария.
        Quality selfPresentation = QualityAt(0);
        Quality stress = QualityAt(2);
        Quality professional = QualityAt(1);

        AddRecommendation("Курс «Твоя успешная карьера»",
            "Для улучшения общего понимания рынка труда и навыков трудоустройства. Поможет выстроить стратегию поиска работы и подготовиться к реальным собеседованиям.",
            "Карьера · Рынок труда");

        // Все три рекомендации из сценария (ТЗ, шаг 8) показываются всегда;
        // по фактическим ответам добавляется пояснение, на что обратить внимание.
        var reasons = new List<string>();

        if (selfPresentation != Quality.strong)
        {
            reasons.Add("самопрезентация");
        }

        if (stress != Quality.strong)
        {
            reasons.Add("стресс-кейс");
        }

        AddRecommendation("Курс «Самопрезентация»",
            "Для отработки навыков эффективного представления себя и прохождения стресс-интервью." +
            (reasons.Count > 0 ? " Особенно рекомендуем: есть зоны роста — " + string.Join(", ", reasons) + "." : string.Empty),
            "Soft skills · Самопрезентация");

        string dpo = "Платные программы дополнительного профессионального образования от Центра карьеры для углублённой профессиональной подготовки по востребованным направлениям.";

        if (!string.IsNullOrEmpty(vacancy.recommendations))
        {
            dpo += "\n<b>Для этой вакансии:</b> " + LowerFirst(vacancy.recommendations) + ".";
        }

        if (professional != Quality.strong)
        {
            dpo += "\nОсобенно полезно: кейс-задача на профессиональные компетенции пройдена не на максимум.";
        }

        AddRecommendation("Программы ДПО от Центра карьеры", dpo, "ДПО · Профессиональный рост");
    }

    /// <summary>«2 + 1 + 0 + 2» — баллы этапов по порядку.</summary>
    private string PointsSum()
    {
        var parts = new List<string>();

        for (int i = 0; i < session.QuestionCount; i++)
        {
            AnswerResult result = session.GetResult(i);
            parts.Add((result.isValid ? QualityUtil.Points(result.quality) : 0).ToString());
        }

        return string.Join(" + ", parts);
    }

    private Quality QualityAt(int index)
    {
        AnswerResult result = session.GetResult(index);
        return result.isValid ? result.quality : Quality.weak;
    }

    private void AddRecommendation(string title, string description, string tag)
    {
        CardView card = Instantiate(recommendationTemplate, recommendationsRoot);
        card.gameObject.SetActive(true);
        card.Set(title, description, null, tag);
        spawned.Add(card.gameObject);
    }

    private void RefreshLayout()
    {
        if (layoutRoot)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        }

        if (scrollRect)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private static void AppendLine(StringBuilder builder, string line)
    {
        if (builder.Length > 0)
        {
            builder.Append("\n\n");
        }

        builder.Append("• ").Append(line);
    }

    private static string LowerFirst(string text)
    {
        return string.IsNullOrEmpty(text) ? text : char.ToLower(text[0]) + text.Substring(1);
    }
}
