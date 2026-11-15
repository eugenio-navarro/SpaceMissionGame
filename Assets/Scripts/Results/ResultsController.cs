using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla de resultados: muestra el satélite sobre el planeta del contrato y el panel
// con el puntaje, los recursos, por qué salió así y qué probar.
public class ResultsController : MonoBehaviour
{
    [Header("Escena 3D")]
    public Transform satellite;
    public SatelliteVisual parts; // muestra solo las piezas que eligió el jugador
    public Transform[] solarPanels;
    public Renderer[] panelRenderers;
    public Light sun;
    public Transform planet;
    public Renderer planetRenderer;
    public Texture earthTexture;
    public Texture moonTexture;
    public GameObject atmosphere;

    [Header("Panel")]
    public RectTransform panel;
    public Image resultIcon;
    public Text resultIconText;
    public Text resultTitle;
    public Text resultHeadline;
    public Text scienceText;
    public RectTransform scienceFill;
    public Image scienceFillImage;
    public RectTransform goalTick;
    public Text[] statValues;          // masa, energía, presupuesto, datos
    public RectTransform[] statFills;
    public Image[] statFillImages;
    public Text whyText;
    public Text tryNextText;
    public Text similarName;
    public Text similarNote;
    public Button hubButton;
    public Button redesignButton;

    [Header("Evento y trade study")]
    public CanvasGroup eventCard;
    public Text eventTitleText;
    public Text eventBodyText;
    public Text riskText;
    public CanvasGroup tradeCard;
    public Text tradeText;

    [Header("Otros")]
    public Text missionName;
    public Text missionDetail;
    public CanvasGroup capcomGroup;
    public Text capcomText;
    public Image fade;

    [Header("Colores")]
    public Color green = new Color(0.48f, 0.85f, 0.56f);
    public Color orange = new Color(0.94f, 0.47f, 0.16f);

    public string hubScene = "CentroEspacial";
    public string hangarScene = "Hangar";

    [Tooltip("Solo para probar la escena sola: Success, Partial o Failed. Vacío = sorteo normal.")]
    public string testResult = "";

    ResultsInfo run;
    float panelShownX;
    bool leaving;

    void Start()
    {
        run = ResultsContext.GetOrCreate();
        if (ResultsContext.IsTestData && !string.IsNullOrEmpty(testResult)) ApplyTestResult();
        if (!run.resolved)
            MissionOutcome.Resolve(run, (LaunchEventKind)Random.Range(0, 3), Random.Range(0f, 100f));

        SetupWorld();
        FillPanel();

        hubButton.onClick.AddListener(() => Leave(hubScene, true));
        redesignButton.onClick.AddListener(() => Leave(hangarScene, false));

        StartCoroutine(Intro());
    }

    // Fuerza un resultado cambiando el diseño de prueba (para ver los tres estados sin jugar).
    void ApplyTestResult()
    {
        if (testResult == "Partial")
        {
            run.downlink = 32f; // antena chica: baja menos de lo que se genera
            MissionOutcome.Resolve(run, LaunchEventKind.StationInterference, 99f);
        }
        else if (testResult == "Failed")
        {
            run.consumption = 235f; // margen de energía muy fino
            run.riskPercent = 30;
            MissionOutcome.Resolve(run, LaunchEventKind.SolarStorm, 0f);
        }
        else
        {
            run.downlink = 60f;
            MissionOutcome.Resolve(run, LaunchEventKind.Calm, 99f);
        }
    }

    void SetupWorld()
    {
        if (parts != null) parts.Show(run.design ?? ReferenceDesign());

        bool moon = run.destination == "MOON";
        if (planetRenderer != null)
        {
            Texture tex = moon ? moonTexture : earthTexture;
            if (tex != null) planetRenderer.material.SetTexture("_BaseMap", tex);
        }
        if (atmosphere != null) atmosphere.SetActive(!moon);

        // Si la misión falló, el satélite queda apagado: paneles oscuros y menos luz.
        if (run.result == ResultKind.Failed)
        {
            foreach (Renderer r in panelRenderers)
                r.material.SetColor("_BaseColor", new Color(0.08f, 0.09f, 0.12f));
            if (sun != null) sun.intensity *= 0.55f;
        }

        foreach (Transform p in solarPanels) p.localScale = new Vector3(0.02f, 1f, 1f);
    }

