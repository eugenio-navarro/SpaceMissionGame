using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantallas de GameShell armadas en tiempo de ejecución con la misma estética que el resto:
// menú de pausa y opciones, créditos y pantalla de título.
public static class ShellScreens
{
    const string GameTitle = UILayout.GameTitle;

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
            tutorial.GetComponentInChildren<Text>().text = "TUTORIAL ON: STARTS IN THE SPACE CENTER";
        });

        Button(window, "MISSION GUIDE", shell.OpenGuide);
        Button(window, "CREDITS", shell.OpenCredits);
        if (!fromTitle && !inHub) Button(window, "SPACE CENTER", shell.ToSpaceCenter);
        if (!fromTitle) Button(window, "MAIN MENU", shell.ToMainMenu);
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
        var window = Window(dim, "CREDITS", GameTitle, 1560f, Palette.Blue);

        // Dos columnas: el juego a la izquierda, las fuentes de NASA a la derecha
        var row = UIFactory.Rect("Columns", window);
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 48f;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = false;
        var game = Column(row);
        var nasa = Column(row);

        Line(game, "MADE FOR", "NASA Space Apps Challenge 2026: \"Space Mission Design Game\" · Buenos Aires local event");
        Line(game, "TEAM", "Hell0 World! · a team of 2 from Buenos Aires");
        Line(game, "3D MODELS AND PARTICLES", "Kenney: Space Kit, City Kit Industrial, Particle Pack (CC0)");
        Line(game, "PLANET AND STAR TEXTURES", "Solar System Scope (CC BY 4.0)");
        Line(game, "FONTS", "Chakra Petch and Share Tech Mono (SIL Open Font License)");
        Line(game, "MUSIC AND SOUNDS", "Made in code for this game · Engine: Unity 6.6");
        Line(game, "ABOUT THE NUMBERS", "Masses, costs, power and data rates are simplified game values, chosen to teach the trade-offs, not real specs.");

        Line(nasa, "MISSION SEGMENTS", "NASA, \"Design and Develop Science Missions\": launch, space, ground and user segments");
        Line(nasa, "EQUIPMENT LIST, TRADE STUDIES, MARGINS", "NASA Goddard Mission Design Lab (MDL)");
        Line(nasa, "GAME IDEAS: LEVELS, REAL-MISSION COMPARISON", "NASA Webb, \"Build It Yourself: Satellite!\"");
        Line(nasa, "POWER, BATTERIES, ECLIPSES, GROUND CONTACT", "NASA CubeSat Launch Initiative, \"CubeSat 101\"");
        Line(nasa, "REAL MISSIONS IN THE GAME", "Landsat 9 (NASA/USGS, 2021, 705 km) · Hubble (NASA/ESA, 1990, 547 km) · Lunar Reconnaissance Orbiter (NASA, 2009, ~50 km above the Moon)");
        Line(nasa, "ARGENTINA IN SPACE (CONAE)", "SAOCOM 1A & 1B radar satellites (2018, 2020) · SAC-D/Aquarius (2011-2015): an Argentine satellite carrying NASA's instrument that measured ocean salinity");
        Line(nasa, "REAL CASES AND ROCKETS", "Starlink orbital drag after the Feb 2022 storm (NASA SVS) · Rocket classes: Electron, Vega, Vega-C (ESA)");
        Line(nasa, "LEARN MORE", "NASA GMAT (mission analysis) · NASA Mission Viz · JPL Concept Maturity Levels · CSA Digital Games and Activities");

        Button(window, "CLOSE", shell.CloseCredits);
    }

    static RectTransform Column(RectTransform parent)
    {
        var col = UIFactory.Rect("Column", parent);
        UIFactory.Vertical(col.gameObject, 0, 0, 0, 0, 12f);
        return col;
    }

    // ---------- Guía de conceptos (para quien nunca diseñó una misión) ----------

    public static void BuildGuide(GameShell shell, RectTransform layer)
    {
        var dim = Dim(layer);
        var window = Window(dim, "MISSION GUIDE", "How a space mission works", 1620f, Palette.Green);

        var row = UIFactory.Rect("Columns", window);
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 48f;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = false;
        var a = Column(row);
        var b = Column(row);

        Line(a, "YOUR JOB", "Pick a contract, build a satellite that can do it, launch it, and see how much science came back. Then improve it and fly again.");
        Line(a, "SCIENCE (the score)", "Useful data that reaches Earth. Instruments make data, the antenna sends it down. Data that can't be sent is lost.");
        Line(a, "MASS (kg)", "How heavy the satellite is. Every rocket can only lift so much. Higher orbits are harder, so the same rocket lifts less.");
        Line(a, "POWER (W)", "Instruments and the antenna use electricity. Solar panels make it. If you use more than you make, you can't launch.");
        Line(a, "BATTERY AND ECLIPSE", "When the satellite passes through shadow, panels make nothing. A battery keeps everything on.");

        Line(b, "DATA AND COMMS (GB/day)", "A satellite can only talk to Earth while it flies over a ground station. More stations or a bigger antenna = more data comes home.");
        Line(b, "ORBIT", "The path around Earth or the Moon. Low orbit: sharp detail, short contact. Geostationary: always in contact, far away. Lunar: around the Moon.");
        Line(b, "MARGIN", "What you have left over. If panels make 250 W and you use 160 W, your margin is 36%. Spare margin protects you from surprises: NASA Goddard's design rules require it.");
        Line(b, "FAILURE RISK", "The chance that something breaks. Thin power or mass margins push it up. If it happens, half the science is lost.");
        Line(b, "BUDGET ($M)", "Every part, rocket and ground station costs money. Going over budget blocks the launch. Reach the science goal to earn a bonus for unused budget.");

        Button(window, "GOT IT", shell.CloseCredits);
    }

    // ---------- Aviso ----------

    public static void BuildNotice(GameShell shell, RectTransform layer, string caption, string title, string body)
    {
        var dim = Dim(layer);
        var window = Window(dim, caption, title, 640f, Palette.Green);
        UIFactory.Label("Body", window, body, 26, Palette.TextSecondary, FontKind.Body);
        Button(window, "OK", shell.CloseCredits);
    }

    static void Line(RectTransform parent, string caption, string text)
    {
        var box = UIFactory.Rect(caption, parent);
        UIFactory.Vertical(box.gameObject, 4, 4, 0, 0, 2f);
        var c = UIFactory.Caption("Caption", box, caption);
        UIFactory.Layout(c.gameObject, 22f);
        UIFactory.Label("Text", box, text, 21, Palette.TextMain, FontKind.Body); // alto según las líneas que ocupe
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
        var frame = UIFactory.Card("Window", parent, accent);
        frame.Find("Fill").GetComponent<Image>().color = new Color(0.035f, 0.045f, 0.065f, 0.99f);
        UIFactory.Pin(frame, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, 960f));
        UIFactory.Appear(frame.gameObject);
        var window = UIFactory.Rect("WindowContent", frame);
        UIFactory.Stretch(window, 30f);
        UIFactory.Vertical(window.gameObject, 10, 10, 0, 0, 12f);
        UILayout.ScrollContent(window);

        var c = UIFactory.Caption("Caption", window, caption);
        UIFactory.Layout(c.gameObject, 22f);
        var t = UIFactory.Label("Title", window, title, 38, Palette.TextMain, FontKind.Title);
        UIFactory.Layout(t.gameObject, 56f);
        UILayout.Fit(t, 25);
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
