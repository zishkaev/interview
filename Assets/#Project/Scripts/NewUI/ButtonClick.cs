using UnityEngine;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{
    public Button button;
    private MenuUI menuUI;

    void Start()
    {
        button.onClick.AddListener(Click);
    }

    public void SetMenu(MenuUI menuUI)
    {
        this.menuUI = menuUI;
    }

    private void Click()
    {
        // Кнопка не зарегистрирована в MenuUI (не prev/next какой-либо страницы) — ничего не делаем.
        if (menuUI != null)
        {
            menuUI.Select(this);
        }
    }

    void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
    }
}
