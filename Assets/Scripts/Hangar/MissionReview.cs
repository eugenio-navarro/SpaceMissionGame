using UnityEngine;
using UnityEngine.UI;

// Revisión previa al lanzamiento, como en el Mission Design Lab de la NASA:
// la MEL (Master Equipment List: cada pieza con su masa, energía, datos y costo),
// los márgenes y el mapa de la órbita elegida. Desde acá se confirma el lanzamiento.
public class MissionReview : MonoBehaviour
{
    HangarController controller;
    RectTransform table, mapArea;
    Text margins, orbitText;

    public bool IsOpen => gameObject.activeSelf;

    public static MissionReview Create(Transform canvasRoot, HangarController controller)
    {
        var modal = UIFactory.Rect("MissionReview", canvasRoot);
        UIFactory.Stretch(modal);
        var dim = modal.gameObject.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.6f);

        var window = UIFactory.Card("Window", modal, Palette.Orange);
        window.Find("Fill").GetComponent<Image>().color = new Color(0.035f, 0.045f, 0.065f, 0.99f);
        UIFactory.Pin(window, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1540f, 860f));
        UIFactory.Appear(window.gameObject);

        var review = modal.gameObject.AddComponent<MissionReview>();
        review.controller = controller;
        review.Build(window);
        modal.gameObject.SetActive(false);
        return review;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Fill();
    }

    public void Close() => gameObject.SetActive(false);

    // ---------- Contenido ----------

    void Fill()
    {
        foreach (Transform child in table) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        foreach (Transform child in mapArea) { child.gameObject.SetActive(false); Destroy(child.gameObject); }

        var d = controller.Design;
        var r = controller.Report;
        var c = controller.Contract;

        Row("PART", "TYPE", "MASS kg", "POWER W", "DATA GB/day", "COST $M", true);
        Row("Satellite bus (frame, computer)", "Base", $"{MissionCalculator.BaseMass:0}", $"-{MissionCalculator.BasePower:0}", "", $"{MissionCalculator.BaseCost}");
        foreach (var i in d.instruments)
            Row(i.displayName, "Instrument", $"{i.mass:0}", $"-{i.power:0}", $"+{i.dataRate:0}", $"{i.cost}");
        if (d.panel != null) Row(d.panel.displayName, "Power", $"{d.panel.mass:0}", $"+{d.panel.generation:0}", "", $"{d.panel.cost}");
        if (d.battery != null) Row(d.battery.displayName, "Power", $"{d.battery.mass:0}", "", "", $"{d.battery.cost}");
        if (d.antenna != null) Row(d.antenna.displayName, "Comms", $"{d.antenna.mass:0}", $"-{d.antenna.power:0}", $"sends {d.antenna.downlink:0}", $"{d.antenna.cost}");
        Row($"Ground stations x{d.stations}", "Ground", "", "", "", $"{MissionCalculator.StationCost * d.stations}");
        if (d.rocket != null) Row(d.rocket.displayName, "Launch", $"lifts {r.capacity:0}", "", "", $"{d.rocket.cost}");

        string power = $"{r.generation - r.consumption:+0;-0;0}";
        Row("TOTAL", "", $"{r.mass:0}", power, $"{r.received:0} reach Earth", $"{r.cost}", true);

        margins.text =
            $"Mass margin  {Margin(r.massMargin)}     Power margin  {Margin(r.energyMargin)}     " +
            $"Budget  {Money(c.budget - r.cost)}     Failure risk  {Risk(r.riskPercent)}     Estimated science  {Science(r.science, c.scienceGoal)}";

        OrbitMap.Build(mapArea, d.orbit);
        orbitText.text = d.orbit == null ? "" :
            $"<b>{d.orbit.displayName}</b>  {d.orbit.altitude}\n" +
            $"<color={Palette.Tag(Palette.TextSecondary)}>Contact with Earth {d.orbit.contact * 100f:0}% · lasts {d.orbit.lifeYears} years · " +
            $"the rocket lifts x{d.orbit.capacityMultiplier.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)} here</color>";
    }

    static string Margin(float m)
    {
        string color = Palette.Tag(m >= 0.10f ? Palette.Green : Palette.Orange);
        return $"<color={color}>{Mathf.RoundToInt(m * 100f)}%</color>";
    }

    string Money(int left)
    {
        if (FreeMode.On) return $"<color={Palette.Tag(Palette.Blue)}>free mode</color>";
        string color = Palette.Tag(left >= 0 ? Palette.Green : Palette.Orange);
        return $"<color={color}>{(left >= 0 ? $"${left}M left" : $"${-left}M over")}</color>";
    }

    static string Risk(int p) => $"<color={Palette.Tag(p <= 10 ? Palette.Green : Palette.Orange)}>{p}%</color>";
    static string Science(int s, int goal) => $"<color={Palette.Tag(s >= goal ? Palette.Green : Palette.Orange)}>{s}/{goal}</color>";

    // Columnas de la tabla (ancho en px): pieza, tipo, masa, energía, datos, costo
    static readonly float[] Widths = { 330f, 130f, 120f, 110f, 170f, 90f };

    void Row(string part, string type, string mass, string power, string data, string cost, bool bold = false)
    {
        var row = UIFactory.Rect("Row", table);
        UIFactory.Layout(row.gameObject, bold ? 40f : 34f);
        if (bold)
        {
            var line = UIFactory.Image("Line", row, null, Palette.CardBorder);
            UIFactory.Place(line.rectTransform, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 1.5f));
        }

        string[] cells = { part, type, mass, power, data, cost };
        float x = 0f;
        for (int i = 0; i < cells.Length; i++)
        {
            bool header = bold && part == "PART";
            var color = header ? Palette.TextSecondary : i == 0 ? Palette.TextMain : Palette.TextSecondary;
            var kind = header || (bold && i == 0) ? FontKind.Title : i >= 2 ? FontKind.Mono : FontKind.Body;
            var t = UIFactory.Label("Cell", row, cells[i], header ? 15 : 21, color, kind, i >= 2 ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
            if (cells[i].StartsWith("+")) t.color = Palette.Green;
            if (bold && !header && i >= 2) t.color = Palette.TextMain;
            if (bold && !header && i == 3) t.color = cells[i].StartsWith("-") ? Palette.Orange : Palette.Green; // energía que sobra o falta
            UILayout.Fit(t, 16);
            UIFactory.Place(t.rectTransform, Vector2.zero, new Vector2(0f, 1f), new Vector2(x, 0f), new Vector2(x + Widths[i] - 10f, 0f));
            x += Widths[i];
        }
    }

    // ---------- Armado ----------

    void Build(RectTransform window)
    {
        var caption = UIFactory.Caption("Caption", window, "Pre-launch review");
        UIFactory.Place(caption.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(40f, -56f), new Vector2(-100f, -28f));
        var title = UIFactory.Label("Title", window, "Master Equipment List (MEL)", 40, Palette.TextMain, FontKind.Title);
        UIFactory.Place(title.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(40f, -108f), new Vector2(-100f, -54f));
        var why = UIFactory.Label("Why", window, "NASA's Mission Design Lab builds this list for every mission concept: each part with its mass, power and cost. Game values are simplified.", 20, Palette.TextSecondary);
        UIFactory.Place(why.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(40f, -146f), new Vector2(-100f, -110f));

        var close = UIFactory.ButtonCard("Close", window, "X", 26, null, null, false);
        UIFactory.Pin((RectTransform)close.transform, Vector2.one, new Vector2(-20f, -20f), new Vector2(56f, 56f));
        close.onClick.AddListener(Close);

        // Tabla (izquierda)
        table = UIFactory.Rect("Table", window);
        UIFactory.Place(table, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(40f, 200f), new Vector2(40f + 950f, -166f));
        var v = UIFactory.Vertical(table.gameObject, 0, 0, 0, 0, 2f);
        v.childAlignment = TextAnchor.UpperLeft;

        // Mapa de la órbita (derecha)
        var mapCard = UIFactory.Image("MapFrame", window, UIFactory.RoundedFill, new Color(0f, 0f, 0f, 0.3f));
        UIFactory.Place(mapCard.rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-40f - 470f, 200f), new Vector2(-40f, -166f));
        var mapCaption = UIFactory.Caption("MapCaption", mapCard.transform, "Orbit map (not to scale)");
        UIFactory.Place(mapCaption.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(20f, -36f), new Vector2(-20f, -10f));
        mapArea = UIFactory.Rect("Map", mapCard.transform);
        UIFactory.Place(mapArea, Vector2.zero, Vector2.one, new Vector2(0f, 70f), new Vector2(0f, -30f));
        
        orbitText = UIFactory.Label("Orbit", mapCard.transform, "", 19, Palette.TextMain, FontKind.Body, TextAnchor.LowerLeft);
        UIFactory.Place(orbitText.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(20f, 12f), new Vector2(-20f, 76f));

        // Márgenes y botones (abajo)
        margins = UIFactory.Label("Margins", window, "", 20, Palette.TextSecondary, FontKind.Body);
        UIFactory.Place(margins.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(40f, 124f), new Vector2(-40f, 180f));

        var back = UIFactory.ButtonCard("Back", window, "BACK TO DESIGN", 26, Palette.TextSecondary, null, false);
        UIFactory.Pin((RectTransform)back.transform, Vector2.zero, new Vector2(40f, 36f), new Vector2(360f, 72f));
        back.onClick.AddListener(Close);

        var go = UIFactory.ButtonCard("Confirm", window, "CONFIRM LAUNCH", 32, null, Palette.Orange, false);
        UIFactory.Pin((RectTransform)go.transform, new Vector2(1f, 0f), new Vector2(-40f, 36f), new Vector2(420f, 72f));
        go.onClick.AddListener(() => controller.Launch());
    }
}
