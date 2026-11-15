using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla de resultados: muestra el satélite sobre el planeta y explica por qué salió así.
public class ResultsController : MonoBehaviour
{
    public string hubScene = "CentroEspacial";
    public string hangarScene = "Hangar";

    [Header("Mundo")]
    public SatelliteVisual satellite;
    public Light sun;
    public GameObject earth, moon;

    [Header("UI")]
    public Image resultAccent;
    public Text resultCaption, statusText, scienceText;
    public Text whyText, tipText;
    public Text eventTitle, eventText, riskText;
    public Text missionName, missionInfo;
    public Text tradeText;
    public Button tryAgainButton, hubButton;

    MissionOutcome outcome;
    Vector3 spinAxis;

    void Start()
    {
        var contract = TestCatalog.Contract(HubController.AcceptedContractId);
        var design = HangarController.LastDesign ?? TestDesign();
        Present(OutcomeCalculator.Roll(design, contract, new System.Random()), design, contract);

        tryAgainButton.onClick.AddListener(() => SceneManager.LoadScene(hangarScene));
        hubButton.onClick.AddListener(() =>
        {
            HangarController.LastDesign = null;
            SceneManager.LoadScene(hubScene);
        });
    }

    // Muestra un resultado. Público para que Lanzamiento (o una prueba) pase su propio resultado.
    public void Present(MissionOutcome result, MissionDesign design, ContractSpec contract)
    {
        outcome = result;
        ShowWorld(design, contract);
        ShowUI(contract);
    }

    void Update()
    {
        // Si falló, la nave gira sin control.
        if (outcome != null && outcome.status == MissionStatus.Failed)
            satellite.satellite.Rotate(spinAxis, 25f * Time.deltaTime, Space.World);
    }

    void ShowWorld(MissionDesign design, ContractSpec contract)
    {
        satellite.Show(design);
        bool lunar = contract.id == "moon";
        earth.SetActive(!lunar);
        moon.SetActive(lunar);

        bool good = outcome.status == MissionStatus.Success;
        sun.intensity = good ? 1.6f : outcome.status == MissionStatus.Partial ? 1.1f : 0.45f;
        spinAxis = new Vector3(0.3f, 1f, 0.5f).normalized;
    }

    void ShowUI(ContractSpec c)
    {
        var o = outcome;
        Color statusColor = o.status == MissionStatus.Success ? Palette.Green : Palette.Orange;
        resultAccent.color = statusColor;
        resultCaption.text = $"MISSION RESULT  ·  {c.title.ToUpperInvariant()}";
        statusText.text = o.status == MissionStatus.Success ? "SUCCESS" : o.status == MissionStatus.Partial ? "PARTIAL" : "FAILED";
        statusText.color = statusColor;
        scienceText.color = statusColor;
        scienceText.GetComponent<CountUp>().Play(o.finalScience, "{0}<color=" + Palette.Tag(Palette.TextSecondary) + ">/" + c.scienceGoal + " science</color>");

        whyText.text = o.why;
        tipText.text = o.tip;

        eventTitle.text = o.eventTitle;
        eventTitle.color = o.eventHurt ? Palette.Orange : Palette.TextMain;
        eventText.text = o.eventText;
        riskText.text = o.riskFailed
            ? $"Risk roll: {o.report.riskPercent}%  ·  <color={Palette.Tag(Palette.Orange)}>FAILED</color>"
            : $"Risk roll: {o.report.riskPercent}%  ·  <color={Palette.Tag(Palette.Green)}>PASSED</color>";

        missionName.text = c.similarMission;
        missionInfo.text = c.similarMissionInfo;
        tradeText.text = o.tradeStudy;
    }

    // Diseño de prueba (solución de referencia de Tierra) para abrir la escena sola.
    static MissionDesign TestDesign()
    {
        var d = new MissionDesign
        {
            panel = TestCatalog.Get("panel_l"), battery = TestCatalog.Get("bat"), antenna = TestCatalog.Get("ant_l"),
            rocket = TestCatalog.Get("chico"), orbit = TestCatalog.Get("leo"), stations = 2,
        };
        d.instruments.Add(TestCatalog.Get("cam_a"));
        return d;
    }
}
