using UnityEngine;
using UnityEngine.UI;

// Menú principal (escena MenuPrincipal): la Tierra desde el espacio con un satélite, el título
// y las opciones de partida. Desde acá se elige nivel, Modo Libre y tutorial, y se carga el juego.
public class MainMenu : MonoBehaviour
{
    public Transform planet;
    public Transform satellite;
    public SatelliteVisual satelliteParts;
    public Transform cameraRig;
    public RectTransform canvasRoot;
    public string firstScene = "CentroEspacial";

    Text levelLabel, freeLabel, tutorialLabel, hint;
    Vector3 cameraStart;

    void Start()
    {
        if (satelliteParts != null) satelliteParts.Show(ShowcaseDesign());
        if (cameraRig != null) cameraStart = cameraRig.position;
        GameShell.SceneEscape = () => true; // en el menú, Esc no abre la pausa
        BuildUI();
        Refresh();
    }

    void OnDestroy() => GameShell.SceneEscape = null;

    void Update()
    {
        if (planet != null) planet.Rotate(Vector3.up, 0.6f * Time.unscaledDeltaTime, Space.Self);
        if (satellite != null) satellite.Rotate(new Vector3(0.15f, 1f, 0.1f), 5f * Time.unscaledDeltaTime, Space.World);
        if (cameraRig != null) // deriva lenta, como flotando
            cameraRig.position = cameraStart + new Vector3(Mathf.Sin(Time.unscaledTime * 0.11f) * 0.5f, Mathf.Sin(Time.unscaledTime * 0.07f) * 0.3f, 0f);
    }

    static MissionDesign ShowcaseDesign()
    {
        var d = new MissionDesign
        {
            panel = TestCatalog.Get("panel_l"), battery = TestCatalog.Get("bat"), antenna = TestCatalog.Get("ant_l"),
        };
        d.instruments.Add(TestCatalog.Get("cam_a"));
        return d;
    }

    // ---------- Acciones ----------

    void Play() => GameShell.Load(firstScene);

    void NewGame()
    {
        SaveData.NewGame();
        Difficulty.Apply();
        Play();
    }

    void Refresh()
    {
        levelLabel.text = "LEVEL:  " + Difficulty.Name;
        freeLabel.text = "FREE MODE:  " + (FreeMode.On ? "ON" : "OFF");
        tutorialLabel.text = "TUTORIAL:  " + (SaveData.TutorialOn ? "ON" : "OFF");
    }

    void Say(string text) => hint.text = text;

    // ---------- Interfaz ----------

    void BuildUI()
    {
        const float left = 120f, width = 470f;

        var column = UIFactory.Rect("Column", canvasRoot);
        UIFactory.Place(column, Vector2.zero, new Vector2(0f, 1f), new Vector2(left, 70f), new Vector2(left + width, -70f));
        var v = UIFactory.Vertical(column.gameObject, 0, 0, 0, 0, 10f);
        v.childAlignment = TextAnchor.MiddleLeft;
        UIFactory.Appear(column.gameObject);

        var caption = UIFactory.Label("Event", column, "NASA SPACE APPS CHALLENGE 2026", 19, Palette.Blue, FontKind.Body);
        UIFactory.Layout(caption.gameObject, 28f);
        var t1 = UIFactory.Label("Title1", column, "SPACE MISSION", 62, Palette.TextMain, FontKind.Title);
        t1.horizontalOverflow = HorizontalWrapMode.Overflow;
        UIFactory.Layout(t1.gameObject, 66f);
        var t2 = UIFactory.Label("Title2", column, "DESIGN", 104, Palette.Orange, FontKind.Title);
        t2.horizontalOverflow = HorizontalWrapMode.Overflow;
        UIFactory.Layout(t2.gameObject, 104f);
        var tag = UIFactory.Label("Tagline", column, "Plan a satellite. Build it. Launch it.\nFind out why it worked.", 22, Palette.TextSecondary, FontKind.Body);
        UIFactory.Layout(tag.gameObject, 64f);
        UIFactory.Layout(UIFactory.Rect("Gap", column).gameObject, 6f);

        bool progress = SaveData.HasProgress;
        Button(column, progress ? "CONTINUE" : "PLAY", Play, 66f, 32, true);
        if (progress) Button(column, "NEW GAME", NewGame);

        levelLabel = Button(column, "", () =>
        {
            Difficulty.Next();
            Refresh();
            Say(Difficulty.Summary);
        }).GetComponentInChildren<Text>();
        freeLabel = Button(column, "", () =>
        {
            FreeMode.Toggle();
            Refresh();
            Say(FreeMode.On
                ? "Free mode ON: every contract is open and there is no budget limit. Flights don't count for the campaign."
                : "Free mode OFF: campaign. Complete a contract to unlock the next one.");
        }).GetComponentInChildren<Text>();
        tutorialLabel = Button(column, "", () =>
        {
            SaveData.TutorialOn = !SaveData.TutorialOn;
            Refresh();
            Say(SaveData.TutorialOn
                ? "Tutorial ON: a guide will walk you through your first mission, step by step."
                : "Tutorial OFF: no step-by-step guide. The Mission Guide is always in the Esc menu.");
        }).GetComponentInChildren<Text>();

        Button(column, "HOW TO PLAY", () => GameShell.Instance.OpenGuide());
        Button(column, "OPTIONS", () => GameShell.Instance.OpenOptions());
        Button(column, "CREDITS", () => GameShell.Instance.OpenCredits());
#if !UNITY_WEBGL
        Button(column, "QUIT", () => GameShell.Instance.Quit(), 50f, 22, false, Palette.TextSecondary);
#endif

        // Explicación de la última opción tocada
        hint = UIFactory.Label("Hint", column, Difficulty.Summary, 18, Palette.TextSecondary, FontKind.Body, TextAnchor.UpperLeft);
        UIFactory.Layout(hint.gameObject, 70f);

        var team = UIFactory.Label("Team", canvasRoot, "Hell0 World!  ·  Buenos Aires  ·  Made with NASA open resources", 16, Palette.TextSecondary, FontKind.Body, TextAnchor.UpperRight);
        UIFactory.Place(team.rectTransform, new Vector2(0.4f, 1f), Vector2.one, new Vector2(0f, -64f), new Vector2(-36f, -32f));
    }

    static Button Button(RectTransform parent, string label, System.Action onClick, float height = 50f, int size = 22, bool accent = false, Color? color = null)
    {
        var b = UIFactory.ButtonCard(label.Length > 0 ? label : "Setting", parent, label, size, color, accent ? Palette.Orange : (Color?)null, false);
        UIFactory.Layout(b.gameObject, height);
        b.onClick.AddListener(() => onClick());
        return b;
    }
}
