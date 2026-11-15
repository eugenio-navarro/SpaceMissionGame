using UnityEngine;
using UnityEngine.EventSystems;

public enum BuildingKind { MissionControl, Hangar, Antennas, Dome, LaunchPad }

// Edificio clickeable del centro espacial. Necesita un Collider y un PhysicsRaycaster en la cámara.
public class BuildingInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public BuildingKind kind;
    public string displayName;
    public float hoverScale = 1.06f;

    // Edificio que el tutorial quiere que el jugador clickee: late para llamar la atención.
    public static BuildingKind? Highlighted;

    Vector3 baseScale;
    HubController hub;
    bool hovered;

    void Update()
    {
        if (hovered) return;
        bool pulse = Highlighted == kind && (hub == null || !hub.Busy);
        float k = pulse ? 1f + 0.07f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f)) : 1f;
        transform.localScale = baseScale * k;
    }

    void Awake()
    {
        if (kind == BuildingKind.LaunchPad) { enabled = false; return; }
        baseScale = transform.localScale;
        hub = FindAnyObjectByType<HubController>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (kind == BuildingKind.LaunchPad || (hub != null && hub.Busy)) return;
        hovered = true;
        transform.localScale = baseScale * hoverScale;
        GameShell.SetPointer(true);
        GameShell.Play(Sfx.Hover);
        if (hub != null) hub.ShowTooltip(displayName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (kind == BuildingKind.LaunchPad) return;
        hovered = false;
        transform.localScale = baseScale;
        GameShell.SetPointer(false);
        if (hub != null) hub.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (kind == BuildingKind.LaunchPad) return;
        hovered = false;
        transform.localScale = baseScale;
        GameShell.SetPointer(false);
        if (hub == null || hub.Busy) return;
        GameShell.Play(Sfx.Click);
        hub.HideTooltip();
        hub.OnBuildingClicked(kind);
    }
}
