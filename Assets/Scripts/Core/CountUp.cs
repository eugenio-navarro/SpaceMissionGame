using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Número que cuenta desde 0 hasta el valor final al aparecer.
[RequireComponent(typeof(Text))]
public class CountUp : MonoBehaviour
{
    public float duration = 1.2f;
    public float delay = 0.3f;

    public void Play(int target, string format)
    {
        StopAllCoroutines();
        StartCoroutine(Run(target, format));
    }

    IEnumerator Run(int target, string format)
    {
        var text = GetComponent<Text>();
        text.text = string.Format(format, 0);
        yield return new WaitForSecondsRealtime(delay);
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / duration, 3f); // frena al final
            text.text = string.Format(format, Mathf.RoundToInt(target * k));
            yield return null;
        }
        text.text = string.Format(format, target);
    }
}
