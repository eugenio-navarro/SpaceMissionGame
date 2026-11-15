using System;
using UnityEngine;
using UnityEngine.UI;

// Ventana modal del centro espacial armada en tiempo de ejecución (mismo estilo que las del constructor):
// fondo oscuro que bloquea los clics, tarjeta centrada, etiqueta, título y botón X.
public static class HubModal
{
    public static RectTransform Create(string name, Transform root, Vector2 size, Color accent,
        string caption, string title, Action onClose, out RectTransform window)
    {
        var modal = UIFactory.Rect(name, root);
        UIFactory.Stretch(modal);
        var dim = modal.gameObject.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.5f);

        window = UIFactory.Card("Window", modal, accent);
        UIFactory.Pin(window, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        UIFactory.Appear(window.gameObject);

        var c = UIFactory.Caption("Caption", window, caption);
        UIFactory.Place(c.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(36f, -56f), new Vector2(-100f, -28f));
        var t = UIFactory.Label("Title", window, title, 40, Palette.TextMain, FontKind.Title);
        UIFactory.Place(t.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(36f, -108f), new Vector2(-100f, -54f));

        var close = UIFactory.ButtonCard("Close", window, "X", 26, null, null, false);
        UIFactory.Pin((RectTransform)close.transform, Vector2.one, new Vector2(-20f, -20f), new Vector2(56f, 56f));
        close.onClick.AddListener(() => onClose());
        return modal;
    }
}
