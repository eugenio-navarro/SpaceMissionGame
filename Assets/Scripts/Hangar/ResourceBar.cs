using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Barra del HUD: valor contra límite (masa vs. capacidad, consumo vs. generación, etc.).
public class ResourceBar : MonoBehaviour
{
    public RectTransform fill;
    public Image fillImage;
    public TMP_Text valueText;

    // overIsBlocking: pasarse del límite bloquea el lanzamiento (rojo) o solo advierte (amarillo).
    public void Set(float value, float max, string text, bool overIsBlocking = true)
    {
        float ratio = max > 0f ? value / max : 0f;
        fill.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);

        Color color;
        if (max <= 0f) color = Palette.Metal;
        else if (ratio > 1f) color = overIsBlocking ? Palette.Red : Palette.Yellow;
        else if (ratio > 0.9f) color = Palette.Yellow;
        else color = Palette.Phosphor;

        fillImage.color = color;
        valueText.text = text;
    }
}
