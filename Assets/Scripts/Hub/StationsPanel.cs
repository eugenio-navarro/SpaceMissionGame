using UnityEngine;
using UnityEngine.UI;

// Edificio de antenas: elige las estaciones en Tierra del contrato actual y muestra con un diagrama
// por qué más estaciones bajan más datos (el satélite solo envía cuando pasa sobre una estación).
public class StationsPanel : MonoBehaviour
{
    // Ángulos (grados) de las 4 estaciones sobre el borde de la Tierra, en el orden en que se agregan.
    static readonly float[] StationAngles = { 90f, 210f, 330f, 30f };
    const float EarthRadius = 130f, OrbitRadius = 215f, ContactAngle = 32f;

    HubController hub;
    RectTransform diagram, satellite, link;
    Image[] stationDots = new Image[4];
    Image linkImage, satelliteImage;
    Text contactText, countText, costText, designText;
    Text[] rowLabels = new Text[4], rowValues = new Text[4];
    Image[] rowFills = new Image[4];
    Button minus, plus;
    float satAngle = 120f;
    int stations = 1;

    MissionDesign design;     // diseño guardado del contrato aceptado (null si no hay contrato)
    ContractSpec contract;

    public static StationsPanel Create(Transform canvasRoot, HubController hub)
    {
        var modal = HubModal.Create("StationsPanel", canvasRoot, new Vector2(1320f, 700f), Palette.Blue,
            "Ground segment", "Ground Stations", hub.CloseAll, out var window);
        var panel = modal.gameObject.AddComponent<StationsPanel>();
        panel.hub = hub;
        panel.Build(window);
        modal.gameObject.SetActive(false);
        return panel;
    }

    public void Open()
    {
        contract = string.IsNullOrEmpty(HubController.AcceptedContractId) ? null : TestCatalog.Contract(HubController.AcceptedContractId);
        design = null;
        stations = 2;
        if (contract != null)
        {
            design = new MissionDesign();
            SaveData.LoadDesign(contract.id, design);
            stations = design.stations;
        }
        gameObject.SetActive(true);
        Refresh();
    }

    void SetStations(int n)
    {
        stations = Mathf.Clamp(n, 1, MissionCalculator.MaxStations);
        if (design != null)
        {
            design.stations = stations;
            SaveData.SaveDesign(contract.id, design);
        }
        Refresh();
    }

    void Refresh()
    {
        countText.text = stations.ToString();
        costText.text = $"${MissionCalculator.StationCost * stations}M";
        minus.interactable = stations > 1;
        plus.interactable = stations < MissionCalculator.MaxStations;

        for (int i = 0; i < 4; i++)
        {
            bool on = i < stations;
            stationDots[i].color = on ? Palette.Orange : Palette.Dim;
            bool current = i == stations - 1;
            rowLabels[i].color = current ? Palette.Orange : Palette.TextSecondary;
            rowValues[i].color = current ? Palette.Orange : Palette.TextSecondary;
            rowFills[i].color = current ? Palette.Orange : Palette.Dim;
        }

        if (contract == null)
        {
            designText.text = $"<color={Palette.Tag(Palette.TextSecondary)}>Try the buttons to see how it works. To save your choice, accept a contract at Mission Control first.</color>";
            return;
        }

        var report = MissionCalculator.Evaluate(design, contract);
        if (design.antenna == null || design.orbit == null || design.instruments.Count == 0)
        {
            designText.text = $"Saved for <b>{contract.title}</b>.\n<color={Palette.Tag(Palette.TextSecondary)}>Pick instruments, an antenna and an orbit in the Hangar to see how much data reaches Earth.</color>";
            return;
        }

        bool allData = report.downlink >= report.dataGenerated;
        string color = Palette.Tag(allData ? Palette.Green : Palette.Orange);
        designText.text = $"Saved for <b>{contract.title}</b>.\nYour satellite makes {report.dataGenerated:0} GB/day and can send <color={color}>{report.downlink:0} GB/day</color>." +
                          (allData ? $"\n<color={color}>All your data reaches Earth.</color>" : $"\n<color={color}>Some data is lost: add stations or a bigger antenna.</color>");
    }

