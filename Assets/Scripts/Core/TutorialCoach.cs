using System;
using UnityEngine;
using UnityEngine.UI;

// Tarjeta de tutorial con pasos ("1/3", texto, SKIP y NEXT). La crea GameShell.Tutorial.
public class TutorialCoach : MonoBehaviour
{
    string key;
    string[] steps;
    int index;
    Func<bool> hideWhen;
    CanvasGroup group;
    Text counter, body;
    Text nextLabel;

    public void Setup(string key, string[] steps, float bottom, float width, Func<bool> hideWhen)
    {
        this.key = key;
        this.steps = steps;
        this.hideWhen = hideWhen;

        var rt = (RectTransform)transform;
        UIFactory.Stretch(rt);

        var card = UIFactory.Card("Card", rt, Palette.Blue);
        UIFactory.Pin(card, new Vector2(0.5f, 0f), new Vector2(0f, bottom), new Vector2(width, 180f));
        group = card.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f; // entra con el fundido de Update

        counter = UIFactory.Caption("Counter", card, "");
        UIFactory.Place(counter.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(30f, -42f), new Vector2(-20f, -14f));

        body = UIFactory.Label("Body", card, "", 24, Palette.TextMain, FontKind.Body, TextAnchor.UpperLeft);
        UIFactory.Place(body.rectTransform, Vector2.zero, Vector2.one, new Vector2(30f, 68f), new Vector2(-24f, -44f));
        body.verticalOverflow = VerticalWrapMode.Truncate; // así el best fit achica la letra si no entra
        body.resizeTextForBestFit = true;
        body.resizeTextMinSize = 16;
        body.resizeTextMaxSize = 24;

        var skip = UIFactory.ButtonCard("Skip", card, "SKIP", 20, Palette.TextSecondary, null, false);
        UIFactory.Pin((RectTransform)skip.transform, Vector2.zero, new Vector2(30f, 14f), new Vector2(110f, 44f));
        skip.onClick.AddListener(Finish);

        var next = UIFactory.ButtonCard("Next", card, "NEXT", 20, null, null, false);
        UIFactory.Pin((RectTransform)next.transform, new Vector2(1f, 0f), new Vector2(-18f, 14f), new Vector2(140f, 44f));
        next.onClick.AddListener(Next);
        nextLabel = next.GetComponentInChildren<Text>();

        Show();
    }

    void Show()
    {
        counter.text = $"TUTORIAL  {index + 1}/{steps.Length}";
        body.text = steps[index];
        nextLabel.text = index == steps.Length - 1 ? "GOT IT" : "NEXT";
    }

    void Next()
    {
        index++;
        if (index >= steps.Length) Finish();
        else Show();
    }

    void Finish()
    {
        SaveData.MarkSeen(key);
        Destroy(gameObject);
    }

    void Update()
    {
        bool hidden = hideWhen != null && hideWhen();
        group.alpha = Mathf.MoveTowards(group.alpha, hidden ? 0f : 1f, Time.unscaledDeltaTime * 6f);
        group.blocksRaycasts = !hidden;
    }
}
