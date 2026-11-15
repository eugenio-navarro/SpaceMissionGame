using UnityEngine;

// Cursor de "manito" dibujado en código (pixel art 16x16 escalado al doble).
public static class CursorArt
{
    static readonly string[] HandRows =
    {
        "....XX..........",
        "...XooX.........",
        "...XooX.........",
        "...XooX.........",
        "...XooXXX.......",
        "...XooXooXXX....",
        "...XooXooXooXX..",
        ".XXXooXooXooXoX.",
        "XooXooooooooooX.",
        "XoooooooooooooX.",
        ".XooooooooooooX.",
        "..XoooooooooooX.",
        "..XooooooooooX..",
        "...XoooooooooX..",
        "....XooooooooX..",
        "....XXXXXXXXXX..",
    };

    public static Texture2D Hand()
    {
        const int scale = 2;
        int size = HandRows.Length * scale;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        var outline = Palette.Hex("#090E18");
        var fill = Palette.TextMain;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            char c = HandRows[y / scale][x / scale];
            // La textura empieza abajo; las filas del dibujo, arriba.
            tex.SetPixel(x, size - 1 - y, c == 'X' ? outline : c == 'o' ? fill : Color.clear);
        }
        tex.Apply();
        return tex;
    }
}
