using System.Text;
using UnityEngine;
using UnityEngine.UI;

// UI del hangar: catálogo por pestañas a la izquierda, HUD de recursos a la derecha y fondos abajo.
// Solo muestra y captura clics; las reglas están en MissionCalculator y HangarController.
public class HangarUI : MonoBehaviour
{
    public HangarController controller;

    [Header("Catálogo")]
    public Button[] tabButtons; // Science, Power, Launch, Comms
    public RectTransform listContent;
    public Text infoText;

    [Header("HUD")]
    public Text contractTitle, contractDetail;
    public ResourceBar massBar, powerBar, dataBar, costBar;
    public Text scienceText, riskText, alertsText, fundsText, launchReasonText;
    public Button launchButton, backButton, clearButton;

    int currentTab;
    float clearArmedUntil;
    MissionReview review;
    Text clearLabel;
    static string Orange => Palette.Tag(Palette.Orange);
    static string Green => Palette.Tag(Palette.Green);
    static string Secondary => Palette.Tag(Palette.TextSecondary);

    void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int tab = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(tab));
        }
        // LAUNCH abre la revisión previa (MEL + mapa de la órbita); desde ahí se confirma.
        review = MissionReview.Create(launchButton.GetComponentInParent<Canvas>().rootCanvas.transform, controller);
        launchButton.onClick.AddListener(review.Open);
        GameShell.SceneEscape = CloseReview;
        backButton.onClick.AddListener(controller.BackToHub);
        controller.Changed += Refresh;
        if (clearButton != null)
        {
            clearLabel = clearButton.GetComponentInChildren<Text>();
            clearButton.onClick.AddListener(Clear);
        }

        infoText.text = "Hover a part to see what it does, click to add or remove it. Watch the bars on the right: if something doesn't fit, LAUNCH stays locked. Numbers are simplified game values.";
        if (controller.StartedFromSeed)
            infoText.text = "<b>Starter design loaded.</b> It can fly, but it won't reach the science goal yet. Change one part at a time and watch the bars. Hover a part to read what it does.";
        ShowTab(0);
        Refresh();
        BuildGuideButton();

        GameShell.Tutorial("tut_hangar", new[]
        {
            "Build your satellite with the four tabs on the left: <b>Science</b>, <b>Power</b>, <b>Rocket</b> and <b>Comms</b>.",
            "The bars on the right compare what you use against what you have. <color=#7BD88F>Green</color> is fine, <color=#F0782A>orange</color> means trouble.",
            "Reach the science goal without going over budget. When there are no red X alerts, press <b>LAUNCH</b>!",
        }, UIFactory.Margin, 500f);
    }

    // Botón visible para abrir la guía de conceptos (también está en el menú de Esc).
    void BuildGuideButton()
    {
        var root = launchButton.GetComponentInParent<Canvas>().rootCanvas.transform;
        var guide = UIFactory.ButtonCard("GuideButton", root, "?  MISSION GUIDE", 20, null, null, false);
        UIFactory.Pin((RectTransform)guide.transform, new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(230f, 46f));
        guide.transform.SetSiblingIndex(1);
        guide.onClick.AddListener(() => { if (GameShell.Instance != null) GameShell.Instance.OpenGuide(); });
    }

    // Primer clic: pide confirmar. Segundo clic (dentro de 3 s): vacía el diseño.
    void Clear()
    {
        if (Time.unscaledTime > clearArmedUntil)
        {
            clearArmedUntil = Time.unscaledTime + 3f;
            clearLabel.text = "SURE?";
            return;
        }
        clearArmedUntil = 0f;
        controller.ClearDesign();
        infoText.text = "Design cleared. Start again from the Science tab.";
        ShowTab(0);
    }

    void Update()
    {
        if (clearLabel != null && clearArmedUntil > 0f && Time.unscaledTime > clearArmedUntil)
        {
            clearArmedUntil = 0f;
            clearLabel.text = "CLEAR ALL";
        }
    }

    bool CloseReview()
    {
        if (review == null || !review.IsOpen) return false;
        review.Close();
        return true;
    }

    void OnDestroy()
    {
        if (GameShell.SceneEscape == CloseReview) GameShell.SceneEscape = null;
        if (controller != null) controller.Changed -= Refresh;
    }

    public void ShowTab(int tab)
    {
        currentTab = tab;
        for (int i = 0; i < tabButtons.Length; i++)
            tabButtons[i].GetComponent<CardButtonFx>().SetActive(i == tab);
        BuildRows();
    }

    void Refresh()
    {
        var r = controller.Report;
        var c = controller.Contract;

        contractTitle.text = c.title;
        contractDetail.text = FreeMode.On
            ? $"Goal {c.scienceGoal} science  ·  <color={Palette.Tag(Palette.Blue)}>Free mode</color>"
            : $"Goal {c.scienceGoal} science  ·  Budget ${c.budget}M";

        massBar.Set(r.mass, r.capacity, r.capacity > 0f ? $"{r.mass:0} / {r.capacity:0} kg" : $"{r.mass:0} kg");
        powerBar.Set(r.consumption, r.generation, r.generation > 0f ? $"{r.consumption:0} / {r.generation:0} W" : $"{r.consumption:0} W");
        dataBar.Set(r.dataGenerated, r.downlink, $"{r.dataGenerated:0} / {r.downlink:0} GB/day", 1f);
        if (FreeMode.On) costBar.Set(r.cost, 0f, $"${r.cost}M · no limit");
        else costBar.Set(r.cost, c.budget, $"${r.cost}M / ${c.budget}M", 1f);

        scienceText.text = $"{r.science}<color={Secondary}>/{c.scienceGoal}</color>";
        scienceText.color = r.science >= c.scienceGoal ? Palette.Green : Palette.Orange;
        // El riesgo depende de los márgenes de energía y masa: sin panel o sin cohete todavía no se puede calcular.
        bool riskKnown = r.generation > 0f && r.capacity > 0f;
        riskText.text = riskKnown ? $"{r.riskPercent}%" : "--";
        riskText.color = !riskKnown ? Palette.Dim : r.riskPercent <= 10 ? Palette.Green : Palette.Orange;

        var alerts = new StringBuilder();
        foreach (var b in r.blockers) alerts.AppendLine($"<color={Orange}>X</color>  {b}");
        foreach (var w in r.warnings) alerts.AppendLine($"<color={Orange}>!</color>  <color={Secondary}>{w}</color>");
        alertsText.text = alerts.Length > 0 ? alerts.ToString() : $"<color={Green}>All good. No alerts.</color>";

        int left = c.budget - r.cost;
        fundsText.text = FreeMode.On ? "FREE" : left >= 0 ? $"${left}M" : $"-${-left}M";
        fundsText.color = FreeMode.On ? Palette.Blue : left >= 0 ? Palette.Green : Palette.Orange;

        launchButton.interactable = r.CanLaunch;
        launchReasonText.text = r.CanLaunch ? "All systems go!" : r.blockers[0];
        launchReasonText.color = r.CanLaunch ? Palette.Green : Palette.TextSecondary;

        BuildRows();
    }

    // ---------- Catálogo ----------

    void BuildRows()
    {
        foreach (Transform child in listContent)
        {
            child.gameObject.SetActive(false); // el layout lo ignora ya mismo; Destroy actúa al final del frame
            Destroy(child.gameObject);
        }

        switch (currentTab)
        {
            case 0:
                Header("Instruments (up to 3)", "kg", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Instrument)) Row(p, $"{p.mass:0}", $"{p.cost}");
                break;
            case 1:
                Header("Solar panel", "W", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.SolarPanel)) Row(p, $"+{p.generation:0}", $"{p.cost}");
                Header("Battery (optional)", "kg", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Battery)) Row(p, $"{p.mass:0}", $"{p.cost}");
                break;
            case 2:
                Header("Rocket", "lifts kg", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Rocket)) Row(p, $"{p.capacity:0}", $"{p.cost}");
                Header("Orbit", "altitude", "");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Orbit)) Row(p, p.altitude.Replace("~", ""), "");
                break;
            case 3:
                Header("Antenna", "GB/day", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Antenna)) Row(p, $"{p.downlink:0}", $"{p.cost}");
                Header("Ground stations", "", "$M");
                StationsRow();
                break;
        }
    }

    void Header(string title, string col2, string col3)
    {
        var row = UIFactory.Rect("Header", listContent);
        UIFactory.Layout(row.gameObject, 34f);
        var a = UIFactory.Caption("Name", row, title);
        UIFactory.Place(a.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 0f), new Vector2(-NameRight, 0f));
        var b = UIFactory.Caption("Col2", row, col2);
        b.alignment = TextAnchor.MiddleRight;
        UIFactory.Place(b.rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-Col2Left, 0f), new Vector2(-Col2Right, 0f));
        var c = UIFactory.Caption("Col3", row, col3);
        c.alignment = TextAnchor.MiddleRight;
        UIFactory.Place(c.rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-Col3Left, 0f), new Vector2(-14f, 0f));
    }

    void Row(PartSpec part, string col2, string col3)
    {
        bool selected = controller.IsSelected(part);
        string unavailable = controller.UnavailableReason(part);

        var button = UIFactory.ButtonCard(part.id, listContent, "", 24, null, null, false);
        UIFactory.Layout(button.gameObject, 52f);
        button.interactable = unavailable == null;
        button.GetComponent<CardButtonFx>().SetActive(selected);

        var nameColor = unavailable != null ? Palette.Dim : selected ? Palette.Orange : Palette.TextMain;
        var numberColor = unavailable != null ? Palette.Dim : Palette.TextSecondary;
        string name = part.displayName + (unavailable != null ? $"  ({unavailable})" : "");

        var row = (RectTransform)button.transform;
        var a = UIFactory.Label("Name", row, name, 24, nameColor, selected ? FontKind.Title : FontKind.Body);
        // Alto de una sola línea (10 px de margen): si no entra, achica la letra en vez de partirla.
        UIFactory.Place(a.rectTransform, Vector2.zero, Vector2.one, new Vector2(18f, 10f), new Vector2(-NameRight, -10f));
        FitOneLine(a, 14);
        var b = UIFactory.Label("Col2", row, col2, 24, numberColor, FontKind.Mono, TextAnchor.MiddleRight);
        UIFactory.Place(b.rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-Col2Left, 10f), new Vector2(-Col2Right, -10f));
        FitOneLine(b, 15);
        var c = UIFactory.Label("Col3", row, col3, 24, numberColor, FontKind.Mono, TextAnchor.MiddleRight);
        UIFactory.Place(c.rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-Col3Left, 0f), new Vector2(-14f, 0f));

        // Pasar el mouse por una pieza muestra qué hace, sin elegirla.
        var hover = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        var entry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        entry.callback.AddListener(_ => infoText.text = Describe(part));
        hover.triggers.Add(entry);

        button.onClick.AddListener(() =>
        {
            string message = controller.Toggle(part);
            infoText.text = message ?? Describe(part);
        });
    }

    // Columnas del catálogo (distancias desde el borde derecho de la fila)
    const float NameRight = 196f, Col2Left = 196f, Col2Right = 62f, Col3Left = 58f;

    // Achica la letra si el texto no entra en una línea (por ejemplo "Lunar orbit (not allowed)").
    static void FitOneLine(Text t, int minSize)
    {
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = minSize;
        t.resizeTextMaxSize = t.fontSize;
    }

    void StationsRow()
    {
        int n = controller.Design.stations;
        var row = UIFactory.Card("Stations", listContent, null, false);
        UIFactory.Layout(row.gameObject, 60f, -1f, -1f, 0f); // flexibleHeight 0: que no se estire
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 16, 6, 6);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        var label = UIFactory.Label("Label", row, "Stations on Earth", 24, Palette.TextMain);
        UIFactory.Layout(label.gameObject, -1f, -1f, 1f);

        var minus = UIFactory.ButtonCard("Minus", row, "-", 28, null, null, false);
        UIFactory.Layout(minus.gameObject, -1f, 48f);
        minus.interactable = n > 1;
        minus.onClick.AddListener(() => { controller.SetStations(n - 1); infoText.text = StationsInfo(); });

        var count = UIFactory.Label("Count", row, n.ToString(), 28, Palette.Orange, FontKind.Mono, TextAnchor.MiddleCenter);
        UIFactory.Layout(count.gameObject, -1f, 36f);

        var plus = UIFactory.ButtonCard("Plus", row, "+", 28, null, null, false);
        UIFactory.Layout(plus.gameObject, -1f, 48f);
        plus.interactable = n < MissionCalculator.MaxStations;
        plus.onClick.AddListener(() => { controller.SetStations(n + 1); infoText.text = StationsInfo(); });

        var cost = UIFactory.Label("Cost", row, $"{MissionCalculator.StationCost * n}", 24, Palette.TextSecondary, FontKind.Mono, TextAnchor.MiddleRight);
        UIFactory.Layout(cost.gameObject, -1f, 46f);
    }

    string StationsInfo()
    {
        int n = controller.Design.stations;
        return $"<b>Ground stations: {n}</b>\nMore stations listen to your satellite longer, so more data reaches Earth. " +
               $"<color={Green}>${MissionCalculator.StationCost}M each</color>";
    }

    static string Describe(PartSpec p)
    {
        string stats;
        switch (p.category)
        {
            case PartCategory.Instrument: stats = $"{p.mass:0} kg · {p.power:0} W · {p.dataRate:0} GB/day · ${p.cost}M"; break;
            case PartCategory.SolarPanel: stats = $"{p.mass:0} kg · +{p.generation:0} W · ${p.cost}M"; break;
            case PartCategory.Battery: stats = $"{p.mass:0} kg · ${p.cost}M"; break;
            case PartCategory.Antenna: stats = $"{p.mass:0} kg · {p.power:0} W · sends {p.downlink:0} GB/day · ${p.cost}M"; break;
            case PartCategory.Rocket: stats = $"lifts {p.capacity:0} kg to low orbit · ${p.cost}M"; break;
            default: stats = $"{p.altitude} · contact {p.contact * 100f:0}% · {p.lifeYears} years · rocket lifts x{p.capacityMultiplier}"; break;
        }
        return $"<b>{p.displayName}</b>\n<color={Secondary}>{p.description}</color>\n<color={Green}>{stats}</color>";
    }
}
