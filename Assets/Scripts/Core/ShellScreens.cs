using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantallas de GameShell armadas en tiempo de ejecución con la misma estética que el resto:
// menú de pausa y opciones, créditos y pantalla de título.
public static class ShellScreens
{
    const string GameTitle = "SPACE MISSION DESIGN";

    // ---------- Menú de pausa / opciones ----------

    public static void BuildMenu(GameShell shell, RectTransform layer, bool fromTitle)
    {
        var dim = Dim(layer);
        bool inHub = SceneManager.GetActiveScene().name == GameShell.HubScene;

        var window = Window(dim, fromTitle ? "OPTIONS" : "PAUSED", fromTitle ? "Options" : "Mission paused", 600f, Palette.Orange);

        Button(window, fromTitle ? "BACK" : "RESUME", shell.CloseMenu);
        VolumeRow(window, "Music", () => SaveData.MusicVolume, v => SaveData.MusicVolume = v, shell);
        VolumeRow(window, "Effects", () => SaveData.EffectsVolume, v => SaveData.EffectsVolume = v, shell);

        Button full = null;
        full = Button(window, FullscreenLabel(Screen.fullScreen), () =>
        {
            bool next = !Screen.fullScreen;
            Screen.fullScreen = next;
            full.GetComponentInChildren<Text>().text = FullscreenLabel(next);
        });

        Button tutorial = null;
        tutorial = Button(window, "REPLAY TUTORIAL", () =>
        {
            SaveData.ResetTutorials();
            tutorial.GetComponentInChildren<Text>().text = "TUTORIAL WILL SHOW AGAIN";
        });

        Button(window, "CREDITS", shell.OpenCredits);
        if (!fromTitle && !inHub) Button(window, "SPACE CENTER", shell.ToSpaceCenter);
#if !UNITY_WEBGL
        Button(window, "QUIT GAME", shell.Quit, Palette.TextSecondary);
#endif
    }

    static string FullscreenLabel(bool on) => on ? "FULLSCREEN: ON" : "FULLSCREEN: OFF";

    static void VolumeRow(RectTransform parent, string label, Func<float> get, Action<float> set, GameShell shell)
    {
        var row = UIFactory.Card(label, parent, null, false);
        UIFactory.Layout(row.gameObject, 64f);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(22, 10, 6, 6);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        var name = UIFactory.Label("Label", row, label, 26, Palette.TextMain, FontKind.Body);
        UIFactory.Layout(name.gameObject, -1f, -1f, 1f);

        var value = UIFactory.Label("Value", row, "", 28, Palette.Orange, FontKind.Mono, TextAnchor.MiddleCenter);
        Action refresh = () => value.text = Mathf.RoundToInt(get() * 100f) + "%";

        var minus = UIFactory.ButtonCard("Minus", row, "-", 30, null, null, false);
        UIFactory.Layout(minus.gameObject, -1f, 52f);
        minus.transform.SetSiblingIndex(value.transform.GetSiblingIndex());
        UIFactory.Layout(value.gameObject, -1f, 90f);

        var plus = UIFactory.ButtonCard("Plus", row, "+", 30, null, null, false);
        UIFactory.Layout(plus.gameObject, -1f, 52f);

        minus.onClick.AddListener(() => { set(Mathf.Max(0f, Mathf.Round(get() * 10f - 1f) / 10f)); shell.ApplyVolumes(); refresh(); });
        plus.onClick.AddListener(() => { set(Mathf.Min(1f, Mathf.Round(get() * 10f + 1f) / 10f)); shell.ApplyVolumes(); refresh(); });
        refresh();
    }

    // ---------- Créditos ----------

    public static void BuildCredits(GameShell shell, RectTransform layer)
    {
        var dim = Dim(layer);
        var window = Window(dim, "CREDITS", GameTitle, 820f, Palette.Blue);

        Line(window, "MADE FOR", "NASA Space Apps Challenge 2026 · Buenos Aires · 48 hours");
        Line(window, "TEAM", "A team of 2 from Buenos Aires");
        Line(window, "BASED ON", "NASA Mission Design Lab (Goddard) · Landsat, Hubble, LRO");
        Line(window, "3D MODELS AND PARTICLES", "Kenney: Space Kit, City Kit Industrial, Particle Pack (CC0)");
        Line(window, "PLANET TEXTURES", "Solar System Scope (CC BY 4.0)");
        Line(window, "FONTS", "Chakra Petch and Share Tech Mono (SIL Open Font License)");
        Line(window, "MUSIC AND SOUNDS", "Made in code for this game · Engine: Unity 6.6");

        Button(window, "CLOSE", shell.CloseCredits);
    }

