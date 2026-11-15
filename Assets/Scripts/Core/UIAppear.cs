using System.Collections;
using UnityEngine;

// Hace que un elemento aparezca con un fundido corto y un rebote chico cada vez que se activa.
[RequireComponent(typeof(CanvasGroup))]
public class UIAppear : MonoBehaviour
{
    public float duration = 0.22f;
    public float startScale = 0.94f;
    public float delay;

    CanvasGroup group;

    void Awake() => group = GetComponent<CanvasGroup>();

    void OnEnable()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        StopAllCoroutines();
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        group.alpha = 0f;
        transform.localScale = Vector3.one * startScale;
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            group.alpha = Mathf.Clamp01(k * 1.6f);
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(startScale, 1f, EaseOutBack(k));
            yield return null;
        }
        group.alpha = 1f;
        transform.localScale = Vector3.one;
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
