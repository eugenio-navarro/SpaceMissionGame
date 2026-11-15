using UnityEngine;

// Paleta del README (sección 7) más algunos tonos de apoyo para la UI.
public static class Palette
{
    public static readonly Color Space = Hex("#0B0F18");
    public static readonly Color Metal = Hex("#8F98A3");
    public static readonly Color Orange = Hex("#F0782A");
    public static readonly Color Phosphor = Hex("#7BD88F");
    public static readonly Color Planet = Hex("#3F77C9");

    public static readonly Color Panel = Hex("#2B313B");
    public static readonly Color PanelDark = Hex("#1A1F28");
    public static readonly Color Selected = Hex("#1F3A28");
    public static readonly Color TextLight = Hex("#D5DAE0");
    public static readonly Color TextDisabled = Hex("#5F6670");
    public static readonly Color Red = Hex("#E0533D");
    public static readonly Color Yellow = Hex("#E8C547");

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var color);
        return color;
    }
}