    static void Line(RectTransform parent, string caption, string text)
    {
        var box = UIFactory.Rect(caption, parent);
        UIFactory.Vertical(box.gameObject, 4, 4, 0, 0, 2f);
        var c = UIFactory.Caption("Caption", box, caption);
        UIFactory.Layout(c.gameObject, 22f);
        UIFactory.Label("Text", box, text, 24, Palette.TextMain, FontKind.Body); // alto según las líneas que ocupe
    }

    // ---------- Título ----------

    public static void BuildTitle(GameShell shell, RectTransform layer, Action onPlay, Action onOptions)
    {
        var dim = UIFactory.Image("Dim", layer, null, new Color(0.02f, 0.03f, 0.06f, 0.72f), true); // la UI mezcla en espacio lineal: hace falta más alfa
        UIFactory.Stretch(dim.rectTransform);
        var root = dim.rectTransform;

        var column = UIFactory.Rect("Column", root);
        UIFactory.Pin(column, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(1400f, 760f));
        UIFactory.Vertical(column.gameObject, 0, 0, 0, 0, 14f).childAlignment = TextAnchor.UpperCenter;
        UIFactory.Appear(column.gameObject);

        var caption = UIFactory.Label("Event", column, "NASA SPACE APPS CHALLENGE 2026", 22, Palette.Blue, FontKind.Body, TextAnchor.MiddleCenter);
        UIFactory.Layout(caption.gameObject, 34f);

        var title = UIFactory.Label("Title", column, $"SPACE MISSION <color={Palette.Tag(Palette.Orange)}>DESIGN</color>", 92, Palette.TextMain, FontKind.Title, TextAnchor.MiddleCenter);
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        UIFactory.Layout(title.gameObject, 120f);

        var sub = UIFactory.Label("Subtitle", column, "Plan a satellite. Build it. Launch it. Find out why it worked.", 28, Palette.TextSecondary, FontKind.Body, TextAnchor.MiddleCenter);
        UIFactory.Layout(sub.gameObject, 60f);

        var buttons = UIFactory.Rect("Buttons", column);
        var v = UIFactory.Vertical(buttons.gameObject, 490, 490, 20, 0, 14f);
        v.childAlignment = TextAnchor.UpperCenter;
        UIFactory.Layout(buttons.gameObject, 440f);

        bool progress = SaveData.HasProgress;
        Button(buttons, progress ? "CONTINUE" : "PLAY", onPlay, null, 72f, 34);
        if (progress)
        {
            Button(buttons, "NEW GAME", () =>
            {
                SaveData.NewGame();
                onPlay();
            });
        }
        Button(buttons, "OPTIONS", onOptions);
        Button(buttons, "CREDITS", shell.OpenCredits);
#if !UNITY_WEBGL
        Button(buttons, "QUIT", shell.Quit, Palette.TextSecondary);
#endif
    }

    // ---------- Piezas comunes ----------

    static RectTransform Dim(RectTransform layer)
    {
        var dim = UIFactory.Image("Dim", layer, null, new Color(0f, 0f, 0f, 0.75f), true);
        UIFactory.Stretch(dim.rectTransform);
        return dim.rectTransform;
    }

    // Ventana centrada que crece con su contenido.
    static RectTransform Window(RectTransform parent, string caption, string title, float width, Color accent)
    {
        var window = UIFactory.Card("Window", parent, accent);
        window.anchorMin = window.anchorMax = window.pivot = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(width, 0f);
        UIFactory.Vertical(window.gameObject, 40, 36, 30, 34, 12f);
        var fit = window.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        UIFactory.Appear(window.gameObject);

        var c = UIFactory.Caption("Caption", window, caption);
        UIFactory.Layout(c.gameObject, 22f);
        var t = UIFactory.Label("Title", window, title, 38, Palette.TextMain, FontKind.Title);
        UIFactory.Layout(t.gameObject, 56f);
        return window;
    }

    static Button Button(RectTransform parent, string label, Action onClick, Color? color = null, float height = 60f, int size = 26)
    {
        var b = UIFactory.ButtonCard(label, parent, label, size, color, null, false);
        UIFactory.Layout(b.gameObject, height);
        b.onClick.AddListener(() => onClick());
        return b;
    }
}