    void FillPanel()
    {
        // Guía de estética: verde = todo bien, naranja = alerta (igual que en el Hangar).
        Color accent = run.result == ResultKind.Success ? green : orange;

        resultTitle.text = run.result == ResultKind.Success ? "MISSION SUCCESS"
            : run.result == ResultKind.Partial ? "PARTIAL SUCCESS" : "MISSION FAILED";
        resultTitle.color = accent;
        resultIcon.color = accent;
        resultIconText.text = run.result == ResultKind.Success ? "OK" : run.result == ResultKind.Partial ? "~" : "X";
        resultHeadline.text = run.headline;

        missionName.text = run.contractTitle;
        missionDetail.text = run.rocketName + "  /  " + run.orbitName;

        // Barra de ciencia: el ancho total representa 1,5 veces la meta.
        float scale = Mathf.Max(1f, run.scienceGoal * 1.5f);
        float goalAt = run.scienceGoal / scale;
        goalTick.anchorMin = new Vector2(goalAt, 0f);
        goalTick.anchorMax = new Vector2(goalAt, 1f);
        scienceFillImage.color = accent;
        SetScience(0f);

        SetStat(0, run.mass.ToString("0") + " / " + run.capacity.ToString("0") + " kg",
            run.capacity > 0f ? run.mass / run.capacity : 0f, MarginColor(run.MassMargin));
        SetStat(1, run.consumption.ToString("0") + " / " + run.generation.ToString("0") + " W",
            run.generation > 0f ? run.consumption / run.generation : 0f, MarginColor(run.EnergyMargin));
        SetStat(2, "$" + run.cost + "M / $" + run.budget + "M",
            run.budget > 0 ? run.cost / (float)run.budget : 0f, MarginColor(run.BudgetMargin));
        float dataRatio = run.dataGenerated > 0f ? run.received / run.dataGenerated : 0f;
        SetStat(3, run.received.ToString("0") + " of " + run.dataGenerated.ToString("0") + " GB/day",
            dataRatio, dataRatio > 0.99f ? green : orange);

        whyText.text = run.why;
        tryNextText.text = run.tryNext;
        similarName.text = run.similarMission;
        similarNote.text = run.similarMissionNote;
        capcomText.text = run.capcomLine;

        // Evento de la simulación y dado de riesgo
        eventTitleText.text = run.eventTitle;
        eventTitleText.color = run.eventHurt ? orange : new Color(0.93f, 0.96f, 1f);
        eventBodyText.text = run.eventText;
        riskText.text = "Risk roll: " + run.riskPercent + "%  ·  " + (run.riskFailure
            ? "<color=#F0782A>FAILED</color>"
            : "<color=#7BD88F>PASSED</color>");

        // Trade study: el cambio de una sola pieza que habría dado mejor resultado
        var contract = TestCatalog.Contracts.Find(x => x.title == run.contractTitle) ?? TestCatalog.Contracts[0];
        tradeText.text = TradeStudy.Best(run.design ?? ReferenceDesign(), contract);
    }

    // Solución de referencia "Tierra" del README (coincide con los valores por defecto de ResultsInfo).
    static MissionDesign ReferenceDesign()
    {
        var d = new MissionDesign
        {
            panel = TestCatalog.Get("panel_l"), battery = TestCatalog.Get("bat"), antenna = TestCatalog.Get("ant_l"),
            rocket = TestCatalog.Get("chico"), orbit = TestCatalog.Get("leo"), stations = 2,
        };
        d.instruments.Add(TestCatalog.Get("cam_a"));
        return d;
    }

    // Igual que las barras del Hangar: verde si sobra 10% o más, naranja si queda menos.
    Color MarginColor(float margin) => margin >= 0.10f ? green : orange;

