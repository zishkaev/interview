using Interview;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Карточка этапа в «Разборе по блокам»: название, оценка, полоска, цель этапа.</summary>
public class StageCardView : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI qualityText;
    [Tooltip("Заливка полоски. Ширина задаётся якорями (0..доля).")]
    public RectTransform barFill;
    public Image barFillImage;
    public TextMeshProUGUI goalText;

    public void Set(QuestionData question, AnswerResult result)
    {
        if (titleText)
        {
            titleText.text = InterviewText.StageName(question);
        }

        Quality quality = result.isValid ? result.quality : Quality.weak;
        Color color = result.isValid ? InterviewText.QualityColor(quality) : Color.gray;

        if (qualityText)
        {
            int points = result.isValid ? QualityUtil.Points(quality) : 0;
            qualityText.text = (result.isValid ? InterviewText.QualityLabel(quality) : "Нет ответа") +
                               " · " + points + " из " + QualityUtil.MaxPoints + " б.";
            qualityText.color = color;
        }

        float ratio = result.isValid ? (float)QualityUtil.Points(quality) / QualityUtil.MaxPoints : 0f;

        if (barFill)
        {
            // Слабый ответ — небольшая полоска, чтобы цвет было видно.
            barFill.anchorMin = new Vector2(0f, 0f);
            barFill.anchorMax = new Vector2(Mathf.Max(ratio, 0.08f), 1f);
            barFill.offsetMin = Vector2.zero;
            barFill.offsetMax = Vector2.zero;
        }

        if (barFillImage)
        {
            barFillImage.color = color;
        }

        if (goalText)
        {
            goalText.text = question != null && !string.IsNullOrEmpty(question.goal) ? question.goal : string.Empty;
        }
    }
}
