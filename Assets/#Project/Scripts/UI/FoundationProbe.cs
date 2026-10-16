using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Этап 0 (фундамент). Строит скелет интерфейса из Прототип_симулятора.html:
/// Header / ProgressBar / Main(Scroll) / Footer + экран-0 (hero + features).
/// [ExecuteAlways] — чтобы скелет был виден и в редакторе, и в Play.
/// Всё собирается кодом из Theme / SpriteFactory / UIFactory.
/// </summary>
[ExecuteAlways]
public class FoundationProbe : MonoBehaviour
{
    static readonly Color LinkLight = new Color(1f, 1f, 1f, 0.70f);
    static readonly Color LinkDim   = new Color(1f, 1f, 1f, 0.50f);
    static readonly Color Transparent = new Color(1f, 1f, 1f, 0f);

    RectTransform progress;

    void Awake() { Build(); }

    void Build()
    {
        if (transform.childCount > 0) return; // защита от дублей при рекомпиле
        var root = (RectTransform)transform;
        UIFactory.VList(transform, 0, 0, TextAnchor.UpperCenter, true, true, false, false);

        BuildHeader(root);
        BuildProgress(root);
        BuildMain(root);
        BuildHome();
        BuildFooter(root);

        if (progress != null) progress.gameObject.SetActive(false); // на главной прогресс скрыт
    }