    void SetStat(int i, string value, float ratio, Color color)
    {
        statValues[i].text = value;
        statFills[i].anchorMax = new Vector2(0f, 1f);
        statFillImages[i].color = color;
        statFills[i].GetComponent<StatTarget>().target = Mathf.Clamp01(ratio);
    }

    void SetScience(float value)
    {
        scienceText.text = Mathf.RoundToInt(value) + "<size=34><color=#8FA3B8>  / " + run.scienceGoal + " goal</color></size>";
        float scale = Mathf.Max(1f, run.scienceGoal * 1.5f);
        scienceFill.anchorMax = new Vector2(Mathf.Clamp01(value / scale), 1f);
    }

    IEnumerator Intro()
    {
        // Entra desde negro mostrando la toma del satélite.
        panelShownX = panel.anchoredPosition.x;
        panel.anchoredPosition = new Vector2(panelShownX + 760f, panel.anchoredPosition.y);
        capcomGroup.alpha = 0f;
        eventCard.alpha = 0f;
        tradeCard.alpha = 0f;

        bool deploy = run.result != ResultKind.Failed;
        for (float t = 0f; t < 1.4f; t += Time.deltaTime)
        {
            SetFade(1f - Mathf.Clamp01(t / 0.7f));
            if (deploy) SetPanels(Mathf.SmoothStep(0.02f, 1f, t / 1.4f));
            yield return null;
        }
        SetFade(0f);
        if (deploy) SetPanels(1f);
        else SetPanels(0.35f);

        // El panel entra desde la derecha.
        for (float t = 0f; t < 0.6f; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / 0.6f, 3f);
            panel.anchoredPosition = new Vector2(Mathf.Lerp(panelShownX + 760f, panelShownX, k), panel.anchoredPosition.y);
            yield return null;
        }
        panel.anchoredPosition = new Vector2(panelShownX, panel.anchoredPosition.y);

        // El puntaje cuenta hacia arriba y las barras se llenan.
        for (float t = 0f; t < 1.2f; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / 1.2f, 3f);
            SetScience(run.science * k);
            for (int i = 0; i < statFills.Length; i++)
                statFills[i].anchorMax = new Vector2(statFills[i].GetComponent<StatTarget>().target * k, 1f);
            capcomGroup.alpha = Mathf.Clamp01(t / 0.5f);
            eventCard.alpha = Mathf.Clamp01(t / 0.4f);
            tradeCard.alpha = Mathf.Clamp01((t - 0.25f) / 0.4f);
            yield return null;
        }
        SetScience(run.science);
        for (int i = 0; i < statFills.Length; i++)
            statFills[i].anchorMax = new Vector2(statFills[i].GetComponent<StatTarget>().target, 1f);
        capcomGroup.alpha = 1f;
        eventCard.alpha = 1f;
        tradeCard.alpha = 1f;
    }

    void Update()
    {
        if (satellite == null || run == null) return;

        // Si falló, el satélite gira sin control; si no, rota apenas.
        if (run.result == ResultKind.Failed)
            satellite.Rotate(new Vector3(18f, 26f, 11f) * Time.deltaTime, Space.World);
        else
            satellite.Rotate(Vector3.up, 4f * Time.deltaTime, Space.World);

        if (planet != null) planet.Rotate(Vector3.up, 1.2f * Time.deltaTime, Space.Self);
    }

    void SetPanels(float open)
    {
        foreach (Transform p in solarPanels) p.localScale = new Vector3(open, 1f, 1f);
    }

    void Leave(string scene, bool clearRun)
    {
        if (leaving) return;
        leaving = true;
        if (clearRun) ResultsContext.Clear();
        else if (ResultsContext.Current != null) ResultsContext.Current.resolved = false; // se vuelve a sortear al relanzar
        StartCoroutine(FadeOut(scene));
    }

    IEnumerator FadeOut(string scene)
    {
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            SetFade(t / 0.5f);
            yield return null;
        }
        SetFade(1f);
        SceneManager.LoadScene(scene);
    }

    void SetFade(float alpha)
    {
        Color c = fade.color;
        c.a = alpha;
        fade.color = c;
        fade.raycastTarget = alpha > 0.01f;
    }
}
