using System;
using UnityEngine;
using UnityEngine.UI;

// Un paso del tutorial. Si tiene 'done', el paso avanza solo cuando el jugador hace lo que se pide
// (no hay botón NEXT); si no, se pasa con NEXT. 'onEnter' corre al mostrarse el paso.
public class TutorialStep
{
    public string text;
    public Func<bool> done;
    public Action onEnter;

    public TutorialStep(string text, Func<bool> done = null, Action onEnter = null)
    {
        this.text = text;
        this.done = done;
        this.onEnter = onEnter;
    }
}

// Tarjeta de tutorial que guía paso a paso: dice qué hacer y espera a que el jugador lo haga.
// La crea GameShell.Tutorial. Ancha (una fila) en el Centro Espacial, angosta (apilada) en el Hangar.
public class TutorialCoach : MonoBehaviour
{
    string key;
    TutorialStep[] steps;
    int index = -1;
    Func<bool> hideWhen;
    Action onFinish;
    CanvasGroup group;
    Text counter, body, nextLabel;
    GameObject nextButton;
    float doneSince = -1f;

    public void Setup(string key, TutorialStep[] steps, float bottom, float width, Func<bool> hideWhen, Action onFinish)
    {
        this.key = key;
        this.steps = steps;
        this.hideWhen = hideWhen;
        this.onFinish = onFinish;

        var rt = (RectTransform)transform;
        UIFactory.Stretch(rt);

        bool wide = width >= 700f; // en una fila: texto a la izquierda y botones a la derecha
        var card = UIFactory.Card("Card", rt, Palette.Blue);
        UIFactory.Pin(card, new Vector2(0.5f, 0f), new Vector2(0f, bottom), new Vector2(width, wide ? 116f : 190f));
        group = card.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f; // entra con el fundido de Update

        counter = UIFactory.Caption("Counter", card, "");
        UIFactory.Place(counter.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(30f, -38f), new Vector2(-20f, -12f));

        body = UIFactory.Label("Body", card, "", 24, Palette.TextMain, FontKind.Body, TextAnchor.UpperLeft);
        if (wide) UIFactory.Place(body.rectTransform, Vector2.zero, Vector2.one, new Vector2(30f, 10f), new Vector2(-290f, -40f));
        else UIFactory.Place(body.rectTransform, Vector2.zero, Vector2.one, new Vector2(30f, 66f), new Vector2(-24f, -40f));
        body.verticalOverflow = VerticalWrapMode.Truncate; // así el best fit achica la letra si no entra
        body.resizeTextForBestFit = true;
        body.resizeTextMinSize = 15;
        body.resizeTextMaxSize = 24;

        var skip = UIFactory.ButtonCard("Skip", card, "SKIP", 18, Palette.TextSecondary, null, false);
        if (wide) UIFactory.Pin((RectTransform)skip.transform, new Vector2(1f, 0.5f), new Vector2(-168f, -8f), new Vector2(100f, 46f));
        else UIFactory.Pin((RectTransform)skip.transform, Vector2.zero, new Vector2(30f, 14f), new Vector2(110f, 44f));
        skip.onClick.AddListener(Finish);

        var next = UIFactory.ButtonCard("Next", card, "NEXT", 20, null, null, false);
        if (wide) UIFactory.Pin((RectTransform)next.transform, new Vector2(1f, 0.5f), new Vector2(-18f, -8f), new Vector2(140f, 46f));
        else UIFactory.Pin((RectTransform)next.transform, new Vector2(1f, 0f), new Vector2(-18f, 14f), new Vector2(140f, 44f));
        next.onClick.AddListener(Next);
        nextLabel = next.GetComponentInChildren<Text>();
        nextButton = next.gameObject;

        Next();
    }

    void Next()
    {
        index++;
        doneSince = -1f;
        if (index >= steps.Length) { Finish(); return; }

        var step = steps[index];
        step.onEnter?.Invoke();
        counter.text = $"TUTORIAL  {index + 1}/{steps.Length}" + (step.done != null ? "  ·  YOUR TURN" : "");
        counter.color = step.done != null ? Palette.Orange : Palette.TextSecondary;
        body.text = step.text;
        nextLabel.text = index == steps.Length - 1 ? "GOT IT" : "NEXT";
        nextButton.SetActive(step.done == null); // los pasos con acción avanzan solos
    }

    void Finish()
    {
        SaveData.MarkSeen(key);
        onFinish?.Invoke();
        Destroy(gameObject);
    }

    void Update()
    {
        bool hidden = hideWhen != null && hideWhen();
        group.alpha = Mathf.MoveTowards(group.alpha, hidden ? 0f : 1f, Time.unscaledDeltaTime * 6f);
        group.blocksRaycasts = !hidden;

        // El jugador hizo lo que pedía el paso: se espera un instante (para que vea el efecto) y se avanza.
        if (index < 0 || index >= steps.Length || steps[index].done == null) return;
        if (!steps[index].done()) { doneSince = -1f; return; }
        if (doneSince < 0f) doneSince = Time.unscaledTime;
        if (Time.unscaledTime - doneSince > 0.35f) Next();
    }
}