    // Центрированный контейнер фиксированной ширины (аналог max-width + margin auto).
    static RectTransform Centered(RectTransform parent, string name, float width)
    {
        var rt = UIFactory.Rect(name, parent);
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, 0f);
        return rt;
    }

    void BuildHeader(RectTransform root)
    {
        var header = UIFactory.Rect("Header", root);
        header.gameObject.AddComponent<Image>().color = Theme.BgMain;
        UIFactory.LE(header, minH: 72);

        var line = UIFactory.Rect("Border", header);
        line.anchorMin = new Vector2(0, 0); line.anchorMax = new Vector2(1, 0);
        line.pivot = new Vector2(0.5f, 0f); line.sizeDelta = new Vector2(0, 1);
        line.gameObject.AddComponent<Image>().color = Hex("EAECEF");

        var inner = Centered(header, "HeaderInner", 1360);
        UIFactory.HList(inner, 24, 32, TextAnchor.MiddleLeft, false, true, false, false);
        UIFactory.Logo(inner);

        var nav = UIFactory.Rect("HeaderNav", inner);
        UIFactory.LE(nav, flexW: 1f);
        UIFactory.HList(nav, 24, 0, TextAnchor.MiddleRight, false, true, false, false);
        NavLink(nav, "Главная", true);
        NavLink(nav, "О сервисе");
        NavLink(nav, "Партнёрам");
        NavLink(nav, "Контакты");
    }

    void NavLink(RectTransform parent, string label, bool active = false)
    {
        UIFactory.Button(parent, label, label, Theme.Small, Transparent,
            active ? Theme.Accent : Theme.TextPrimary, 0, 0);
    }

    void BuildProgress(RectTransform root)
    {
        progress = UIFactory.Rect("ProgressBar", root);
        progress.gameObject.AddComponent<Image>().color = Theme.BgSecondary;
        UIFactory.LE(progress, minH: 30);

        var inner = Centered(progress, "ProgressInner", 1360);
        UIFactory.HList(inner, 16, 32, TextAnchor.MiddleLeft, false, true, false, false);
        UIFactory.Text(inner, "ProgressLabel", "Шаг 1 из 6", Theme.Xs, Theme.TextSecondary,
            TextAlignmentOptions.MidlineLeft, FontStyles.Bold);

        var steps = UIFactory.Rect("ProgressSteps", inner);
        UIFactory.LE(steps, flexW: 1f);
        UIFactory.HList(steps, 8, 0, TextAnchor.MiddleLeft, true, false, false, false);
        for (int i = 0; i < 6; i++)
        {
            var step = UIFactory.Panel(steps, "Step", Theme.Border, 3);
            UIFactory.LE(step, flexW: 1f, h: 6f);
        }
    }

    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

    RectTransform screensHost;      // контейнер для экранов (переключение через SetActive)
    RectTransform homeScreen;       // screen-0

    // Область контента: скролл + фиксированный по ширине столбец 1360 (padding 64/80, как .main).
    void BuildMain(RectTransform root)
    {
        var main = UIFactory.Rect("Main", root);
        UIFactory.LE(main, flexW: 1f, flexH: 1f);          // занимает весь остаток по высоте
        UIFactory.Stretch(main);                           // тянется по ширине canvas

        var scroll = UIFactory.Rect("Scroll", main);
        UIFactory.Stretch(scroll);
        var sr = scroll.gameObject.AddComponent<ScrollRect>();
        sr.horizontal = false; sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 40f;

        var viewport = UIFactory.Rect("Viewport", scroll);
        UIFactory.Stretch(viewport);
        var vpImg = viewport.gameObject.AddComponent<Image>();
        vpImg.color = new Color(1f, 1f, 1f, 0.001f);
        var mask = viewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = false;
        sr.viewport = viewport;

        var content = UIFactory.Rect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = Vector2.zero; content.offsetMax = Vector2.zero;
        UIFactory.Fit(content, false, true);
        sr.content = content;

        // Внутренний столбец max-width 1360 + вертикальный паддинг экрана (.main padding: 64 32 80).
        var inner = Centered(content, "MainInner", 1360);
        UIFactory.VList(inner, 0, 0, TextAnchor.UpperCenter, true, true, false, false);
        UIFactory.LE(inner, h: -1);
        // Верх/низ паддинги столбца зададим LayoutElement'ами-распорками.
        Spacer(inner, 64);
        screensHost = UIFactory.Rect("Screens", inner);
        UIFactory.LE(screensHost, flexW: 1f);
        Spacer(inner, 80);
    }

    static void Spacer(RectTransform parent, float h)
    {
        var s = UIFactory.Rect("Pad", parent);
        UIFactory.LE(s, h: h);
    }

    void BuildFooter(RectTransform root)
    {
        var footer = UIFactory.Rect("Footer", root);
        footer.gameObject.AddComponent<Image>().color = Theme.BgMain;
        UIFactory.LE(footer, minH: 56);

        var top = UIFactory.Rect("Border", footer);
        top.anchorMin = new Vector2(0, 1); top.anchorMax = new Vector2(1, 1);
        top.pivot = new Vector2(0.5f, 1f); top.sizeDelta = new Vector2(0, 1);
        top.gameObject.AddComponent<Image>().color = Hex("EAECEF");

        var inner = Centered(footer, "FooterInner", 1360);
        UIFactory.HList(inner, 24, 32, TextAnchor.MiddleLeft, false, true, false, false);
        UIFactory.Text(inner, "Copy", "© 2026 Финуниверситет", Theme.Xs, Theme.TextSecondary,
            TextAlignmentOptions.MidlineLeft, FontStyles.Normal);

        var links = UIFactory.Rect("FooterLinks", inner);
        UIFactory.LE(links, flexW: 1f);
        UIFactory.HList(links, 24, 0, TextAnchor.MiddleRight, false, true, false, false);
        FooterLink(links, "Политика конфиденциальности");
        FooterLink(links, "Помощь");
    }

    void FooterLink(RectTransform parent, string label)
    {
        UIFactory.Button(parent, label, label, Theme.Small, Transparent,
            Theme.TextSecondary, 0, 0);
    }

    // screen-0: welcome-hero (текст + градиентная плашка с котом) + features (3 карточки).
    void BuildHome()
    {
        var screen = UIFactory.Rect("screen-0", screensHost);
        UIFactory.VList(screen, 0, 0, TextAnchor.UpperCenter, true, true, false, false);
        homeScreen = screen;

        // welcome-hero: две колонки 1fr 1fr, gap 64.
        var hero = UIFactory.Rect("WelcomeHero", screen);
        UIFactory.HList(hero, 64, 0, TextAnchor.MiddleLeft, true, true, false, false);
        UIFactory.LE(hero, flexW: 1f);

        // Левая колонка: текст (h1 margin-bottom 32, subtitle margin-bottom 40).
        var text = UIFactory.Rect("WelcomeText", hero);
        UIFactory.LE(text, flexW: 1f);
        UIFactory.VList(text, 0, 0, TextAnchor.UpperLeft, true, true, false, false);
        UIFactory.Text(text, "H1", "Симулятор собеседований", Theme.H1, Theme.Primary,
            TextAlignmentOptions.TopLeft, FontStyles.Bold);
        Spacer(text, 32);
        UIFactory.Text(text, "Subtitle", "Подготовьтесь к реальному интервью с ведущими компаниями-партнёрами Финуниверситета в безопасной среде",
            Theme.Subtitle, Theme.TextSecondary, TextAlignmentOptions.TopLeft, FontStyles.Normal);
        Spacer(text, 40);
        var cta = UIFactory.Button(text, "CTA", "Начать симуляцию →", Theme.Body,
            Theme.Primary, Theme.White, Theme.R_Button, 58);
        UIFactory.LE(cta, w: 280);

        // Правая колонка: градиентная плашка с котом-цитатой (padding 48, space-between).
        var visual = UIFactory.PanelGradient(hero, "WelcomeVisual",
            Theme.Primary, Theme.Secondary, Theme.R_Hero);
        UIFactory.LE(visual, flexW: 1f, h: 420);
        var vInner = UIFactory.Rect("VisualInner", visual.transform);
        UIFactory.VList(vInner, 0, 48, TextAnchor.UpperCenter, true, true, false, false);
        UIFactory.Stretch(vInner);
        UIFactory.Text(vInner, "Mascot", "🐱", 120, Theme.White, TextAlignmentOptions.Center, FontStyles.Normal);
        Spacer(vInner, 0); // заполняет пространство (аналог space-between)
        UIFactory.Text(vInner, "Quote", "«Мы не следуем привычным трендам, мы создаём новые»",
            20, Theme.White, TextAlignmentOptions.Center, FontStyles.Italic);

        // features: 3 карточки, gap 32, margin-top 80.
        Spacer(screen, 80);
        var features = UIFactory.Rect("Features", screen);
        UIFactory.HList(features, 32, 0, TextAnchor.UpperLeft, true, true, false, false);
        UIFactory.LE(features, flexW: 1f);
        FeatureCard(features, "🎯", "Реалистичные сценарии", "Диалоги с виртуальным рекрутером, максимально приближенные к реальным собеседованиям");
        FeatureCard(features, "🤝", "Компании-партнёры", "Сбер, ВТБ, Яндекс и другие ведущие работодатели. Выбирайте вакансию под свои интересы");
        FeatureCard(features, "📋", "Персональный отчёт", "Автоматическая оценка ответов и рекомендации по развитию карьерных навыков");
    }

    void FeatureCard(RectTransform parent, string icon, string title, string desc)
    {
        var card = UIFactory.Panel(parent, "FeatureCard", Theme.BgSecondary, Theme.R_Card);
        UIFactory.LE(card, flexW: 1f, h: 260);

        var inner = UIFactory.Rect("Inner", card.transform);
        UIFactory.VList(inner, 0, 32, TextAnchor.UpperLeft, true, true, false, false);
        UIFactory.Stretch(inner);

        var ic = UIFactory.Panel(inner, "Icon", Theme.Accent, Theme.R_Icon);
        UIFactory.LE(ic, 56, 56);
        var glyph = UIFactory.Text(ic.transform, "Glyph", icon, 24, Theme.White,
            TextAlignmentOptions.Center, FontStyles.Normal);
        UIFactory.Stretch(glyph.rectTransform);

        Spacer(inner, 20);                                   // .feature-icon margin-bottom 20
        UIFactory.Text(inner, "Title", title, Theme.H3, Theme.Primary, TextAlignmentOptions.TopLeft, FontStyles.Bold);
        Spacer(inner, 12);                                   // h3 margin-bottom 12
        UIFactory.Text(inner, "Desc", desc, Theme.Small, Theme.TextSecondary, TextAlignmentOptions.TopLeft, FontStyles.Normal);
    }
}
