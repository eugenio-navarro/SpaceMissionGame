using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI del hangar: catálogo por pestañas a la izquierda, HUD de recursos a la derecha y fondos abajo.
// Solo muestra y captura clics; las reglas están en MissionCalculator y HangarController.
public class HangarUI : MonoBehaviour
{
    public HangarController controller;
    public Sprite uiSprite;

    [Header("Catálogo")]
    public Button[] tabButtons; // Instruments, Power, Rocket/Orbit, Comms
    public RectTransform listContent;
    public TMP_Text infoText;

    [Header("HUD")]
    public TMP_Text contractText;
    public ResourceBar massBar, powerBar, dataBar, costBar;
    public TMP_Text scienceText, riskText, alertsText, launchReasonText, fundsText;
    public Button launchButton, backButton;

    int currentTab;

    void Awake() => UIFactory.Sprite = uiSprite;

    void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int tab = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(tab));
        }
        launchButton.onClick.AddListener(controller.Launch);
        backButton.onClick.AddListener(controller.BackToHub);
        controller.Changed += Refresh;

        infoText.text = "Pick parts from each tab. Watch the bars on the right: if something doesn't fit, LAUNCH stays locked.";
        ShowTab(0);
        Refresh();
    }

    void OnDestroy()
    {
        if (controller != null) controller.Changed -= Refresh;
    }

    public void ShowTab(int tab)
    {
        currentTab = tab;
        for (int i = 0; i < tabButtons.Length; i++)
        {
            bool on = i == tab;
            tabButtons[i].image.color = on ? Palette.Orange : Palette.PanelDark;
            tabButtons[i].GetComponentInChildren<TMP_Text>().color = on ? Palette.Space : Palette.Phosphor;
        }
        BuildRows();
    }

    void Refresh()
    {
        var r = controller.Report;
        var c = controller.Contract;

        contractText.text = $"{c.title.ToUpper()}\n<color=#8F98A3>Goal: {c.scienceGoal} science  |  Budget: ${c.budget}M</color>";

        massBar.Set(r.mass, r.capacity, r.capacity > 0f ? $"{r.mass:0} / {r.capacity:0} kg" : $"{r.mass:0} kg  (no rocket/orbit)");
        powerBar.Set(r.consumption, r.generation, r.generation > 0f ? $"{r.consumption:0} / {r.generation:0} W" : $"{r.consumption:0} W  (no panel)");
        dataBar.Set(r.dataGenerated, r.downlink, $"{r.dataGenerated:0} / {r.downlink:0} GB/day", overIsBlocking: false);
        costBar.Set(r.cost, c.budget, $"${r.cost}M / ${c.budget}M");

        scienceText.text = $"Estimated science: <b>{r.science}</b> / {c.scienceGoal}";
        scienceText.color = r.science >= c.scienceGoal ? Palette.Phosphor : Palette.Yellow;
        riskText.text = $"Failure risk: <b>{r.riskPercent}%</b>";
        riskText.color = r.riskPercent <= 10 ? Palette.Phosphor : r.riskPercent <= 20 ? Palette.Yellow : Palette.Red;

        var alerts = new StringBuilder();
        foreach (var b in r.blockers) alerts.AppendLine($"<color=#E0533D>X  {b}</color>");
        foreach (var w in r.warnings) alerts.AppendLine($"<color=#E8C547>!  {w}</color>");
        alertsText.text = alerts.Length > 0 ? alerts.ToString() : "<color=#7BD88F>No alerts.</color>";

        int left = c.budget - r.cost;
        fundsText.text = $"<size=20><color=#8F98A3>FUNDS LEFT</color></size>\n${left}M";
        fundsText.color = left >= 0 ? Palette.Phosphor : Palette.Red;

        launchButton.interactable = r.CanLaunch;
        launchReasonText.text = r.CanLaunch ? "All systems go!" : r.blockers[0];
        launchReasonText.color = r.CanLaunch ? Palette.Phosphor : Palette.Red;

        BuildRows();
    }

    // ---------- Catálogo ----------

    void BuildRows()
    {
        foreach (Transform child in listContent)
        {
            child.gameObject.SetActive(false); // el layout lo ignora ya mismo; Destroy recién actúa al final del frame
            Destroy(child.gameObject);
        }

        switch (currentTab)
        {
            case 0:
                Header("INSTRUMENTS  (up to 3)", "kg", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Instrument)) Row(p, $"{p.mass:0}", $"{p.cost}");
                break;
            case 1:
                Header("SOLAR PANEL", "W", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.SolarPanel)) Row(p, $"+{p.generation:0}", $"{p.cost}");
                Header("BATTERY  (optional)", "kg", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Battery)) Row(p, $"{p.mass:0}", $"{p.cost}");
                break;
            case 2:
                Header("ROCKET", "lifts kg", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Rocket)) Row(p, $"{p.capacity:0}", $"{p.cost}");
                Header("ORBIT", "altitude", "");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Orbit)) Row(p, p.altitude, "");
                break;
            case 3:
                Header("ANTENNA", "GB/day", "$M");
                foreach (var p in TestCatalog.ByCategory(PartCategory.Antenna)) Row(p, $"{p.downlink:0}", $"{p.cost}");
                Header("GROUND STATIONS", "", "$M");
                StationsRow();
                break;
        }
    }

    void Header(string title, string col2, string col3)
    {
        var row = UIFactory.Rect("Header", listContent);
        UIFactory.Layout(row.gameObject, 34f);
        Columns(row, title, col2, col3, Palette.Metal, 17f);
    }

    void Row(PartSpec part, string col2, string col3)
    {
        bool selected = controller.IsSelected(part);
        string unavailable = controller.UnavailableReason(part);

        var button = UIFactory.Button(part.id, listContent, "", selected ? Palette.Selected : Palette.Panel, Palette.TextLight);
        UIFactory.Layout(button.gameObject, 46f);
        button.interactable = unavailable == null;

        string name = (selected ? "> " : "   ") + part.displayName + (unavailable != null ? $"  ({unavailable})" : "");
        var color = unavailable != null ? Palette.TextDisabled : selected ? Palette.Phosphor : Palette.TextLight;
        Columns((RectTransform)button.transform, name, col2, col3, color, 20f);

        button.onClick.AddListener(() =>
        {
            string message = controller.Toggle(part);
            infoText.text = message ?? Describe(part);
        });
    }

    void StationsRow()
    {
        int n = controller.Design.stations;
        var row = UIFactory.Panel("Stations", listContent, Palette.Panel);
        UIFactory.Layout(row.gameObject, 52f);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 6, 6);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;

        var label = UIFactory.Text("Label", row.transform, "Stations on Earth", 20f, Palette.TextLight);
        UIFactory.Layout(label.gameObject, -1f, -1f, 1f);

        var minus = UIFactory.Button("Minus", row.transform, "-", Palette.PanelDark, Palette.Phosphor, 26f, true);
        UIFactory.Layout(minus.gameObject, -1f, 40f);
        minus.interactable = n > 1;
        minus.onClick.AddListener(() => { controller.SetStations(n - 1); infoText.text = StationsInfo(); });

        var count = UIFactory.Text("Count", row.transform, n.ToString(), 24f, Palette.Phosphor, TextAlignmentOptions.Center, true);
        UIFactory.Layout(count.gameObject, -1f, 34f);

        var plus = UIFactory.Button("Plus", row.transform, "+", Palette.PanelDark, Palette.Phosphor, 26f, true);
        UIFactory.Layout(plus.gameObject, -1f, 40f);
        plus.interactable = n < MissionCalculator.MaxStations;
        plus.onClick.AddListener(() => { controller.SetStations(n + 1); infoText.text = StationsInfo(); });

        var cost = UIFactory.Text("Cost", row.transform, $"{MissionCalculator.StationCost * n}", 20f, Palette.TextLight, TextAlignmentOptions.MidlineRight);
        UIFactory.Layout(cost.gameObject, -1f, 50f);
    }

    static void Columns(RectTransform row, string name, string col2, string col3, Color color, float size)
    {
        var a = UIFactory.Text("Name", row, name, size, color);
        UIFactory.Place(a.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-160f, 0f));
        var b = UIFactory.Text("Col2", row, col2, size, color, TextAlignmentOptions.MidlineRight);
        UIFactory.Place(b.rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-160f, 0f), new Vector2(-64f, 0f));
        var c = UIFactory.Text("Col3", row, col3, size, color, TextAlignmentOptions.MidlineRight);
        UIFactory.Place(c.rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-60f, 0f), new Vector2(-12f, 0f));
    }

    string StationsInfo()
    {
        int n = controller.Design.stations;
        return $"<b>Ground stations: {n}</b>\nMore stations listen to your satellite longer, so more data reaches Earth. " +
               $"${MissionCalculator.StationCost}M each.";
    }

    static string Describe(PartSpec p)
    {
        string stats;
        switch (p.category)
        {
            case PartCategory.Instrument: stats = $"{p.mass:0} kg  |  {p.power:0} W  |  {p.dataRate:0} GB/day  |  ${p.cost}M"; break;
            case PartCategory.SolarPanel: stats = $"{p.mass:0} kg  |  +{p.generation:0} W  |  ${p.cost}M"; break;
            case PartCategory.Battery: stats = $"{p.mass:0} kg  |  ${p.cost}M"; break;
            case PartCategory.Antenna: stats = $"{p.mass:0} kg  |  {p.power:0} W  |  sends {p.downlink:0} GB/day  |  ${p.cost}M"; break;
            case PartCategory.Rocket: stats = $"Lifts {p.capacity:0} kg to low orbit  |  ${p.cost}M"; break;
            default: stats = $"{p.altitude}  |  contact {p.contact * 100f:0}%  |  lasts {p.lifeYears} years  |  rocket lifts x{p.capacityMultiplier}"; break;
        }
        return $"<b>{p.displayName}</b>\n{p.description}\n<color=#7BD88F>{stats}</color>";
    }
}
