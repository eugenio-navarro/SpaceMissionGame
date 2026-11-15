using UnityEngine;
using UnityEngine.UI;

// Vista de mapa (UI): la Tierra con las tres órbitas del juego. La elegida se resalta en naranja y
// el satélite la recorre. No está a escala (la Luna queda 770 veces más lejos que la órbita baja).
public class OrbitMap : MonoBehaviour
{
    RectTransform satellite;
    float radius, speed, angle;
    Vector2 center;

    // Radios en px de cada órbita del dibujo y velocidad (grados/s): las bajas giran más rápido.
    static float RadiusFor(string id) => id == "leo" ? 86f : id == "geo" ? 150f : 42f;

    public static OrbitMap Build(RectTransform parent, PartSpec orbit)
    {
        var root = UIFactory.Rect("OrbitMap", parent);
        UIFactory.Stretch(root);
        var map = root.gameObject.AddComponent<OrbitMap>();
        map.Draw(root, orbit);
        return map;
    }

    void Draw(RectTransform root, PartSpec chosen)
    {
        string id = chosen != null ? chosen.id : "";
        var earthCenter = new Vector2(-95f, 0f);
        var moonCenter = new Vector2(180f, 0f);

        Ring(root, earthCenter, RadiusFor("geo"), id == "geo");
        Ring(root, earthCenter, RadiusFor("leo"), id == "leo");
        Ring(root, moonCenter, RadiusFor("lunar"), id == "lunar");

        Disc(root, earthCenter, 52f, Palette.Planet, "EARTH");
        Disc(root, moonCenter, 22f, Palette.Metal, "");
        Label(root, moonCenter + new Vector2(0f, -70f), "MOON", Palette.TextSecondary);
        Label(root, earthCenter + new Vector2(0f, RadiusFor("leo") + 12f), "LOW ~500 km", id == "leo" ? Palette.Orange : Palette.Dim);
        Label(root, earthCenter + new Vector2(0f, RadiusFor("geo") + 12f), "GEOSTATIONARY ~36,000 km", id == "geo" ? Palette.Orange : Palette.Dim);
        Label(root, moonCenter + new Vector2(0f, RadiusFor("lunar") + 12f), "LUNAR\n~384,000 km", id == "lunar" ? Palette.Orange : Palette.Dim);

        if (chosen == null) return;
        center = id == "lunar" ? moonCenter : earthCenter;
        radius = RadiusFor(id);
        speed = id == "leo" ? 70f : id == "geo" ? 18f : 40f;
        var sat = UIFactory.Image("Satellite", root, UIFactory.RoundedFill, Palette.Orange);
        satellite = sat.rectTransform;
        satellite.anchorMin = satellite.anchorMax = satellite.pivot = new Vector2(0.5f, 0.5f);
        satellite.sizeDelta = new Vector2(16f, 16f);
    }

    void Update()
    {
        if (satellite == null) return;
        angle = Mathf.Repeat(angle + speed * Time.unscaledDeltaTime, 360f);
        float a = angle * Mathf.Deg2Rad;
        satellite.anchoredPosition = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
    }

    static void Ring(RectTransform parent, Vector2 at, float r, bool chosen)
    {
        var img = UIFactory.Image("Orbit", parent, ShapeArt.Ring, chosen ? Palette.Orange : new Color(1f, 1f, 1f, 0.2f));
        img.type = Image.Type.Simple;
        UIFactory.Pin(img.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * r * 2f);
        img.rectTransform.anchoredPosition = at;
    }

    static void Disc(RectTransform parent, Vector2 at, float r, Color color, string text)
    {
        var img = UIFactory.Image("Body", parent, ShapeArt.Circle, color);
        img.type = Image.Type.Simple;
        UIFactory.Pin(img.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * r * 2f);
        img.rectTransform.anchoredPosition = at;
        if (text.Length > 0)
            UIFactory.Stretch(UIFactory.Label("Text", img.transform, text, 18, Color.white, FontKind.Title, TextAnchor.MiddleCenter).rectTransform);
    }

    static void Label(RectTransform parent, Vector2 at, string text, Color color)
    {
        var t = UIFactory.Label("Label", parent, text, 17, color, FontKind.Title, TextAnchor.LowerCenter);
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        UIFactory.Pin(t.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 20f));
        t.rectTransform.pivot = new Vector2(0.5f, 0f);
        t.rectTransform.anchoredPosition = at;
    }
}
