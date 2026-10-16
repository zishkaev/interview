using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public List<PageUI> pages;

    void Start()
    {
        pages[0].Show(true);
        for (int i = 1; i < pages.Count; i++)
        {
            pages[i].Show(false);
        }
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i].buttonPrev)
                pages[i].buttonPrev.SetMenu(this);
            if (pages[i].buttonNext)
                pages[i].buttonNext.SetMenu(this);
        }
    }

    public void Select(ButtonClick buttonClick)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i].buttonPrev == buttonClick)
            {
                if (i - 1 < 0)
                    return;
                pages[i].Show(false);
                pages[i - 1].Show(true);
                break;
            }
            if (pages[i].buttonNext == buttonClick)
            {
                if (pages[i].itemListUI)
                {
                    if (pages[i].itemListUI.ItemSelected() == false)
                        return;
                }
                if (i + 1 >= pages.Count)
                    return;
                pages[i].Show(false);
                pages[i + 1].Show(true);
                break;
            }
        }
    }

    public void GoToPage(int id)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].Show(false);
            if (id == i)
                pages[i].Show(true);
        }
    }

    public void GoToPage(PageUI pageUI)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].Show(false);
        }
        pageUI.Show(true);
    }
}
