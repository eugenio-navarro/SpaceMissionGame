using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Botón-tarjeta: al pasar el mouse el borde se pone naranja y la tarjeta hace un rebote chico.
// Si el botón está desactivado, el texto se apaga.
public class CardButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Button button;
    public Image border;
    public Text label;
    public Color labelColor = Color.white;
    public Color borderColor = new Color(1f, 1f, 1f, 0.16f);
    public bool active; // estado "elegido" (pestañas): borde y texto naranja fijos

    bool hover, pressed;
    float scale = 1f;

    public void SetActive(bool value)
    {
        active = value;
        Refresh();
    }

    void OnEnable()
    {
        hover = pressed = false;
        scale = 1f;
        transform.localScale = Vector3.one;
        Refresh();
    }

    void Update()
    {
        float target = !button.interactable ? 1f : pressed ? 0.97f : hover ? 1.03f : 1f;
        scale = Mathf.Lerp(scale, target, Time.unscaledDeltaTime * 18f);
        transform.localScale = Vector3.one * scale;
        Refresh();
    }

    void Refresh()
    {
        if (button == null) return;
        bool on = button.interactable;
        if (label != null) label.color = !on ? Palette.Dim : active ? Palette.Orange : labelColor;
        if (border != null) border.color = on && (hover || active) ? Palette.Orange : borderColor;
    }

    public void OnPointerEnter(PointerEventData e) => hover = true;
    public void OnPointerExit(PointerEventData e) { hover = false; pressed = false; }
    public void OnPointerDown(PointerEventData e) => pressed = true;
    public void OnPointerUp(PointerEventData e) => pressed = false;
}
