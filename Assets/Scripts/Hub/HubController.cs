using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Menú principal en 3D: clics en edificios, panel de contratos, carteles y tooltip.
public class HubController : MonoBehaviour
{
    // TEMPORAL hasta que exista MissionState: contrato aceptado, sobrevive al cambio de escena.
    public static string AcceptedContractId;

    public string hangarScene = "Hangar";

    [Header("UI")]
    public RectTransform tooltip;
    public TMP_Text tooltipText;
    public GameObject contractsPanel;
    public GameObject infoPanel;
    public TMP_Text infoTitle;
    public TMP_Text infoBody;
    public TMP_Text statusText;
    public TMP_Text toastText;
    public CanvasGroup toastGroup;

    float toastUntil;

    bool AnyPanelOpen => contractsPanel.activeSelf || infoPanel.activeSelf;

    void Start()
    {
        CloseAll();
        HideTooltip();
        toastGroup.alpha = 0f;
        RefreshStatus();
    }

    void Update()
    {
        if (tooltip.gameObject.activeSelf && Mouse.current != null)
            tooltip.position = Mouse.current.position.ReadValue() + new Vector2(18f, -18f);

        if (toastGroup.alpha > 0f && Time.unscaledTime > toastUntil)
            toastGroup.alpha = Mathf.MoveTowards(toastGroup.alpha, 0f, Time.unscaledDeltaTime * 2f);
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
                ShowInfo("GROUND STATIONS",
                    "Antennas on Earth that talk to your satellite. More stations = more data reaches home.\n\n" +
                    "Coming soon. For now, pick them in the Hangar (Comms tab).");
                break;
            case BuildingKind.Dome:
                ShowInfo("MISSION ARCHIVE",
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
            statusText.text = "No contract yet  |  Visit Mission Control";
            return;
        }
        var c = TestCatalog.Contract(AcceptedContractId);
        statusText.text = $"Contract: {c.title}  |  Budget: ${c.budget}M  |  Goal: {c.scienceGoal} science";
    }

    void Toast(string message)
    {
        toastText.text = message;
        toastGroup.alpha = 1f;
        toastUntil = Time.unscaledTime + 3f;
    }
}
