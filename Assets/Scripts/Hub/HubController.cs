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
    public Button[] contractButtons; // uno por contrato, en el orden de TestCatalog.Contracts
    public Text[] contractStatus;
    public Image[] contractAccents;
    public Button freeModeButton;
    public Text freeModeHint;

    float toastUntil;
    CanvasGroup toastGroup;
    Canvas canvas;
    StationsPanel stationsPanel;
    ArchivePanel archivePanel;

    bool AnyPanelOpen => contractsPanel.activeSelf || infoPanel.activeSelf
        || stationsPanel.gameObject.activeSelf || archivePanel.gameObject.activeSelf;

    // Mientras hay un panel, el título o la pausa, los edificios no responden.
    public bool Busy => AnyPanelOpen || (GameShell.Instance != null && GameShell.Instance.MenuOpen);

    void Start()
    {
        canvas = tooltip.GetComponentInParent<Canvas>().rootCanvas;
        stationsPanel = StationsPanel.Create(canvas.transform, this);
        archivePanel = ArchivePanel.Create(canvas.transform, this);
        tooltip.SetAsLastSibling(); // tooltip y aviso, por encima de los paneles nuevos
        toastGroup = toast.GetComponent<CanvasGroup>();
        toast.SetActive(false);
        CloseAll();
        HideTooltip();
        RefreshStatus();
        if (menuButton != null && GameShell.Instance != null) menuButton.onClick.AddListener(GameShell.Instance.OpenMenu);
        GameShell.SceneEscape = HandleEscape;

        StartTutorial();
    }

    void OnDestroy()
    {
        if (GameShell.SceneEscape == HandleEscape) GameShell.SceneEscape = null;
        BuildingInteractable.Highlighted = null;
    }

    // Tutorial guiado: cada paso espera a que el jugador haga lo que se pide y resalta el edificio.
    void StartTutorial()
    {
        bool Accepted() => !string.IsNullOrEmpty(AcceptedContractId);
        // Mientras guía el tutorial, se esconde el cartel genérico de abajo (queda en el mismo lugar).
        var hintCard = canvas.transform.Find("HintCard");
        if (hintCard != null) hintCard.gameObject.SetActive(SaveData.Seen("tut_hub"));
        GameShell.Tutorial("tut_hub", new[]
        {
            new TutorialStep("Welcome, mission designer! This is your space center. Your job: plan a satellite mission that brings science back to Earth."),
            new TutorialStep("First you need a mission. Click <b>Mission Control</b>: the building that is pulsing.",
                () => contractsPanel.activeSelf || Accepted(),
                () => BuildingInteractable.Highlighted = BuildingKind.MissionControl),
            new TutorialStep("Each contract has a <b>science goal</b> to reach and a <b>budget</b> you can't go over. Press <b>ACCEPT</b> on \"Observe the Earth\".",
                Accepted,
                () => BuildingInteractable.Highlighted = Accepted() ? (BuildingKind?)null : BuildingKind.MissionControl),
            new TutorialStep("Contract accepted! Now click the <b>Assembly Hangar</b> (the big pulsing building) to build your satellite.",
                () => false, // termina al cambiar de escena
                () => { BuildingInteractable.Highlighted = BuildingKind.Hangar; SaveData.MarkSeen("tut_hub"); }),
        }, 4f, 1000f, () => GameShell.Instance != null && GameShell.Instance.MenuOpen || stationsPanel.gameObject.activeSelf || archivePanel.gameObject.activeSelf,
        () => { BuildingInteractable.Highlighted = null; if (hintCard != null) hintCard.gameObject.SetActive(true); });
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
                RefreshContracts();
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
            case BuildingKind.LaunchPad:
                // Los cohetes salen de acá, pero el satélite se arma y se lanza desde el hangar.
                if (string.IsNullOrEmpty(AcceptedContractId))
                {
                    GameShell.Play(Sfx.Deny);
                    Toast("Rockets launch from here. First pick a contract at Mission Control.");
                }
                else GameShell.Load(hangarScene);
                break;
            case BuildingKind.Antennas:
                toast.SetActive(false);
                stationsPanel.Open();
                break;
            case BuildingKind.Dome:
                toast.SetActive(false);
                archivePanel.Open();
                break;
        }
    }

    public void AcceptContract(int index)
    {
        var contract = TestCatalog.Contracts[index];
        if (!CanAccept(contract)) return;
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
        if (stationsPanel != null) stationsPanel.gameObject.SetActive(false);
        if (archivePanel != null) archivePanel.gameObject.SetActive(false);
    }

    Button difficultyButton;

    // Botón de nivel de dificultad en Mission Control (se arma la primera vez que se abre el panel).
    void BuildDifficultyButton()
    {
        if (difficultyButton != null) return;
        difficultyButton = UIFactory.ButtonCard("Difficulty", freeModeButton.transform.parent, "", 22, null, null, false);
        UIFactory.Pin((RectTransform)difficultyButton.transform, Vector2.one, new Vector2(-92f - 280f - 12f, -20f), new Vector2(340f, 56f));
        difficultyButton.onClick.AddListener(() =>
        {
            Difficulty.Next();
            RefreshContracts();
            RefreshStatus();
        });
    }

    // En Modo Libre se puede aceptar cualquier contrato; en campaña, solo los desbloqueados.
    static bool CanAccept(ContractSpec c) => FreeMode.On || Campaign.IsUnlocked(c);

    public void ToggleFreeMode()
    {
        FreeMode.Toggle();
        RefreshContracts();
        RefreshStatus();
    }

    // Estado de cada tarjeta de contrato según la campaña: bloqueado, disponible, aceptado o completado.
    void RefreshContracts()
    {
        BuildDifficultyButton();
        difficultyButton.GetComponentInChildren<Text>().text = "LEVEL: " + Difficulty.Name;
        freeModeButton.GetComponentInChildren<Text>().text = FreeMode.On ? "FREE MODE: ON" : "FREE MODE: OFF";
        freeModeButton.GetComponent<CardButtonFx>().SetActive(FreeMode.On);
        freeModeHint.text = FreeMode.On
            ? "Experiment freely: no budget limit, all contracts open. Results don't count for the campaign."
            : Difficulty.Summary;
        for (int i = 0; i < contractButtons.Length; i++)
        {
            var c = TestCatalog.Contracts[i];
            bool open = CanAccept(c);
            bool accepted = c.id == AcceptedContractId;
            int rank = SaveData.BestRank(c.title);

            contractButtons[i].GetComponentInChildren<Text>().text =
                !open ? "LOCKED" : accepted ? "ACCEPTED" : rank > 0 ? "FLY AGAIN" : "ACCEPT";
            contractButtons[i].interactable = open && !accepted;
            contractAccents[i].color = open ? Palette.Orange : Palette.Dim;

            // El presupuesto de la tarjeta depende del nivel de dificultad
            var budgetText = contractButtons[i].transform.parent.Find("Line_Budget/Value");
            if (budgetText != null) budgetText.GetComponent<Text>().text = $"${c.budget}M";

            var status = contractStatus[i];
            if (FreeMode.On) { status.text = accepted ? "FREE MODE · CURRENT CONTRACT" : "FREE MODE · NO BUDGET LIMIT"; status.color = Palette.Blue; }
            else if (!open) { status.text = $"UNLOCKS AFTER {Campaign.Previous(c)?.title.ToUpperInvariant()}"; status.color = Palette.Dim; }
            else if (rank == 3) { status.text = $"COMPLETED · BEST {SaveData.BestScience(c.title)} SCIENCE"; status.color = Palette.Green; }
            else if (rank > 0) { status.text = $"NOT DONE YET · BEST {SaveData.BestScience(c.title)} SCIENCE"; status.color = Palette.Orange; }
            else if (accepted) { status.text = "CURRENT CONTRACT · GO TO THE HANGAR"; status.color = Palette.Green; }
            else { status.text = "NEW CONTRACT"; status.color = Palette.Blue; }
        }
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
        contractDetail.text = FreeMode.On
            ? $"Goal {c.scienceGoal} science  ·  Free mode"
            : $"Goal {c.scienceGoal} science  ·  Budget ${c.budget}M";
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
