using UnityEngine;

// Colores de la guía de estética. El naranja es para lo activo o importante; el verde, para lo que está bien.
public static class Palette
{
    // UI
    public static readonly Color CardBackground = Hex("#090E18", 0.84f);
    public static readonly Color CardBorder = new Color(1f, 1f, 1f, 0.16f);
    public static readonly Color TextMain = Hex("#EDF5FF");
    public static readonly Color TextSecondary = Hex("#8FA3B8");
    public static readonly Color Dim = Hex("#4D5969");
    public static readonly Color Green = Hex("#7BD88F");
    public static readonly Color Orange = Hex("#F0782A");
    public static readonly Color Blue = Hex("#5AA3F0");

    // Mundo 3D
    public static readonly Color Metal = Hex("#8F98A3");
    public static readonly Color Yellow = Hex("#E8C547");
    public static readonly Color Planet = Hex("#3F77C9");

    public static Color Hex(string hex, float alpha = 1f)
    {
        ColorUtility.TryParseHtmlString(hex, out var color);
        color.a = alpha;
        return color;
    }

    public static string Tag(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
}
