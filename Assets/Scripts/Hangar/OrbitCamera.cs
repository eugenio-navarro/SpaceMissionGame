using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Cámara del hangar: arrastrar con el mouse gira alrededor de la nave, la rueda hace zoom.
public class OrbitCamera : MonoBehaviour
{
    public Transform focus;           // la cámara apunta a la altura media entre el piso y este punto
    public float focusHeightFactor = 0.75f;
    public float distance = 16f;
    public float minDistance = 6f;
    public float maxDistance = 20f;
    public float yaw = 25f;
    public float pitch = 12f;
    public float rotateSpeed = 0.25f;
    public float zoomStep = 1.5f;

    Vector3 target = new Vector3(0f, 3.5f, 0f);
    bool dragging;

    void LateUpdate()
    {
        var mouse = Mouse.current;
        if (mouse != null) HandleInput(mouse);

        if (focus != null)
        {
            var wanted = new Vector3(0f, Mathf.Max(2f, focus.position.y * focusHeightFactor), 0f);
            target = Vector3.Lerp(target, wanted, Time.deltaTime * 4f);
        }

        var rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target + rotation * new Vector3(0f, 0f, -distance);
        transform.rotation = rotation;
    }

    void HandleInput(Mouse mouse)
    {
        if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
            dragging = !PointerOverUI();
        if (!mouse.leftButton.isPressed && !mouse.rightButton.isPressed)
            dragging = false;

        if (dragging)
        {
            var delta = mouse.delta.ReadValue();
            yaw += delta.x * rotateSpeed;
            pitch = Mathf.Clamp(pitch - delta.y * rotateSpeed, -5f, 60f);
        }

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f && !PointerOverUI())
            distance = Mathf.Clamp(distance - Mathf.Sign(scroll) * zoomStep, minDistance, maxDistance);
    }

    static bool PointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
}
