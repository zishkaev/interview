using TMPro;
using UnityEngine;

public class BlockText : MonoBehaviour
{
    public TextMeshProUGUI leftText, rightText;
    public GameObject leftBlock, rightBlock;
    public RectTransform rect;

    public void SetSide(bool isAnswer)
    {
        leftBlock.SetActive(isAnswer == false);
        rightBlock.SetActive(isAnswer == true);
    }

    public void SetText(string text)
    {
        if (leftBlock.activeInHierarchy)
        {
            leftText.text = text;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, leftText.preferredHeight);
        }
        else if (rightBlock.activeInHierarchy)
        {
            rightText.text = text;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, rightText.preferredHeight);
        }
    }
}
