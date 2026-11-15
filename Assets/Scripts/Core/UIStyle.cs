using UnityEngine;

// Referencias de estilo (fuentes y sprites) que usa UIFactory en tiempo de ejecución.
// Va en el Canvas de cada escena; lo asigna el constructor de la escena.
[DefaultExecutionOrder(-100)]
public class UIStyle : MonoBehaviour
{
    public Font titleFont; // Chakra Petch Bold: títulos y botones
    public Font bodyFont;  // Chakra Petch SemiBold: textos y etiquetas
    public Font monoFont;  // Share Tech Mono: números
    public Sprite roundedFill;
    public Sprite roundedOutline;
    public Sprite softShadow;
    public float spriteScale = 2f;

    void Awake() => Apply();

    public void Apply()
    {
        UIFactory.TitleFont = titleFont;
        UIFactory.BodyFont = bodyFont;
        UIFactory.MonoFont = monoFont;
        UIFactory.RoundedFill = roundedFill;
        UIFactory.RoundedOutline = roundedOutline;
        UIFactory.SoftShadow = softShadow;
        UIFactory.SpriteScale = spriteScale;
    }
}
