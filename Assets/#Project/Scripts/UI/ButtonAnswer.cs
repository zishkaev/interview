using System;
using Interview;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnswer : MonoBehaviour
{
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private float minHeight = 65;
    public Button button;
    public TextMeshProUGUI textMeshProUGUI;
    public Action<int> OnClick;
    public int id;

    void Start()
    {
        button.onClick.AddListener(Press);
    }

    public void SetID(int id)
    {
        this.id = id;
    }

    public void SetText(string text)
    {
        textMeshProUGUI.text = text;
        if (buttonRect) {
            float y = Mathf.Max( textMeshProUGUI.preferredHeight, minHeight);
            buttonRect.sizeDelta = new Vector2(buttonRect.sizeDelta.x, y);
        }
    }

    public void SetData(AnswerData answerData)
    {
        SetText(answerData.text);
    }

    private void Press()
    {
        OnClick?.Invoke(id);
    }
}