    // El satélite gira; si pasa cerca de una estación activa, se dibuja el enlace.
    void Update()
    {
        satAngle = Mathf.Repeat(satAngle + Time.unscaledDeltaTime * 40f, 360f);
        Vector2 satPos = Polar(satAngle, OrbitRadius);
        satellite.anchoredPosition = satPos;
        satellite.localRotation = Quaternion.Euler(0f, 0f, satAngle);

        int linked = -1;
        for (int i = 0; i < stations; i++)
            if (Mathf.Abs(Mathf.DeltaAngle(satAngle, StationAngles[i])) < ContactAngle) linked = i;

        link.gameObject.SetActive(linked >= 0);
        if (linked >= 0)
        {
            Vector2 from = Polar(StationAngles[linked], EarthRadius);
            Vector2 delta = satPos - from;
            link.anchoredPosition = from;
            link.sizeDelta = new Vector2(delta.magnitude, 4f);
            link.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }
        satelliteImage.color = linked >= 0 ? Palette.Green : Palette.Metal;
        contactText.text = linked >= 0 ? "IN CONTACT: SENDING DATA" : "OUT OF RANGE: DATA WAITS ON BOARD";
        contactText.color = linked >= 0 ? Palette.Green : Palette.TextSecondary;
    }

    static Vector2 Polar(float degrees, float radius) =>
        new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad)) * radius;

    // ---------- Armado ----------

    void Build(RectTransform window)
    {
        // Diagrama (izquierda)
        var left = UIFactory.Rect("Diagram", window);
        UIFactory.Place(left, Vector2.zero, new Vector2(0f, 1f), new Vector2(36f, 36f), new Vector2(36f + 520f, -124f));
        var frame = UIFactory.Image("Frame", left, UIFactory.RoundedFill, new Color(0f, 0f, 0f, 0.3f));
        UIFactory.Stretch(frame.rectTransform);

        diagram = UIFactory.Rect("Center", left);
        UIFactory.Pin(diagram, new Vector2(0.5f, 0.5f), new Vector2(0f, 22f), Vector2.zero);

        var orbit = UIFactory.Image("Orbit", diagram, ShapeArt.Ring, new Color(1f, 1f, 1f, 0.18f));
        orbit.type = Image.Type.Simple;
        UIFactory.Pin(orbit.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * OrbitRadius * 2f);

        var earth = UIFactory.Image("Earth", diagram, ShapeArt.Circle, Palette.Planet);
        earth.type = Image.Type.Simple;
        UIFactory.Pin(earth.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * EarthRadius * 2f);
        var earthLabel = UIFactory.Label("Label", earth.transform, "EARTH", 26, Color.white, FontKind.Title, TextAnchor.MiddleCenter);
        UIFactory.Stretch(earthLabel.rectTransform);

        linkImage = UIFactory.Image("Link", diagram, null, Palette.Green);
        link = linkImage.rectTransform;
        link.anchorMin = link.anchorMax = new Vector2(0.5f, 0.5f);
        link.pivot = new Vector2(0f, 0.5f);

        for (int i = 0; i < 4; i++)
        {
            var dot = UIFactory.Image("Station" + (i + 1), diagram, ShapeArt.Circle, Palette.Dim);
            dot.type = Image.Type.Simple;
            UIFactory.Pin(dot.rectTransform, new Vector2(0.5f, 0.5f), Polar(StationAngles[i], EarthRadius), Vector2.one * 26f);
            dot.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            dot.rectTransform.anchoredPosition = Polar(StationAngles[i], EarthRadius);
            stationDots[i] = dot;
        }

        satelliteImage = UIFactory.Image("Satellite", diagram, UIFactory.RoundedFill, Palette.Metal);
        satellite = satelliteImage.rectTransform;
        satellite.anchorMin = satellite.anchorMax = satellite.pivot = new Vector2(0.5f, 0.5f);
        satellite.sizeDelta = new Vector2(26f, 40f);

        contactText = UIFactory.Label("Contact", left, "", 18, Palette.TextSecondary, FontKind.Body, TextAnchor.MiddleCenter);
        UIFactory.Place(contactText.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(10f, 10f), new Vector2(-10f, 44f));

        // Explicación y controles (derecha)
        var right = UIFactory.Rect("Info", window);
        UIFactory.Place(right, Vector2.zero, Vector2.one, new Vector2(36f + 520f + 36f, 36f), new Vector2(-40f, -124f));
        UIFactory.Vertical(right.gameObject, 0, 0, 0, 0, 12f);

        var body = UIFactory.Label("Body", right,
            "Your satellite can only send data while it flies over a ground station. More stations around the world mean more contact time, so more of your science reaches Earth.",
            24, Palette.TextSecondary, FontKind.Body);
        body.horizontalOverflow = HorizontalWrapMode.Wrap;

        BuildStepper(right);

        UIFactory.Layout(UIFactory.Caption("RowsCaption", right, "Contact time: how much your antenna can send").gameObject, 24f);
        for (int i = 0; i < 4; i++) BuildRow(right, i);

        designText = UIFactory.Label("Design", right, "", 22, Palette.TextMain, FontKind.Body);
        UIFactory.Label("Note", right, "You can also change this in the Hangar, Comms tab.", 18, Palette.Dim, FontKind.Body);
    }

    void BuildStepper(RectTransform parent)
    {
        var row = UIFactory.Card("Stepper", parent, null, false);
        UIFactory.Layout(row.gameObject, 64f, -1f, -1f, 0f); // flexibleHeight 0: que no se estire
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(22, 18, 6, 6);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        UIFactory.Layout(UIFactory.Label("Label", row, "Ground stations", 26, Palette.TextMain).gameObject, -1f, -1f, 1f);
        minus = UIFactory.ButtonCard("Minus", row, "-", 30, null, null, false);
        UIFactory.Layout(minus.gameObject, -1f, 52f);
        minus.onClick.AddListener(() => SetStations(stations - 1));
        countText = UIFactory.Label("Count", row, "", 30, Palette.Orange, FontKind.Mono, TextAnchor.MiddleCenter);
        UIFactory.Layout(countText.gameObject, -1f, 50f);
        plus = UIFactory.ButtonCard("Plus", row, "+", 30, null, null, false);
        UIFactory.Layout(plus.gameObject, -1f, 52f);
        plus.onClick.AddListener(() => SetStations(stations + 1));
        costText = UIFactory.Label("Cost", row, "", 26, Palette.TextSecondary, FontKind.Mono, TextAnchor.MiddleRight);
        UIFactory.Layout(costText.gameObject, -1f, 90f);
    }

    void BuildRow(RectTransform parent, int i)
    {
        float factor = MissionCalculator.StationFactor[i];
        var row = UIFactory.Rect("Row" + (i + 1), parent);
        UIFactory.Layout(row.gameObject, 26f);

        rowLabels[i] = UIFactory.Label("Label", row, i == 0 ? "1 station" : $"{i + 1} stations", 20, Palette.TextSecondary);
        UIFactory.Place(rowLabels[i].rectTransform, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(130f, 0f));

        var track = UIFactory.Image("Track", row, UIFactory.RoundedFill, new Color(1f, 1f, 1f, 0.08f));
        UIFactory.Place(track.rectTransform, Vector2.zero, Vector2.one, new Vector2(140f, 6f), new Vector2(-90f, -6f));
        rowFills[i] = UIFactory.Image("Fill", track.transform, UIFactory.RoundedFill, Palette.Dim);
        UIFactory.Place(rowFills[i].rectTransform, Vector2.zero, new Vector2(factor / 1.15f, 1f), Vector2.zero, Vector2.zero);

        rowValues[i] = UIFactory.Label("Value", row, "x" + factor.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture), 20, Palette.TextSecondary, FontKind.Mono, TextAnchor.MiddleRight);
        UIFactory.Place(rowValues[i].rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-80f, 0f), Vector2.zero);
    }
}
