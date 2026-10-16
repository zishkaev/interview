using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemListUI : MonoBehaviour
{
    public List<ItemUI> items;
    public Action<int> OnSelectItem;

    void Start()
    {
        SetList(items);
    }

    void OnEnable()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Select(false);
        }
    }

    public void SetList(List<ItemUI> itemUIs)
    {
        items = itemUIs;
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Select(false);
            items[i].SetList(this);
        }
    }

    public void SetItem(ItemUI itemUI)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Select(false);
        }
        itemUI.Select(true);
        OnSelectItem?.Invoke(items.IndexOf(itemUI));
    }

    public bool ItemSelected()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].IsSelected)
                return true;
        }
        return false;
    }
}
