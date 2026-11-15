using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Shared authored coordinate space and small corrections for scenes saved before this increment.
public static class UILayout
{
    public const string GameTitle = "Mission Control: Build to Launch";

    public static void Configure(CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
    }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    static extern bool SetWindowText(System.IntPtr window, string title);
#endif

    public static void ApplyScene()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        using (var process = System.Diagnostics.Process.GetCurrentProcess())
            SetWindowText(process.MainWindowHandle, GameTitle);
#endif
        foreach (var scaler in Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include)) Configure(scaler);
        foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include))
        {
            if (text.text == "Space Mission Design" || text.text == "SPACE MISSION DESIGN")
            {
                text.text = GameTitle;
                Fit(text, 22);
            }
        }
        var hangar = Object.FindAnyObjectByType<HangarUI>();
        if (hangar != null) ApplyHangar(hangar);
        var results = Object.FindAnyObjectByType<ResultsController>();
        if (results != null) ApplyResults(results);
        var launch = Object.FindAnyObjectByType<LaunchHud>();
        if (launch != null)
        {
            Fit(launch.statusText, 20);
            launch.eventCard.sizeDelta = new Vector2(1020f, 330f);
            UIFactory.Place(launch.eventBody.rectTransform, Vector2.zero, Vector2.one, new Vector2(42f, 24f), new Vector2(-32f, -90f));
            ScrollText(launch.eventBody);
            Fit(launch.eventTitle, 24);
        }
    }

    public static void Fit(Text text, int minimum)
    {
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = minimum;
        text.resizeTextMaxSize = text.fontSize;
    }

    public static void ApplyHangar(HangarUI ui)
    {
        ScrollContent(ui.listContent);
        ScrollText(ui.infoText);
        ScrollText(ui.alertsText);
        Fit(ui.contractTitle, 25);
        Fit(ui.contractDetail, 20);
        Fit(ui.fundsText, 36);
        Fit(ui.scienceText, 38);
        Fit(ui.launchReasonText, 16);
        foreach (var bar in new[] { ui.massBar, ui.powerBar, ui.dataBar, ui.costBar })
        {
            var caption = bar.transform.Find("Caption").GetComponent<Text>();
            string label = bar == ui.massBar ? "MASS" : bar == ui.powerBar ? "POWER" : bar == ui.dataBar ? "DATA" : "COST";
            caption.text = label;
            UIFactory.Place(caption.rectTransform, new Vector2(0f, 1f), new Vector2(0.32f, 1f), new Vector2(0f, -28f), Vector2.zero);
            UIFactory.Place(bar.valueText.rectTransform, new Vector2(0.32f, 1f), Vector2.one, new Vector2(0f, -28f), Vector2.zero);
            Fit(bar.valueText, 18);
        }
    }

    static void ApplyResults(ResultsController ui)
    {
        if (ui.eventCard.transform.parent.Find("ScoreCard") != null) return;
        ui.eventCard.GetComponent<RectTransform>().sizeDelta = new Vector2(460f, 390f);
        UIFactory.Place(ui.eventBodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -332f), new Vector2(432f, -82f));
        ScrollText(ui.eventBodyText);
        UIFactory.Pin(ui.riskText.rectTransform, new Vector2(0f, 1f), new Vector2(28f, -350f), new Vector2(410f, 28f));
        ui.tradeCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(36f, -574f);
        foreach (var label in ui.tradeCard.GetComponentsInChildren<Text>())
            if (label.text == "WITH A DIFFERENT CHOICE") label.text = "CALM-FLIGHT DESIGN COMPARISON";
        ScrollText(ui.whyText);
        ScrollText(ui.tryNextText);
        Fit(ui.similarNote, 13);
        Fit(ui.resultHeadline, 16);
        var parent = ui.eventCard.transform.parent;
        if (parent.Find("ScoreCard") == null)
        {
            var card = UIFactory.Card("ScoreCard", parent, Palette.Green);
            UIFactory.Pin(card, new Vector2(0f, 1f), new Vector2(36f, -764f), new Vector2(460f, 160f));
            var score = UIFactory.Label("Score", card, "", 23, Palette.TextMain, FontKind.Body);
            UIFactory.Stretch(score.rectTransform, 24f);
            var run = ResultsContext.GetOrCreate();
            int bonus = SaveData.EfficiencyBonus(run.budget, run.cost, run.science >= run.scienceGoal, FreeMode.On);
            score.text = $"<b>MISSION SCORE   {SaveData.Score(run, FreeMode.On)}</b>\nScience  {run.science} x 10 = {run.science * 10}\nEfficiency bonus  +{bonus}\n<size=18>Save budget and reach the goal to earn the bonus.</size>";
        }
    }

    public static ScrollRect ScrollText(Text text)
    {
        var scroll = ScrollContent(text.rectTransform);
        text.resizeTextForBestFit = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.alignment = TextAnchor.UpperLeft;
        text.raycastTarget = true;
        return scroll;
    }

    public static ScrollRect ScrollContent(RectTransform content)
    {
        if (content.parent.name == content.name + "Viewport") return content.parent.parent.GetComponent<ScrollRect>();
        var parent = content.parent;
        int index = content.GetSiblingIndex();
        var host = UIFactory.Rect(content.name + "Scroll", parent);
        host.SetSiblingIndex(index);
        host.anchorMin = content.anchorMin;
        host.anchorMax = content.anchorMax;
        host.pivot = content.pivot;
        host.offsetMin = content.offsetMin;
        host.offsetMax = content.offsetMax;
        var viewport = UIFactory.Rect(content.name + "Viewport", host);
        UIFactory.Stretch(viewport);
        viewport.gameObject.AddComponent<RectMask2D>();
        var hit = viewport.gameObject.AddComponent<Image>();
        hit.color = Color.clear;
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        var fit = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = host.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 32f;
        viewport.offsetMax = new Vector2(-14f, 0f);
        var track = UIFactory.Image("Scrollbar", host, UIFactory.RoundedFill, new Color(1f, 1f, 1f, 0.08f), true);
        UIFactory.Place(track.rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-8f, 0f), Vector2.zero);
        var handle = UIFactory.Image("Handle", track.transform, UIFactory.RoundedFill, Palette.TextSecondary, true);
        UIFactory.Stretch(handle.rectTransform);
        var scrollbar = track.gameObject.AddComponent<Scrollbar>();
        scrollbar.handleRect = handle.rectTransform;
        scrollbar.targetGraphic = handle;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        return scroll;
    }
}
