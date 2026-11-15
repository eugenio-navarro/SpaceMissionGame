using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Menú principal en 3D: clics en edificios, panel de contratos, carteles y tooltip.
public class HubController : MonoBehaviour
{
    // TEMPORAL hasta que exista MissionState: el contrato aceptado queda guardado (sobrevive al cerrar el juego).
    public static string AcceptedContractId
    {
        get => SaveData.AcceptedContract;
        set => SaveData.AcceptedContract = value;
    }

    public string hangarScene = "Hangar";

    [Header("UI")]
    public RectTransform tooltip;
    public Text tooltipText;
    public GameObject contractsPanel;
    public GameObject infoPanel;
    public Text infoTitle;
    public Text infoBody;
    public Text contractTitle;
    public Text contractDetail;
    public GameObject toast;
    public Text toastText;
    public Button menuButton;

    float toastUntil;
    CanvasGroup toastGroup;
    Canvas canvas;
    bool titleOpen;

    bool AnyPanelOpen => contractsPanel.activeSelf || infoPanel.activeSelf;

    // Mientras hay un panel, el título o la pausa, los edificios no responden.
    public bool Busy => AnyPanelOpen || titleOpen || (GameShell.Instance != null && GameShell.Instance.MenuOpen);

    void Start()
    {
        canvas = tooltip.GetComponentInParent<Canvas>().rootCanvas;
        toastGroup = toast.GetComponent<CanvasGroup>();
        toast.SetActive(false);
        CloseAll();
        HideTooltip();
        RefreshStatus();
        if (menuButton != null && GameShell.Instance != null) menuButton.onClick.AddListener(GameShell.Instance.OpenMenu);
        GameShell.SceneEscape = HandleEscape;

        // La primera vez de la sesión se ve el título encima del centro espacial.
        titleOpen = GameShell.ShowTitleOnce(Enter);
        if (titleOpen) canvas.enabled = false;
        else Enter();
    }

    void OnDestroy()
    {
        if (GameShell.SceneEscape == HandleEscape) GameShell.SceneEscape = null;
    }

    void Enter()
    {
        titleOpen = false;
        canvas.enabled = true;
        RefreshStatus();
        GameShell.Tutorial("tut_hub", new[]
        {
            "Welcome, mission designer! Your job: plan a satellite mission that brings science back to Earth.",
            "Click <b>Mission Control</b> to pick a contract. Each one has a science goal and a budget.",
            "Then click the <b>Assembly Hangar</b> to build your satellite and launch it.",
        }, UIFactory.Margin + 172f, 720f, () => Busy);
    }

    // Esc cierra el panel abierto; si no hay ninguno, deja que se abra la pausa.
    bool HandleEscape()
    {
        if (!AnyPanelOpen) return false;
        CloseAll();
        GameShell.Play(Sfx.Click);
        return true;
    }

    void Update()
    {
        if (tooltip.gameObject.activeSelf && Mouse.current != null)
            tooltip.position = Mouse.current.position.ReadValue() + new Vector2(20f, -20f) * canvas.scaleFactor;

        if (toast.activeSelf && Time.unscaledTime > toastUntil)
        {
            toastGroup.alpha = Mathf.MoveTowards(toastGroup.alpha, 0f, Time.unscaledDeltaTime * 3f);
            if (toastGroup.alpha <= 0f) toast.SetActive(false);
        }
    }

    public void ShowTooltip(string text)
    {
        if (Busy) return;
        tooltipText.text = text;
        tooltip.gameObject.SetActive(true);
    }

    public void HideTooltip() => tooltip.gameObject.SetActive(false);

    public void OnBuildingClicked(BuildingKind kind)
    {
        if (Busy) return;
        switch (kind)
        {
            case BuildingKind.MissionControl:
                toast.SetActive(false);
                contractsPanel.SetActive(true);
                break;
            case BuildingKind.Hangar:
                if (string.IsNullOrEmpty(AcceptedContractId))
                {
                    GameShell.Play(Sfx.Deny);
                    Toast("First pick a contract at Mission Control.");
                }
                else GameShell.Load(hangarScene);
                break;
            case BuildingKind.Antennas:
                ShowInfo("Ground Stations",
                    "Antennas on Earth that talk to your satellite. More stations = more data reaches home.\n\n" +
                    "Coming soon. For now, pick them in the Hangar (Comms tab).");
                break;
            case BuildingKind.Dome:
                ShowInfo("Mission Archive", ArchiveText());
                break;
        }
    }

    // Mejores resultados guardados de cada contrato.
    static string ArchiveText()
    {
        int flown = SaveData.MissionsFlown;
        if (flown == 0) return "No missions flown yet.\n\nLaunch your first satellite and its result will be saved here.";

        var sb = new StringBuilder();
        sb.AppendLine($"Missions flown: <color={Palette.Tag(Palette.TextMain)}>{flown}</color>\n");
        foreach (var c in TestCatalog.Contracts)
        {
            int rank = SaveData.BestRank(c.title);
            if (rank == 0) continue;
            string label = rank == 3 ? $"<color={Palette.Tag(Palette.Green)}>SUCCESS</color>"
                : rank == 2 ? $"<color={Palette.Tag(Palette.Orange)}>PARTIAL</color>"
                : $"<color={Palette.Tag(Palette.Orange)}>FAILED</color>";
            sb.AppendLine($"<color={Palette.Tag(Palette.TextMain)}>{c.title}</color>   best: {label}   science {SaveData.BestScience(c.title)}/{c.scienceGoal}");
        }
        return sb.ToString();
    }

    public void AcceptContract(int index)
    {
        var contract = TestCatalog.Contracts[index];
        if (!contract.unlocked) return;
        AcceptedContractId = contract.id;
        CloseAll();
        RefreshStatus();
        GameShell.Play(Sfx.Accept);
        Toast($"Contract accepted: {contract.title}. Now head to the Assembly Hangar!");
    }

    public void CloseAll()
    {
        contractsPanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    void ShowInfo(string title, string body)
    {
        infoTitle.text = title;
        infoBody.text = body;
        infoPanel.SetActive(true);
    }

    void RefreshStatus()
    {
        var c = string.IsNullOrEmpty(AcceptedContractId) ? null : TestCatalog.Contract(AcceptedContractId);
        if (c == null)
        {
            contractTitle.text = "None yet";
            contractTitle.color = Palette.Dim;
            contractDetail.text = "Visit Mission Control to pick one";
            return;
        }
        contractTitle.text = c.title;
        contractTitle.color = Palette.TextMain;
        contractDetail.text = $"Goal {c.scienceGoal} science  ·  Budget ${c.budget}M";
    }

    void Toast(string message)
    {
        toastText.text = message;
        toast.transform.SetAsLastSibling(); // siempre por encima de los paneles
        toast.SetActive(false); // reinicia la animación de entrada
        toast.SetActive(true);
        toastUntil = Time.unscaledTime + 3f;
    }
}
