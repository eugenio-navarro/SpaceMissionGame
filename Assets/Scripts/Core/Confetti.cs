using UnityEngine;
using UnityEngine.UI;

// Lluvia de papelitos de colores (UI) para festejar una misión exitosa. Se destruye sola.
public class Confetti : MonoBehaviour
{
    class Piece
    {
        public RectTransform rt;
        public Image img;
        public Vector2 velocity;
        public float spin, wobble, phase;
    }

    Piece[] pieces;
    float age;
    const float Life = 4f;

    public static void Burst(RectTransform parent, int count = 140)
    {
        var root = UIFactory.Rect("Confetti", parent);
        UIFactory.Stretch(root);
        root.gameObject.AddComponent<Confetti>().Spawn(count);
    }

    void Spawn(int count)
    {
        Color[] colors = { Palette.Orange, Palette.Green, Palette.Blue, Palette.TextMain, Palette.Yellow };
        var size = ((RectTransform)transform).rect.size;
        pieces = new Piece[count];
        for (int i = 0; i < count; i++)
        {
            var img = UIFactory.Image("Bit", transform, null, colors[i % colors.Length]);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(Random.Range(8f, 14f), Random.Range(14f, 24f));
            rt.anchoredPosition = new Vector2(Random.Range(-size.x * 0.5f, size.x * 0.5f), Random.Range(20f, 400f));
            pieces[i] = new Piece
            {
                rt = rt,
                img = img,
                velocity = new Vector2(Random.Range(-80f, 80f), Random.Range(-520f, -260f)),
                spin = Random.Range(-360f, 360f),
                wobble = Random.Range(40f, 120f),
                phase = Random.Range(0f, 6.28f),
            };
        }
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        age += dt;
        float alpha = Mathf.Clamp01((Life - age) / 0.8f);
        foreach (var p in pieces)
        {
            p.velocity.y -= 160f * dt;
            p.rt.anchoredPosition += new Vector2(p.velocity.x + Mathf.Sin(age * 4f + p.phase) * p.wobble, p.velocity.y) * dt;
            p.rt.Rotate(0f, 0f, p.spin * dt);
            var c = p.img.color;
            c.a = alpha;
            p.img.color = c;
        }
        if (age >= Life) Destroy(gameObject);
    }
}
