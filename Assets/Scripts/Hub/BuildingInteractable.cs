using UnityEngine;
using UnityEngine.EventSystems;

public enum BuildingKind { MissionControl, Hangar, Antennas, Dome }

// Edificio clickeable del centro espacial. Necesita un Collider y un PhysicsRaycaster en la cámara.
public class BuildingInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public BuildingKind kind;
    public string displayName;
    public float hoverScale = 1.06f;

    Vector3 baseScale;
    HubController hub;

    void Awake()
    {
        baseScale = transform.localScale;
        hub = FindAnyObjectByType<HubController>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = baseScale * hoverScale;
        if (hub != null) hub.ShowTooltip(displayName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = baseScale;
        if (hub != null) hub.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        transform.localScale = baseScale;
        if (hub == null) return;
        hub.HideTooltip();
        hub.OnBuildingClicked(kind);
    }
}
