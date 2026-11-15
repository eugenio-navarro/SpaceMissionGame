using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menú principal en 3D: clics en edificios, panel de contratos, carteles y tooltip.
public class HubController : MonoBehaviour
{
    // TEMPORAL hasta que exista MissionState: contrato aceptado, sobrevive al cambio de escena.
    public static string AcceptedContractId;

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

    float toastUntil;
    CanvasGroup toastGroup;

    bool AnyPanelOpen => contractsPanel.activeSelf || infoPanel.activeSelf;

    void Start()
    {
        toastGroup = toast.GetComponent<CanvasGroup>();
        toast.SetActive(false);
        CloseAll();
        HideTooltip();
        RefreshStatus();
    }

    void Update()
    {
        if (tooltip.gameObject.activeSelf && Mouse.current != null)
        {
            var canvas = tooltip.GetComponentInParent<Canvas>();
            tooltip.position = Mouse.current.position.ReadValue() + new Vector2(20f, -20f) * canvas.scaleFactor;
        }

        if (toast.activeSelf && Time.unscaledTime > toastUntil)
        {
            toastGroup.alpha = Mathf.MoveTowards(toastGroup.alpha, 0f, Time.unscaledDeltaTime * 3f);
            if (toastGroup.alpha <= 0f) toast.SetActive(false);
        }
    }

    public void ShowTooltip(string text)
    {
        if (AnyPanelOpen) return;
        tooltipText.text = text;
        tooltip.gameObject.SetActive(true);
    }

    public void HideTooltip() => tooltip.gameObject.SetActive(false);

    public void OnBuildingClicked(BuildingKind kind)
    {
        if (AnyPanelOpen) return;
        switch (kind)
        {
            case BuildingKind.MissionControl:
                contractsPanel.SetActive(true);
                break;
            case BuildingKind.Hangar:
                if (string.IsNullOrEmpty(AcceptedContractId)) Toast("First pick a contract at Mission Control.");
                else SceneManager.LoadScene(hangarScene);
                break;
            case BuildingKind.Antennas:
                ShowInfo("Ground Stations",
                    "Antennas on Earth that talk to your satellite. More stations = more data reaches home.\n\n" +
                    "Coming soon. For now, pick them in the Hangar (Comms tab).");
                break;
            case BuildingKind.Dome:
                ShowInfo("Mission Archive",
                    "Your past missions, results and unlocked parts will live here.\n\nComing soon.");
                break;
        }
    }

    public void AcceptContract(int index)
    {
        var contract = TestCatalog.Contracts[index];
        if (!contract.unlocked) return;
        AcceptedContractId = contract.id;
        CloseAll();
        RefreshStatus();
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
        if (string.IsNullOrEmpty(AcceptedContractId))
        {
            contractTitle.text = "None yet";
            contractTitle.color = Palette.Dim;
            contractDetail.text = "Visit Mission Control to pick one";
            return;
        }
        var c = TestCatalog.Contract(AcceptedContractId);
        contractTitle.text = c.title;
        contractTitle.color = Palette.TextMain;
        contractDetail.text = $"Goal {c.scienceGoal} science  ·  Budget ${c.budget}M";
    }

    void Toast(string message)
    {
        toastText.text = message;
        toast.SetActive(false); // reinicia la animación de entrada
        toast.SetActive(true);
        toastUntil = Time.unscaledTime + 3f;
    }
}
