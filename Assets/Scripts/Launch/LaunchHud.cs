using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

// HUD del lanzamiento: reloj de misión, altímetro, lista de etapas, cartel de evento y botón Saltar.
public class LaunchHud : MonoBehaviour
{
    public Text missionText;
    public Text clockText;
    public Text altitudeText;
    public Text statusText;
    public Text[] stageRows;
    public GameObject eventCard;
    public Text eventTitle;
    public Text eventBody;
    public Image fade;
    public Button skipButton;

    public Color stageActive = new Color(0.94f, 0.47f, 0.16f);   // naranja
    public Color stagePending = new Color(0.48f, 0.85f, 0.56f);  // verde fósforo
    public Color stageSpent = new Color(0.35f, 0.38f, 0.42f);    // gris apagado

    string[] stageNames;

    public void Setup(LaunchInfo info)
    {
        stageNames = info.stageNames;
        if (missionText != null) missionText.text = info.missionName.ToUpperInvariant() + "  |  " + info.rocketName;

        for (int i = 0; i < stageRows.Length; i++)
        {
            bool used = i < stageNames.Length;
            stageRows[i].gameObject.SetActive(used);
        }

        SetActiveStage(0);
        SetStatus("");
        SetAltitude(0f);
        if (eventCard != null) eventCard.SetActive(false);
        SetFade(0f);
    }

    // seconds < 0 es cuenta regresiva (T-), >= 0 es tiempo de misión (T+).
    public void SetClock(float seconds)
    {
        int total = Mathf.FloorToInt(Mathf.Abs(seconds));
        if (seconds < 0f) total = Mathf.CeilToInt(Mathf.Abs(seconds));
        string sign = seconds < 0f ? "T-" : "T+";
        clockText.text = string.Format("{0} {1:00}:{2:00}", sign, total / 60, total % 60);
    }

    public void SetAltitude(float meters)
    {
        if (meters < 1000000f)
            altitudeText.text = Mathf.FloorToInt(meters).ToString("000,000", CultureInfo.InvariantCulture) + " m";
        else
            altitudeText.text = (meters / 1000f).ToString("#,0", CultureInfo.InvariantCulture) + " km";
    }

    public void SetStatus(string text)
    {
        statusText.text = text;
    }

    // Las etapas anteriores quedan apagadas, la actual en naranja y las siguientes en verde.
    public void SetActiveStage(int index)
    {
        for (int i = 0; i < stageRows.Length && i < stageNames.Length; i++)
        {
            if (i < index)
            {
                stageRows[i].color = stageSpent;
                stageRows[i].text = "   " + stageNames[i];
            }
            else if (i == index)
            {
                stageRows[i].color = stageActive;
                stageRows[i].text = ">  " + stageNames[i];
            }
            else
            {
                stageRows[i].color = stagePending;
                stageRows[i].text = "   " + stageNames[i];
            }
        }
    }

    public void ShowEvent(string title, string body)
    {
        eventTitle.text = title;
        eventBody.text = body;
        eventCard.SetActive(true);
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
