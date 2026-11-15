using UnityEngine;
using UnityEngine.UI;

public enum FontKind { Title, Body, Mono }

// Arma elementos de UI con la guía de estética: tarjetas redondeadas con borde fino, sombra
// y barra de color a la izquierda; textos con Text común y las fuentes del juego.
// Lo usan los constructores de escenas (editor) y la UI que se arma en tiempo de ejecución.
public static class UIFactory
{
    public const float Margin = 36f;
    public const float AccentWidth = 5f;

    public static Font TitleFont, BodyFont, MonoFont;
    public static Sprite RoundedFill, RoundedOutline, SoftShadow;

    public static Font FontFor(FontKind kind)
    {
        var f = kind == FontKind.Title ? TitleFont : kind == FontKind.Mono ? MonoFont : BodyFont;
        return f != null ? f : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

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

    public static void Stretch(RectTransform rt, float padding = 0f) =>
        Place(rt, Vector2.zero, Vector2.one, new Vector2(padding, padding), new Vector2(-padding, -padding));

    // Ancla una caja de tamaño fijo en una esquina/borde (pivot = anchor).
    public static void Pin(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    public static Image Image(string name, Transform parent, Sprite sprite, Color color, bool raycast = false)
    {
        var rt = Rect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.type = sprite != null ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    public static Text Label(string name, Transform parent, string text, int size, Color color,
        FontKind kind = FontKind.Body, TextAnchor align = TextAnchor.MiddleLeft)
    {
        var rt = Rect(name, parent);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = FontFor(kind);
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.text = text;
        return t;
    }

    // Etiqueta chica en mayúsculas (15 px, color secundario).
    public static Text Caption(string name, Transform parent, string text) =>
        Label(name, parent, text.ToUpperInvariant(), 15, Palette.TextSecondary, FontKind.Body);

    // Tarjeta: sombra difusa, relleno al 84%, borde fino y barra de color opcional a la izquierda.
    // Los hijos decorativos ignoran el layout, así la tarjeta puede tener un LayoutGroup propio.
    public static RectTransform Card(string name, Transform parent, Color? accent = null, bool shadow = true)
    {
        var root = Rect(name, parent);
        if (shadow && SoftShadow != null)
        {
            var s = Image("Shadow", root, SoftShadow, new Color(0f, 0f, 0f, 0.5f));
            Place(s.rectTransform, Vector2.zero, Vector2.one, new Vector2(-32f, -38f), new Vector2(32f, 26f));
            IgnoreLayout(s.gameObject);
        }
        var fill = Image("Fill", root, RoundedFill, Palette.CardBackground, true);
        Stretch(fill.rectTransform);
        IgnoreLayout(fill.gameObject);

        var border = Image("Border", root, RoundedOutline, Palette.CardBorder);
        Stretch(border.rectTransform);
        IgnoreLayout(border.gameObject);

        if (accent.HasValue)
        {
            var bar = Image("Accent", root, RoundedFill, accent.Value);
            bar.type = UnityEngine.UI.Image.Type.Simple;
            bar.sprite = null;
            Place(bar.rectTransform, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 18f), new Vector2(AccentWidth, -18f));
            IgnoreLayout(bar.gameObject);
        }
        return root;
    }

    // Botón = tarjeta con texto naranja en negrita.
    public static Button ButtonCard(string name, Transform parent, string label, int size = 28, Color? labelColor = null,
        Color? accent = null, bool shadow = true)
    {
        var card = Card(name, parent, accent, shadow);
        var button = card.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = card.Find("Fill").GetComponent<Image>();

        var text = Label("Label", card, label, size, labelColor ?? Palette.Orange, FontKind.Title, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform, 6f);

        var fx = card.gameObject.AddComponent<CardButtonFx>();
        fx.button = button;
        fx.border = card.Find("Border").GetComponent<Image>();
        fx.label = text;
        fx.labelColor = labelColor ?? Palette.Orange;
        fx.borderColor = Palette.CardBorder;
        return button;
    }

    public static UIAppear Appear(GameObject go, float delay = 0f)
    {
        if (go.GetComponent<CanvasGroup>() == null) go.AddComponent<CanvasGroup>();
        var a = go.GetComponent<UIAppear>();
        if (a == null) a = go.AddComponent<UIAppear>();
        a.delay = delay;
        return a;
    }

    public static void IgnoreLayout(GameObject go)
    {
        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
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

    public static VerticalLayoutGroup Vertical(GameObject go, int left, int right, int top, int bottom, float spacing)
    {
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(left, right, top, bottom);
        v.spacing = spacing;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        return v;
    }
}
