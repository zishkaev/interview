using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Генерирует белые скруглённые спрайты (9-slice) и градиентные спрайты.
/// Решает проблему отсутствия border-radius / linear-gradient в uGUI.
/// Белые спрайты тонируются через Image.color; градиент — с запёкшимися цветами.
/// </summary>
public static class SpriteFactory
{
    static Sprite _white;
    public static Sprite White
    {
        get
        {
            if (_white == null)
            {
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
                _white.name = "white1x1";
            }
            return _white;
        }
    }

    static readonly Dictionary<int, Sprite> _rounded = new Dictionary<int, Sprite>();
    public static Sprite Rounded(int radius)
    {
        radius = Mathf.Max(1, radius);
        if (_rounded.TryGetValue(radius, out var s)) return s;

        int size = Mathf.Max(radius * 2 + 2, 8);
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        var mask = MakeRoundedMask(size, radius);
        tex.SetPixels(mask);
        tex.Apply();

        var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sp.name = "rounded_" + radius;
        _rounded[radius] = sp;
        return sp;
    }

    static readonly Dictionary<string, Sprite> _grad = new Dictionary<string, Sprite>();
    /// Градиент 135deg: цвет a слева-сверху -> b справа-снизу, в скруглённой маске (как welcome-visual).
    public static Sprite GradientRounded(int radius, Color a, Color b)
    {
        radius = Mathf.Max(1, radius);
        string key = radius + "|" + ColorUtility.ToHtmlStringRGB(a) + "|" + ColorUtility.ToHtmlStringRGB(b);
        if (_grad.TryGetValue(key, out var s)) return s;

        int size = Mathf.Max(radius * 2 + 2, 128);
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var mask = MakeRoundedMask(size, radius);
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float t = ((float)x / (size - 1) + (1f - (float)y / (size - 1))) * 0.5f;
                var c = Color.Lerp(a, b, t);
                c.a = mask[y * size + x].a;
                px[y * size + x] = c;
            }
        tex.SetPixels(px);
        tex.Apply();

        var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sp.name = "grad_" + key;
        _grad[key] = sp;
        return sp;
    }

    /// Маска скруглённого прямоугольника со сглаживанием по 1px. Возвращает белый цвет с альфой.
    static Color[] MakeRoundedMask(int size, int radius)
    {
        float half = size * 0.5f;
        float inner = half - radius;
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - half) - inner;
                float dy = Mathf.Abs(y + 0.5f - half) - inner;
                float ax = Mathf.Max(dx, 0f), ay = Mathf.Max(dy, 0f);
                float dist = Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(dx, dy), 0f) - radius;
                float alpha = Mathf.Clamp01(0.5f - dist);
                px[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        return px;
    }
}
