using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Переиспользуемые билдеры UI-элементов uGUI (панели, текст, кнопки, лейаут-хелперы).
/// Все размеры/цвета берутся из Theme, спрайты — из SpriteFactory.
/// </summary>
public static class UIFactory
{
    public static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    /// Залитая панель. radius &lt;=0 — plain-белый спрайт (Simple), иначе скруглённый 9-slice.
    public static Image Panel(Transform parent, string name, Color color, int radius)
    {
        var rt = Rect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        if (radius <= 0) { img.sprite = SpriteFactory.White; img.type = Image.Type.Simple; }
        else { img.sprite = SpriteFactory.Rounded(radius); img.type = Image.Type.Sliced; }
        img.color = color;
        return img;
    }

    /// Панель с градиентом (primary-&gt;secondary), спрайт уже с цветом, tint = белый.
    public static Image PanelGradient(Transform parent, string name, Color a, Color b, int radius)
    {
        var rt = Rect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = SpriteFactory.GradientRounded(radius, a, b);
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        return img;
    }

    public static TextMeshProUGUI Text(Transform parent, string name, string content, int size, Color color,
        TextAlignmentOptions align = TextAlignmentOptions.TopLeft, FontStyles style = FontStyles.Normal)
    {
        var rt = Rect(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.fontStyle = style;
        t.enableWordWrapping = true;
        t.richText = true;
        t.raycastTarget = false;
        return t;
    }

    public static Button Button(Transform parent, string name, string label, int size,
        Color bg, Color fg, int radius = Theme.R_Button, int minHeight = 0)
    {
        var img = Panel(parent, name, bg, radius);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.None;
        var t = Text(img.transform, "Label", label, size, fg, TextAlignmentOptions.Center, FontStyles.Normal);
        Stretch(t.rectTransform, 16, 0);
        if (minHeight > 0) LE(img, minH: minHeight);
        return btn;
    }

    // --- Лейаут-хелперы ---
    public static LayoutElement LE(Component c, float w = -1, float h = -1, float flexW = -1,
        float flexH = -1, float minW = -1, float minH = -1)
    {
        var le = c.gameObject.GetComponent<LayoutElement>();
        if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
        if (w >= 0) le.preferredWidth = w;
        if (h >= 0) le.preferredHeight = h;
        if (flexW >= 0) le.flexibleWidth = flexW;
        if (flexH >= 0) le.flexibleHeight = flexH;
        if (minW >= 0) le.minWidth = minW;
        if (minH >= 0) le.minHeight = minH;
        return le;
    }

    public static void Stretch(RectTransform rt, float padX = 0, float padY = 0)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padX, padY);
        rt.offsetMax = new Vector2(-padX, -padY);
    }

    /// На всю ширину/высоту родителя, но с отступом сверху (top может быть отрицательным).
    public static void Fill(RectTransform rt, float padX = 0, float top = 0)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padX, padX);
        rt.offsetMax = new Vector2(-padX, top);
    }

    // --- Группы лейаута / авто-размер ---
    public static VerticalLayoutGroup VList(Component c, int spacing = 0, int pad = 0,
        TextAnchor align = TextAnchor.UpperLeft, bool ctrlW = true, bool ctrlH = true,
        bool expW = false, bool expH = false)
    {
        var g = c.gameObject.AddComponent<VerticalLayoutGroup>();
        g.spacing = spacing;
        g.padding = new RectOffset(pad, pad, pad, pad);
        g.childAlignment = align;
        g.childControlWidth = ctrlW; g.childControlHeight = ctrlH;
        g.childForceExpandWidth = expW; g.childForceExpandHeight = expH;
        return g;
    }

    public static HorizontalLayoutGroup HList(Component c, int spacing = 0, int pad = 0,
        TextAnchor align = TextAnchor.MiddleLeft, bool ctrlW = true, bool ctrlH = true,
        bool expW = false, bool expH = false)
    {
        var g = c.gameObject.AddComponent<HorizontalLayoutGroup>();
        g.spacing = spacing;
        g.padding = new RectOffset(pad, pad, pad, pad);
        g.childAlignment = align;
        g.childControlWidth = ctrlW; g.childControlHeight = ctrlH;
        g.childForceExpandWidth = expW; g.childForceExpandHeight = expH;
        return g;
    }

    public static ContentSizeFitter Fit(Component c, bool w = false, bool h = true)
    {
        var f = c.gameObject.GetComponent<ContentSizeFitter>();
        if (f == null) f = c.gameObject.AddComponent<ContentSizeFitter>();
        f.horizontalFit = w ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        f.verticalFit = h ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        return f;
    }

    /// Логотип "ФУ" (квадрат-mark + две строки текста). Возвращает корневой RectTransform.
    public static RectTransform Logo(Transform parent, float markSize = 40)
    {
        var root = Rect("Logo", parent);
        HList(root, 12, 0, TextAnchor.MiddleLeft, true, true, false, false);
        var mark = Panel(root, "LogoMark", Theme.Primary, Theme.R_Logo);
        LE(mark, markSize, markSize);
        var markT = Text(mark.transform, "Mark", "ФУ", 14, Theme.White, TextAlignmentOptions.Center, FontStyles.Bold);
        Stretch(markT.rectTransform);
        var txt = Rect("LogoText", root);
        VList(txt, 0, 0, TextAnchor.UpperLeft, true, true, false, false);
        Text(txt, "Name", "Финуниверситет", Theme.H3, Theme.Primary, TextAlignmentOptions.TopLeft, FontStyles.Bold);
        Text(txt, "Sub", "Симулятор собеседований", 11, Theme.TextSecondary, TextAlignmentOptions.TopLeft, FontStyles.Normal);
        return root;
    }

    /// Вертикальный ScrollRect. content закреплён сверху, ширина = родителю, высота авто (ContentSizeFitter).
    public static RectTransform ScrollView(Transform parent, string name)
    {
        var root = Rect(name, parent);
        var scroll = root.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        var vp = Rect("Viewport", root);
        Stretch(vp);
        var vpImg = vp.gameObject.AddComponent<Image>();
        vpImg.color = new Color(1f, 1f, 1f, 0.001f);
        var mask = vp.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = false;
        scroll.viewport = vp;

        var content = Rect("Content", vp);
        content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = new Vector2(0f, 0f); content.offsetMax = new Vector2(0f, 0f);
        Fit(content, false, true);
        scroll.content = content;
        return content;
    }
}
