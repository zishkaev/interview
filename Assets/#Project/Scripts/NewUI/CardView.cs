using TMPro;
using UnityEngine;

/// <summary>Тексты карточки выбора (уровень / факультет / компания / вакансия).</summary>
public class CardView : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    [Tooltip("Доп. строка (например, «ВТБ · 4 этапа»). Необязательно.")]
    public TextMeshProUGUI metaText;
    [Tooltip("Текст плашки (например, «Топ-партнёр»). Необязательно.")]
    public TextMeshProUGUI tagText;
    [Tooltip("Объект плашки — скрывается, если тега нет.")]
    public GameObject tagRoot;

    public void Set(string title, string description, string meta = null, string tag = null)
    {
        SetText(titleText, title);
        SetText(descriptionText, description);
        SetText(metaText, meta);
        SetText(tagText, tag);

        if (tagRoot)
        {
            tagRoot.SetActive(!string.IsNullOrEmpty(tag));
        }
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target)
        {
            target.text = value ?? string.Empty;
        }
    }
}
