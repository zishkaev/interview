using UnityEngine;
using UnityEngine.UI;

public class ButtonMove : MonoBehaviour
{
    public Button button;
    public MenuUI menuUI;
    public PageUI page;

    void Start()
    {
        button.onClick.AddListener(Click);
    }

    private void Click()
    {
        menuUI.GoToPage(page);
    }

    void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
    }
}
