using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour, IPointerDownHandler
{
    public Image image;
    public Color baseColor, selectColor;
    public GameObject icon;
    private ItemListUI itemListUI;
    public bool IsSelected;

    public void OnPointerDown(PointerEventData eventData)
    {
        itemListUI.SetItem(this);
    }

    public void Select(bool state)
    {
        if (icon)
            icon.SetActive(state);
        image.color = state ? selectColor : baseColor;
        IsSelected = state;
    }

    public void SetList(ItemListUI itemListUI)
    {
        this.itemListUI = itemListUI;
    }

    void OnValidate()
    {
        if (image == null)
            image = GetComponent<Image>();
    }
}
