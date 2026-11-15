using UnityEngine;

// Círculo y anillo dibujados en código (con borde suave) para diagramas de la UI.
public static class ShapeArt
{
    static Sprite circle, ring;

    public static Sprite Circle => circle != null ? circle : circle = Make(1f, 0f);
    public static Sprite Ring => ring != null ? ring : ring = Make(1f, 0.96f);

    // outer e inner en fracción del radio: inner 0 = disco lleno.
    static Sprite Make(float outer, float inner)
    {
        const int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
            float a = Mathf.Clamp01((outer - d) * r) * (inner > 0f ? Mathf.Clamp01((d - inner) * r) : 1f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
