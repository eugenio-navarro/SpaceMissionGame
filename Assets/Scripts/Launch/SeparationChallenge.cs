using System;
using UnityEngine;
using UnityEngine.UI;

public enum SeparationRating { Perfect, Early, Late, Automatic }

// Mini desafío del README: soltar la etapa 1 en el momento justo (Espacio o clic).
// Un marcador avanza por una barra; la zona verde es cuando la etapa se queda sin combustible.
public class SeparationChallenge : MonoBehaviour
{
    public const float PerfectWindow = 0.3f; // segundos antes o después del momento justo

    RectTransform marker, card;
    CanvasGroup group;
    Text title, hint;
    float start, end, perfectAt;
    bool done;
    float hideAt;

    public bool Active => gameObject.activeSelf && !done;

    public static SeparationChallenge Create(Transform canvasRoot, Action onPress)
    {
        var root = UIFactory.Rect("SeparationChallenge", canvasRoot);
        UIFactory.Stretch(root);
        var c = root.gameObject.AddComponent<SeparationChallenge>();
        c.Build(root, onPress);
        root.gameObject.SetActive(false);
        return c;
    }

    // start y end: tiempos de vuelo en que se abre y se cierra la ventana; perfectAt: el momento justo.
    public void Begin(float start, float end, float perfectAt)
    {
        this.start = start;
        this.end = end;
        this.perfectAt = perfectAt;
        done = false;
        title.text = "STAGE 1 IS RUNNING OUT OF FUEL";
        title.color = Palette.TextMain;
        hint.text = "Press SPACE (or click here) when the marker is in the green zone";

        // Zona verde
        var zone = card.Find("Track/Zone") as RectTransform;
        zone.anchorMin = new Vector2(Fraction(perfectAt - PerfectWindow), 0f);
        zone.anchorMax = new Vector2(Fraction(perfectAt + PerfectWindow), 1f);
        gameObject.SetActive(true);
        group.alpha = 1f;
    }

    public void Tick(float flightTime)
    {
        if (done)
        {
            if (Time.time > hideAt) group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.deltaTime * 3f);
            if (group.alpha <= 0f) gameObject.SetActive(false);
            return;
        }
        float f = Fraction(flightTime);
        marker.anchorMin = marker.anchorMax = new Vector2(f, 0.5f);
    }

    public static SeparationRating Rate(float pressTime, float perfectAt) =>
        Mathf.Abs(pressTime - perfectAt) <= PerfectWindow ? SeparationRating.Perfect
        : pressTime < perfectAt ? SeparationRating.Early : SeparationRating.Late;

    public void Finish(SeparationRating rating)
    {
        done = true;
        hideAt = Time.time + 1.6f;
        switch (rating)
        {
            case SeparationRating.Perfect:
                title.text = "PERFECT SEPARATION!"; title.color = Palette.Green;
                hint.text = "Right when the fuel ran out. Not a drop wasted.";
                break;
            case SeparationRating.Early:
                title.text = "A BIT EARLY"; title.color = Palette.Orange;
                hint.text = "Stage 1 still had some fuel left.";
                break;
            case SeparationRating.Late:
                title.text = "A BIT LATE"; title.color = Palette.Orange;
                hint.text = "Stage 1 was empty for a moment: dead weight.";
                break;
            default:
                title.text = "AUTOMATIC SEPARATION"; title.color = Palette.TextSecondary;
                hint.text = "The flight computer did it for you.";
                break;
        }
    }

    float Fraction(float t) => Mathf.Clamp01(Mathf.InverseLerp(start, end, t));

    void Build(RectTransform root, Action onPress)
    {
        card = UIFactory.Card("Card", root, Palette.Orange);
        UIFactory.Pin(card, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(640f, 170f));
        group = card.gameObject.AddComponent<CanvasGroup>();

        // Toda la tarjeta es un botón (sirve con mouse o pantalla táctil)
        var button = card.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = card.Find("Fill").GetComponent<Image>();
        button.onClick.AddListener(() => onPress());

        title = UIFactory.Label("Title", card, "", 30, Palette.TextMain, FontKind.Title, TextAnchor.MiddleCenter);
        UIFactory.Place(title.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(24f, -60f), new Vector2(-24f, -14f));

        var track = UIFactory.Image("Track", card, UIFactory.RoundedFill, new Color(1f, 1f, 1f, 0.1f));
        UIFactory.Place(track.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(40f, -14f), new Vector2(-40f, 14f));
        var zone = UIFactory.Image("Zone", track.transform, UIFactory.RoundedFill, Palette.Green);
        UIFactory.Place(zone.rectTransform, new Vector2(0.7f, 0f), new Vector2(0.8f, 1f), Vector2.zero, Vector2.zero);

        var m = UIFactory.Image("Marker", track.transform, UIFactory.RoundedFill, Palette.Orange);
        marker = m.rectTransform;
        marker.sizeDelta = new Vector2(10f, 44f);
        marker.pivot = new Vector2(0.5f, 0.5f);
        marker.anchoredPosition = Vector2.zero;

        hint = UIFactory.Label("Hint", card, "", 20, Palette.TextSecondary, FontKind.Body, TextAnchor.MiddleCenter);
        UIFactory.Place(hint.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(24f, 12f), new Vector2(-24f, 52f));
    }
}
