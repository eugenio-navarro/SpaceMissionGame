using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

// HUD del lanzamiento: reloj de misión, altímetro con barra hacia la órbita, lista de etapas,
// línea de CAPCOM, cartel de evento y botón Saltar.
public class LaunchHud : MonoBehaviour
{
    [Header("Misión")]
    public Text missionName;
    public Text missionDetail;

    [Header("Reloj y altímetro")]
    public Text clockText;
    public Text altitudeText;
    public RectTransform altitudeFill;
    public Image altitudeFillImage;
    public RectTransform spaceTick;
    public Text orbitLabel;

    [Header("Etapas")]
    public RectTransform stagePanel;
    public Text[] stageRows;
    public Image[] stageDots;
    public Image[] stageRings;
    public Image[] stageLinks;

    [Header("CAPCOM")]
    public CanvasGroup statusGroup;
    public Text statusText;

    [Header("Evento")]
    public CanvasGroup eventGroup;
    public RectTransform eventCard;
    public Image eventBorder;
    public Image eventIcon;
    public Text eventTitle;
    public Text eventBody;

    [Header("Otros")]
    public Image fade;
    public Button skipButton;

    [Header("Colores")]
    public Color green = new Color(0.48f, 0.85f, 0.56f);
    public Color orange = new Color(0.94f, 0.47f, 0.16f);
    public Color white = new Color(0.93f, 0.96f, 1f);
    public Color muted = new Color(0.56f, 0.64f, 0.72f);
    public Color spent = new Color(0.30f, 0.35f, 0.41f);

    const float SpaceLineKm = 100f; // línea de Kármán: donde empieza el espacio

    string[] stageNames;
    float orbitAltitudeM = 500000f;
    int activeStage;
    string currentStatus = "";
    float eventTimer = -1f;

    public void Setup(LaunchInfo info)
    {
        stageNames = info.stageNames;
        orbitAltitudeM = Mathf.Max(1f, info.orbitAltitudeKm * 1000f);

        missionName.text = info.missionName;
        missionDetail.text = info.rocketName + "  /  " + info.orbitName;
        orbitLabel.text = "ORBIT " + info.orbitAltitudeKm.ToString("#,0", CultureInfo.InvariantCulture) + " km";

        // Marca de "espacio" a 100 km; si la órbita es muy alta no entra en la barra.
        float spaceFraction = SpaceLineKm / Mathf.Max(1f, info.orbitAltitudeKm);
        spaceTick.gameObject.SetActive(spaceFraction > 0.04f && spaceFraction < 0.9f);
        spaceTick.anchorMin = new Vector2(spaceFraction, 0f);
        spaceTick.anchorMax = new Vector2(spaceFraction, 1f);

        int count = Mathf.Min(stageNames.Length, stageRows.Length);
        for (int i = 0; i < stageRows.Length; i++)
        {
            bool used = i < count;
            stageRows[i].gameObject.SetActive(used);
            stageDots[i].gameObject.SetActive(used);
            stageRings[i].gameObject.SetActive(used);
            stageLinks[i].gameObject.SetActive(i < count - 1);
        }
        stagePanel.sizeDelta = new Vector2(stagePanel.sizeDelta.x, 74f + count * 52f);

        SetActiveStage(0);
        SetStatus("");
        SetAltitude(0f);
        eventGroup.alpha = 0f;
        eventGroup.gameObject.SetActive(false);
        SetFade(0f);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        // La etapa activa late.
        if (stageRings != null && activeStage < stageRings.Length)
        {
            float pulse = Mathf.Repeat(Time.unscaledTime * 0.9f, 1f);
            Image ring = stageRings[activeStage];
            ring.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 2.1f, pulse);
            Color c = orange;
            c.a = 1f - pulse;
            ring.color = c;
        }

        // El texto de CAPCOM entra con un fundido.
        float target = string.IsNullOrEmpty(currentStatus) ? 0f : 1f;
        statusGroup.alpha = Mathf.MoveTowards(statusGroup.alpha, target, dt * 5f);

        // El cartel de evento aparece con un pequeño rebote.
        if (eventTimer >= 0f)
        {
            eventTimer += dt;
            float k = Mathf.Clamp01(eventTimer / 0.4f);
            float back = 1f + 2.2f * Mathf.Pow(k - 1f, 3f) + 1.2f * Mathf.Pow(k - 1f, 2f); // ease-out con rebote
            eventCard.localScale = Vector3.one * Mathf.LerpUnclamped(0.8f, 1f, back);
            eventGroup.alpha = Mathf.Clamp01(eventTimer / 0.2f);
        }
    }

    // seconds < 0 es cuenta regresiva (T-), >= 0 es tiempo de misión (T+).
    public void SetClock(float seconds)
    {
        int total = seconds < 0f ? Mathf.CeilToInt(-seconds) : Mathf.FloorToInt(seconds);
        string sign = seconds < 0f ? "T-" : "T+";
        clockText.text = string.Format("{0} {1:00}:{2:00}", sign, total / 60, total % 60);
    }

    public void SetAltitude(float meters)
    {
        string number, unit;
        if (meters < 1000000f)
        {
            number = Mathf.FloorToInt(meters).ToString("000,000", CultureInfo.InvariantCulture);
            unit = "m";
        }
        else
        {
            number = (meters / 1000f).ToString("#,0", CultureInfo.InvariantCulture);
            unit = "km";
        }
        altitudeText.text = number + "<size=30><color=#8FA3B8> " + unit + "</color></size>";

        float p = Mathf.Clamp01(meters / orbitAltitudeM);
        altitudeFill.anchorMax = new Vector2(p, 1f);
        altitudeFillImage.color = p >= 0.999f ? green : orange;
    }

    public void SetStatus(string text)
    {
        if (text == currentStatus) return;
        currentStatus = text;
        if (!string.IsNullOrEmpty(text))
        {
            statusText.text = text;
            statusGroup.alpha = 0f;
        }
    }

    // Las etapas anteriores quedan apagadas, la actual en naranja y las siguientes en verde.
    public void SetActiveStage(int index)
    {
        activeStage = index;
        for (int i = 0; i < stageRows.Length && i < stageNames.Length; i++)
        {
            stageRows[i].text = stageNames[i];
            bool done = i < index;
            bool active = i == index;

            stageRows[i].color = done ? spent : active ? white : muted;
            stageDots[i].color = done ? spent : active ? orange : green;
            stageLinks[i].color = done ? spent : new Color(muted.r, muted.g, muted.b, 0.45f);
            if (!active)
            {
                stageRings[i].color = Color.clear;
                stageRings[i].rectTransform.localScale = Vector3.one;
            }
        }
    }

    // good = true pinta el cartel en verde (no pasó nada malo); false en naranja.
    public void ShowEvent(string title, string body, bool good)
    {
        Color accent = good ? green : orange;
        eventTitle.text = title;
        eventTitle.color = accent;
        eventBody.text = body;
        eventBorder.color = accent;
        eventIcon.color = accent;

        eventGroup.gameObject.SetActive(true);
        eventGroup.alpha = 0f;
        eventTimer = 0f;
    }

    public void SetFade(float alpha)
    {
        if (fade == null) return;
        Color c = fade.color;
        c.a = alpha;
        fade.color = c;
        fade.raycastTarget = alpha > 0.01f;
    }
}
