using UnityEngine;
using UnityEngine.UI;

// UI del Hangar con la guía de estética (tarjetas, Text común, márgenes de 36 px).
public static class HangarUIBuilder
{
    const float M = UIFactory.Margin;
    const float LeftWidth = 480f;
    const float RightWidth = 520f;
    const float Gap = 20f;

    public static void Build(HangarController controller)
    {
        var root = BuilderUtil.CreateCanvas().transform;
        var ui = root.gameObject.AddComponent<HangarUI>();
        ui.controller = controller;

        BuildTitle(root, ui);
        BuildCatalog(root, ui);
        BuildFunds(root, ui);
        BuildStatus(root, ui);
        BuildAlerts(root, ui);
        BuildLaunch(root, ui);
    }

    static Text Caption(RectTransform card, string text, float top = 18f)
    {
        var c = UIFactory.Caption("Caption", card, text);
        UIFactory.Place(c.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -top - 24f), new Vector2(-20f, -top));
        return c;
    }

    static void BuildTitle(Transform root, HangarUI ui)
    {
        var card = UIFactory.Card("TitleCard", root, Palette.Blue);
        UIFactory.Pin(card, new Vector2(0f, 1f), new Vector2(M, -M), new Vector2(LeftWidth, 140f));
        Caption(card, "Assembly Hangar  ·  Contract");
        ui.contractTitle = UIFactory.Label("Title", card, "", 34, Palette.TextMain, FontKind.Title);
        UIFactory.Place(ui.contractTitle.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -92f), new Vector2(-20f, -44f));
        ui.contractDetail = UIFactory.Label("Detail", card, "", 24, Palette.TextSecondary);
        UIFactory.Place(ui.contractDetail.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(28f, 12f), new Vector2(-20f, 48f));
        UIFactory.Appear(card.gameObject);

        ui.backButton = UIFactory.ButtonCard("BackButton", root, "<  SPACE CENTER", 26);
        UIFactory.Pin((RectTransform)ui.backButton.transform, Vector2.one, new Vector2(-M, -M), new Vector2(RightWidth, 64f));

        var hint = UIFactory.Label("Hint", root, "DRAG TO ROTATE  ·  SCROLL TO ZOOM", 15, Palette.TextSecondary, FontKind.Body, TextAnchor.MiddleCenter);
        UIFactory.Pin(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -M), new Vector2(600f, 30f));
    }

    static void BuildCatalog(Transform root, HangarUI ui)
    {
        float top = M + 140f + Gap, bottom = M + 150f + Gap;
        var card = UIFactory.Card("Catalog", root, Palette.Orange);
        UIFactory.Place(card, Vector2.zero, new Vector2(0f, 1f), new Vector2(M, bottom), new Vector2(M + LeftWidth, -top));
        UIFactory.Appear(card.gameObject, 0.06f);

        var tabs = UIFactory.Rect("Tabs", card);
        UIFactory.Place(tabs, new Vector2(0f, 1f), Vector2.one, new Vector2(22f, -74f), new Vector2(-16f, -16f));
        var h = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 8f;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = h.childForceExpandHeight = true;
        string[] names = { "SCIENCE", "POWER", "ROCKET", "COMMS" };
        ui.tabButtons = new Button[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            ui.tabButtons[i] = UIFactory.ButtonCard("Tab_" + names[i], tabs, names[i], 20, Palette.TextSecondary, null, false);
        }

        ui.listContent = UIFactory.Rect("List", card);
        UIFactory.Place(ui.listContent, Vector2.zero, Vector2.one, new Vector2(22f, 190f), new Vector2(-16f, -84f));
        var v = UIFactory.Vertical(ui.listContent.gameObject, 0, 0, 0, 0, 8f);
        v.childAlignment = TextAnchor.UpperLeft;

        var info = UIFactory.Card("Info", card, null, false);
        UIFactory.Place(info, Vector2.zero, new Vector2(1f, 0f), new Vector2(22f, 16f), new Vector2(-16f, 176f));
        ui.infoText = UIFactory.Label("Text", info, "", 22, Palette.TextMain, FontKind.Body, TextAnchor.UpperLeft);
        UIFactory.Stretch(ui.infoText.rectTransform, 16f);
    }

    static void BuildFunds(Transform root, HangarUI ui)
    {
        var card = UIFactory.Card("Funds", root, Palette.Green);
        UIFactory.Pin(card, Vector2.zero, new Vector2(M, M), new Vector2(LeftWidth, 150f));
        Caption(card, "Funds left");
        ui.fundsText = UIFactory.Label("Value", card, "", 64, Palette.Green, FontKind.Mono);
        UIFactory.Place(ui.fundsText.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 10f), new Vector2(-20f, -44f));
        UIFactory.Appear(card.gameObject, 0.12f);
    }

    static void BuildStatus(Transform root, HangarUI ui)
    {
        float top = M + 64f + Gap;
        var card = UIFactory.Card("Status", root, Palette.Green);
        UIFactory.Pin(card, Vector2.one, new Vector2(-M, -top), new Vector2(RightWidth, 500f));
        Caption(card, "Mission status");
        UIFactory.Appear(card.gameObject, 0.06f);

        var bars = UIFactory.Rect("Bars", card);
        UIFactory.Place(bars, Vector2.zero, Vector2.one, new Vector2(28f, 140f), new Vector2(-24f, -54f));
        UIFactory.Vertical(bars.gameObject, 0, 0, 0, 0, 14f);
        ui.massBar = Bar(bars, "Mass  (vs. what the rocket lifts)");
        ui.powerBar = Bar(bars, "Power  (used vs. made)");
        ui.dataBar = Bar(bars, "Data  (made vs. sent to Earth)");
        ui.costBar = Bar(bars, "Cost  (spent vs. budget)");

        ui.scienceText = BigNumber(card, "Estimated science", new Vector2(0f, 0f), new Vector2(0.55f, 0f));
        ui.riskText = BigNumber(card, "Failure risk", new Vector2(0.55f, 0f), new Vector2(1f, 0f));
    }

    static Text BigNumber(RectTransform card, string caption, Vector2 anchorMin, Vector2 anchorMax)
    {
        var block = UIFactory.Rect("Block_" + caption, card);
        UIFactory.Place(block, anchorMin, new Vector2(anchorMax.x, 0f), new Vector2(28f, 18f), new Vector2(-12f, 128f));
        var c = UIFactory.Caption("Caption", block, caption);
        UIFactory.Place(c.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -24f), Vector2.zero);
        var value = UIFactory.Label("Value", block, "", 56, Palette.Green, FontKind.Mono);
        UIFactory.Place(value.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -26f));
        return value;
    }

    static ResourceBar Bar(Transform parent, string caption)
    {
        var group = UIFactory.Rect("Bar_" + caption.Split(' ')[0], parent);
        UIFactory.Layout(group.gameObject, 58f);
        var bar = group.gameObject.AddComponent<ResourceBar>();

        var c = UIFactory.Caption("Caption", group, caption);
        UIFactory.Place(c.rectTransform, new Vector2(0f, 1f), new Vector2(0.62f, 1f), new Vector2(0f, -26f), Vector2.zero);
        bar.valueText = UIFactory.Label("Value", group, "", 22, Palette.TextMain, FontKind.Mono, TextAnchor.MiddleRight);
        UIFactory.Place(bar.valueText.rectTransform, new Vector2(0.45f, 1f), Vector2.one, new Vector2(0f, -28f), Vector2.zero);

        var track = UIFactory.Image("Track", group, UIFactory.RoundedFill, new Color(1f, 1f, 1f, 0.08f));
        UIFactory.Place(track.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 4f), new Vector2(0f, 22f));
        bar.fillImage = UIFactory.Image("Fill", track.transform, UIFactory.RoundedFill, Palette.Green);
        bar.fill = bar.fillImage.rectTransform;
        UIFactory.Place(bar.fill, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
        return bar;
    }

    static void BuildAlerts(Transform root, HangarUI ui)
    {
        float top = M + 64f + Gap + 500f + Gap, bottom = M + 150f + Gap;
        var card = UIFactory.Card("Alerts", root, Palette.Orange);
        UIFactory.Place(card, new Vector2(1f, 0f), Vector2.one, new Vector2(-M - RightWidth, bottom), new Vector2(-M, -top));
        Caption(card, "Alerts");
        ui.alertsText = UIFactory.Label("Text", card, "", 22, Palette.TextMain, FontKind.Body, TextAnchor.UpperLeft);
        UIFactory.Place(ui.alertsText.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 14f), new Vector2(-20f, -48f));
        UIFactory.Appear(card.gameObject, 0.1f);
    }

    static void BuildLaunch(Transform root, HangarUI ui)
    {
        ui.launchButton = UIFactory.ButtonCard("LaunchButton", root, "LAUNCH", 56, null, Palette.Orange);
        var rt = (RectTransform)ui.launchButton.transform;
        UIFactory.Pin(rt, new Vector2(1f, 0f), new Vector2(-M, M), new Vector2(RightWidth, 150f));
        var label = rt.Find("Label").GetComponent<Text>();
        UIFactory.Place(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(20f, 46f), new Vector2(-20f, -10f));

        ui.launchReasonText = UIFactory.Label("Reason", rt, "", 18, Palette.TextSecondary, FontKind.Body, TextAnchor.MiddleCenter);
        UIFactory.Place(ui.launchReasonText.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(24f, 12f), new Vector2(-24f, 52f));
    }
}
