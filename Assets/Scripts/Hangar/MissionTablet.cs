using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// A lightweight forecast screen. Charts use the same probabilities and resolver as flight.
public class MissionTablet : MonoBehaviour
{
    HangarController controller;
    RectTransform panel, content;
    Coroutine slide;
    public bool IsOpen => gameObject.activeSelf;

    public static MissionTablet Create(Transform root, HangarController controller)
    {
        var panel = UIFactory.Card("MissionTablet", root, Palette.Blue);
        UIFactory.Pin(panel, Vector2.one, new Vector2(-36f, -110f), new Vector2(740f, 900f));
        panel.Find("Fill").GetComponent<Image>().color = new Color(0.025f, 0.045f, 0.075f, 0.99f);
        var tablet = panel.gameObject.AddComponent<MissionTablet>();
        tablet.panel = panel;
        tablet.controller = controller;
        var title = UIFactory.Label("Title", panel, "MISSION FORECAST", 32, Palette.TextMain, FontKind.Title);
        UIFactory.Pin(title.rectTransform, new Vector2(0f, 1f), new Vector2(30f, -25f), new Vector2(580f, 44f));
        var close = UIFactory.ButtonCard("Close", panel, "X", 24, null, null, false);
        UIFactory.Pin((RectTransform)close.transform, Vector2.one, new Vector2(-24f, -24f), new Vector2(50f, 44f));
        close.onClick.AddListener(tablet.Close);
        tablet.content = UIFactory.Rect("ForecastContent", panel);
        UIFactory.Place(tablet.content, Vector2.zero, Vector2.one, new Vector2(30f, 24f), new Vector2(-30f, -82f));
        UIFactory.Vertical(tablet.content.gameObject, 0, 10, 0, 0, 16f);
        UILayout.ScrollContent(tablet.content);
        controller.Changed += tablet.Refresh;
        panel.gameObject.SetActive(false);
        return tablet;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Refresh();
        if (slide != null) StopCoroutine(slide);
        slide = StartCoroutine(SlideIn());
    }

    IEnumerator SlideIn()
    {
        for (float t = 0; t < 0.22f; t += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / 0.22f, 3f);
            panel.anchoredPosition = new Vector2(Mathf.Lerp(780f, -36f, k), -110f);
            yield return null;
        }
        panel.anchoredPosition = new Vector2(-36f, -110f);
        slide = null;
    }

    public void Close() => gameObject.SetActive(false);
    void OnDestroy() { if (controller != null) controller.Changed -= Refresh; }

    void Refresh()
    {
        if (!IsOpen) return;
        foreach (Transform child in content) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        var forecast = MissionForecast.Build(controller.Design, controller.Contract);
        TextBlock("Independent chances per mission. Several events can occur together. These probabilities and losses are simplified game values.", 20, Palette.TextSecondary);
        TextBlock($"<b>Any event  {100f - forecast.noneProbability:0.#}%     Two or more  {forecast.multipleProbability:0.#}%</b>", 23, Palette.Blue);
        foreach (var row in forecast.rows)
        {
            var card = UIFactory.Card(row.kind.ToString(), content, row.mitigated ? Palette.Green : Palette.Orange, false);
            UIFactory.Layout(card.gameObject, 170f);
            var name = UIFactory.Label("Name", card, MissionForecast.Name(row.kind).ToUpperInvariant(), 21, Palette.TextMain, FontKind.Title);
            UIFactory.Place(name.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(20f, -40f), new Vector2(-85f, -14f));
            var chance = UIFactory.Label("Chance", card, $"{row.probability:0}%", 25, Palette.Blue, FontKind.Mono, TextAnchor.MiddleRight);
            UIFactory.Pin(chance.rectTransform, Vector2.one, new Vector2(-18f, -12f), new Vector2(70f, 30f));
            var track = UIFactory.Image("Track", card, UIFactory.RoundedFill, new Color(1f, 1f, 1f, 0.1f));
            UIFactory.Place(track.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(20f, -58f), new Vector2(-20f, -48f));
            var fill = UIFactory.Image("Probability", track.transform, UIFactory.RoundedFill, Palette.Blue);
            UIFactory.Place(fill.rectTransform, Vector2.zero, new Vector2(row.probability / 100f, 1f), Vector2.zero, Vector2.zero);
            var body = UIFactory.Label("Effect", card, $"{row.mitigation}\n<b>Science with this event  {row.science}/{controller.Contract.scienceGoal}</b>", 20, Palette.TextSecondary);
            UIFactory.Place(body.rectTransform, Vector2.zero, Vector2.one, new Vector2(20f, 14f), new Vector2(-20f, -70f));
        }
        TextBlock($"<b>SCIENCE PREVIEW</b>\nCalm  {forecast.baseline.science}    All eligible events together  {forecast.combined.science}\nGoal  {controller.Contract.scienceGoal}    Hardware failure risk  {forecast.hardwareRisk}%\n<size=18>Previews assume hardware survives. Hardware failure halves final science.</size>", 22, Palette.TextMain);
        int bonus = SaveData.EfficiencyBonus(controller.Contract.budget, controller.Report.cost, controller.Report.science >= controller.Contract.scienceGoal, FreeMode.On);
        TextBlock($"<b>EFFICIENCY REWARD</b>\nCalm score  {SaveData.Score(forecast.baseline, FreeMode.On)}    Savings bonus  +{bonus}\n<size=18>The bonus requires the final science goal. Free mode earns no savings bonus.</size>", 22, Palette.Green);
        TextBlock("<b>CHOOSING YOUR CAMERA</b>\n" + TestCatalog.Get("cam_b").description + "\n\n" + TestCatalog.Get("cam_a").description, 20, Palette.TextSecondary);
        TextBlock("Keep this tablet open while selecting parts on the left to compare their effects.", 19, Palette.Blue);
    }

    void TextBlock(string message, int size, Color color)
    {
        UIFactory.Label("Note", content, message, size, color, FontKind.Body, TextAnchor.UpperLeft);
    }
}
