using UnityEngine;

/// <summary>
/// Единый источник правды по палитре и типографике (значения из CSS :root оригинала).
/// UI-компоненты берут цвета/размеры только отсюда.
/// </summary>
public static class Theme
{
    public static readonly Color Primary       = Hex("#233E76");
    public static readonly Color Accent        = Hex("#0198AF");
    public static readonly Color Secondary     = Hex("#275E9F");
    public static readonly Color BgMain        = Hex("#FFFFFF");
    public static readonly Color BgSecondary   = Hex("#F3F3F3");
    public static readonly Color BgDark        = Hex("#193D57");
    public static readonly Color TextPrimary   = Hex("#020202");
    public static readonly Color TextSecondary = Hex("#9197A3");
    public static readonly Color Border        = Hex("#E0E4EC");
    public static readonly Color SelectedBg    = Hex("#F5F7FC");
    public static readonly Color AccentHover   = Hex("#017A8D");
    public static readonly Color White         = Color.white;

    // Типографика (px, при reference 1440x1024)
    public const int H1 = 42, H2 = 28, H3 = 18, Subtitle = 18, Body = 16, Small = 14, Xs = 13;

    // Радиусы скругления (px)
    public const int R_Logo = 6, R_Button = 8, R_Icon = 12, R_Card = 16, R_Hero = 24, R_Pill = 32;

    // Опорное разрешение для CanvasScaler
    public static readonly Vector2Int Reference = new Vector2Int(1440, 1024);

    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }
}
