using UnityEngine;
using UnityEngine.UI;

// Barra del HUD: valor contra límite (masa vs. capacidad, consumo vs. generación, etc.).
public class ResourceBar : MonoBehaviour
{
    public RectTransform fill;
    public Image fillImage;
    public Text valueText;

    // warnAbove: desde qué proporción se avisa. Masa y energía avisan con margen menor al 10% (0.9);
    // datos y costo solo cuando se pasan (1.0), igual que las alertas del README.
    public void Set(float value, float max, string text, float warnAbove = 0.9f)
    {
        float ratio = max > 0f ? value / max : 0f;
        fill.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);

        // Verde: todo bien. Naranja: cerca del límite o pasado.
        Color color = max <= 0f ? Palette.Dim : ratio > warnAbove ? Palette.Orange : Palette.Green;
        fillImage.color = color;
        valueText.text = text;
        valueText.color = max > 0f && ratio > 1f ? Palette.Orange : Palette.TextMain;
    }
}
