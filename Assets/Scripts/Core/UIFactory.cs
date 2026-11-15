using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Crea elementos de UI con el estilo del juego (paneles grises biselados, texto verde fósforo).
// Lo usan los constructores de escenas (editor) y la UI que se arma en tiempo de ejecución.
public static class UIFactory
{
    public static Sprite Sprite; // sprite biselado de Unity (UISprite), lo asigna quien arma la UI

    public static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5; // UI
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    public static void Stretch(RectTransform rt, float padding = 0f)
    {
        Place(rt, Vector2.zero, Vector2.one, new Vector2(padding, padding), new Vector2(-padding, -padding));
    }

    public static Image Panel(string name, Transform parent, Color color, bool outline = true)
    {
        var rt = Rect(name, parent);
        var image = rt.gameObject.AddComponent<Image>();
        image.sprite = Sprite;
        image.type = Sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        if (outline)
        {
            var o = rt.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.6f);
            o.effectDistance = new Vector2(2f, -2f);
        }
        return image;
    }

    public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color,
        TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, bool bold = false)
    {
        var rt = Rect(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) t.font = TMP_Settings.defaultFontAsset;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        if (bold) t.fontStyle = FontStyles.Bold;
        return t;
    }

    public static Button Button(string name, Transform parent, string label, Color background, Color labelColor, float size = 22f, bool bold = false)
    {
        var image = Panel(name, parent, background);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.82f, 1f, 0.86f);
        colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.65f, 0.75f, 0.68f);
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.7f);
        button.colors = colors;
        if (!string.IsNullOrEmpty(label))
        {
            var t = Text("Label", image.transform, label, size, labelColor, TextAlignmentOptions.Center, bold);
            Stretch(t.rectTransform, 4f);
        }
        return button;
    }

    public static LayoutElement Layout(GameObject go, float preferredHeight = -1f, float preferredWidth = -1f,
        float flexibleWidth = -1f, float flexibleHeight = -1f)
    {
        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;
        le.preferredWidth = preferredWidth;
        le.flexibleWidth = flexibleWidth;
        le.flexibleHeight = flexibleHeight;
        return le;
    }
}
