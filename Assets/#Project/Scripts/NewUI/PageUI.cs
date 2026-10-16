using UnityEngine;

public class PageUI : MonoBehaviour
{
    public GameObject progress;
    public GameObject menu;
    public ButtonClick buttonPrev, buttonNext;
    public ItemListUI itemListUI;

    public void Show(bool state)
    {
        if (progress)
            progress.SetActive(state);
        if (menu)
            menu.SetActive(state);
    }

    void OnValidate()
    {
        if (itemListUI == null)
            itemListUI = GetComponent<ItemListUI>();
    }
}
