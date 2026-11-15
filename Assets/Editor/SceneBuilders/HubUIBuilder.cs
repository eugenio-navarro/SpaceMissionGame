using System.Linq;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

// UI del CentroEspacial con la guía de estética (tarjetas, Text común, márgenes de 36 px).
public static class HubUIBuilder
{
    const float M = UIFactory.Margin;

    public static void Build(HubController hub)
    {
        var root = BuilderUtil.CreateCanvas().transform;

        // Título (arriba a la izquierda)
        var title = UIFactory.Card("TitleCard", root, Palette.Orange);
        UIFactory.Pin(title, new Vector2(0f, 1f), new Vector2(M, -M), new Vector2(520f, 112f));
        Caption(title, "Space Center");
<<<<<<< HEAD
        var name = UIFactory.Label("Title", title, UILayout.GameTitle, 30, Palette.TextMain, FontKind.Title);
=======
        var name = UIFactory.Label("Title", title, "Mission Control: Build to Launch", 29, Palette.TextMain, FontKind.Title);
>>>>>>> 67306400d0ec165baa68b4130621682b16d4ae8a
        UIFactory.Place(name.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 12f), new Vector2(-20f, -44f));
        UIFactory.Appear(title.gameObject);

        // Botón de menú (pausa y opciones), al lado del título
        hub.menuButton = UIFactory.ButtonCard("MenuButton", root, "MENU", 26);
        UIFactory.Pin((RectTransform)hub.menuButton.transform, new Vector2(0f, 1f), new Vector2(M + 520f + 16f, -M), new Vector2(140f, 112f));
        UIFactory.Appear(hub.menuButton.gameObject, 0.04f);

        // Contrato actual (arriba a la derecha)
        var status = UIFactory.Card("ContractCard", root, Palette.Blue);
        UIFactory.Pin(status, Vector2.one, new Vector2(-M, -M), new Vector2(560f, 140f));
        Caption(status, "Current contract");
        hub.contractTitle = UIFactory.Label("Title", status, "None yet", 30, Palette.TextMain, FontKind.Title);
        UIFactory.Place(hub.contractTitle.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -88f), new Vector2(-20f, -44f));
        hub.contractDetail = UIFactory.Label("Detail", status, "", 24, Palette.TextSecondary);
        UIFactory.Place(hub.contractDetail.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(28f, 12f), new Vector2(-20f, 52f));
        UIFactory.Appear(status.gameObject, 0.08f);

        // Ayuda (abajo al centro)
        var hint = UIFactory.Card("HintCard", root, null, false);
        UIFactory.Pin(hint, new Vector2(0.5f, 0f), new Vector2(0f, M), new Vector2(600f, 60f));
        var hintText = UIFactory.Label("Text", hint, "Click a building to begin  ·  Esc: menu", 24, Palette.TextSecondary, FontKind.Body, TextAnchor.MiddleCenter);
        UIFactory.Stretch(hintText.rectTransform);
        UIFactory.Appear(hint.gameObject, 0.16f);

        // Tooltip que sigue al mouse
        var tip = UIFactory.Card("Tooltip", root, Palette.Orange, false);
        tip.anchorMin = tip.anchorMax = Vector2.zero;
        tip.pivot = new Vector2(0f, 1f);
        tip.sizeDelta = new Vector2(320f, 56f);
        tip.Find("Fill").GetComponent<Image>().raycastTarget = false;
        hub.tooltip = tip;
        hub.tooltipText = UIFactory.Label("Text", tip, "Building", 26, Palette.TextMain, FontKind.Title);
        UIFactory.Place(hub.tooltipText.rectTransform, Vector2.zero, Vector2.one, new Vector2(22f, 0f), new Vector2(-12f, 0f));
        UIFactory.Appear(tip.gameObject).duration = 0.12f;
        tip.gameObject.SetActive(false);

        // Aviso temporal
        var toast = UIFactory.Card("Toast", root, Palette.Green);
        UIFactory.Pin(toast, new Vector2(0.5f, 0f), new Vector2(0f, M + 84f), new Vector2(980f, 72f));
        toast.Find("Fill").GetComponent<Image>().raycastTarget = false;
        hub.toast = toast.gameObject;
        hub.toastText = UIFactory.Label("Text", toast, "", 26, Palette.TextMain, FontKind.Body, TextAnchor.MiddleCenter);
        UIFactory.Stretch(hub.toastText.rectTransform, 12f);
        UIFactory.Appear(toast.gameObject);
        toast.gameObject.SetActive(false);

        hub.contractsPanel = BuildContractsPanel(root, hub);
        BuildInfoPanel(root, hub);
    }

    static void Caption(RectTransform card, string text)
    {
        var c = UIFactory.Caption("Caption", card, text);
        UIFactory.Place(c.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -44f), new Vector2(-20f, -18f));
    }

    static RectTransform Modal(string name, Transform root, Vector2 size, Color accent, out RectTransform window)
    {
        var modal = UIFactory.Rect(name, root);
        UIFactory.Stretch(modal);
        var dim = modal.gameObject.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.5f); // bloquea los clics a los edificios
        window = UIFactory.Card("Window", modal, accent);
        UIFactory.Pin(window, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        UIFactory.Appear(window.gameObject);
        return modal;
    }

    static void CloseButton(Transform window, HubController hub)
    {
        var close = UIFactory.ButtonCard("Close", window, "X", 26, null, null, false);
        UIFactory.Pin((RectTransform)close.transform, Vector2.one, new Vector2(-20f, -20f), new Vector2(56f, 56f));
        UnityEventTools.AddPersistentListener(close.onClick, hub.CloseAll);
    }

    static GameObject BuildContractsPanel(Transform root, HubController hub)
    {
        var modal = Modal("ContractsPanel", root, new Vector2(1440f, 780f), Palette.Blue, out var window);
        var caption = UIFactory.Caption("Caption", window, "Mission Control");
        UIFactory.Place(caption.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(36f, -56f), new Vector2(-100f, -28f));
        var header = UIFactory.Label("Header", window, "Choose your contract", 40, Palette.TextMain, FontKind.Title);
        UIFactory.Place(header.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(36f, -108f), new Vector2(-100f, -54f));
        CloseButton(window, hub);

        // Modo Libre: todos los contratos, sin límite de presupuesto
        hub.freeModeButton = UIFactory.ButtonCard("FreeMode", window, "FREE MODE: OFF", 22, null, null, false);
        UIFactory.Pin((RectTransform)hub.freeModeButton.transform, Vector2.one, new Vector2(-92f, -20f), new Vector2(280f, 56f));
        UnityEventTools.AddPersistentListener(hub.freeModeButton.onClick, hub.ToggleFreeMode);
        hub.freeModeHint = UIFactory.Label("FreeModeHint", window, "", 18, Palette.TextSecondary, FontKind.Body, TextAnchor.MiddleRight);
        UIFactory.Place(hub.freeModeHint.rectTransform, new Vector2(0.4f, 1f), Vector2.one, new Vector2(0f, -112f), new Vector2(-40f, -80f));

        var cards = UIFactory.Rect("Cards", window);
        UIFactory.Place(cards, Vector2.zero, Vector2.one, new Vector2(36f, 36f), new Vector2(-36f, -130f));
        var row = cards.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 28f;
        row.childControlWidth = row.childControlHeight = true;
        row.childForceExpandWidth = row.childForceExpandHeight = true;

        int n = TestCatalog.Contracts.Count;
        hub.contractButtons = new Button[n];
        hub.contractStatus = new Text[n];
        hub.contractAccents = new Image[n];
        for (int i = 0; i < n; i++)
            BuildCard(cards, TestCatalog.Contracts[i], Campaign.DestinationColor(TestCatalog.Contracts[i]), i, hub);

        modal.gameObject.SetActive(false);
        return modal.gameObject;
    }

    static void BuildCard(Transform parent, ContractSpec c, Color color, int index, HubController hub)
    {
        // El estado (bloqueado, aceptado, completado) lo pone HubController en tiempo de ejecución.
        var card = UIFactory.Card("Card_" + c.id, parent, Palette.Orange, false);
        hub.contractAccents[index] = card.Find("Accent").GetComponent<Image>();
        UIFactory.Layout(card.gameObject, -1f, 400f, 1f); // las tres tarjetas del mismo ancho
        UIFactory.Vertical(card.gameObject, 26, 22, 22, 22, 10f);

        var destination = UIFactory.Image("Destination", card, UIFactory.RoundedFill, color);
        UIFactory.Layout(destination.gameObject, 170f);
        var destText = UIFactory.Label("Text", destination.transform, c.destination, 40, Color.white, FontKind.Title, TextAnchor.MiddleCenter);
        UIFactory.Stretch(destText.rectTransform);

        UIFactory.Layout(UIFactory.Label("Title", card, c.title, 30, Palette.TextMain, FontKind.Title).gameObject, 44f);
        UIFactory.Layout(UIFactory.Label("Objective", card, c.objective, 24, Palette.TextSecondary).gameObject, 60f);

        string orbits = string.Join(", ", c.allowedOrbits.Select(id => TestCatalog.Get(id).displayName));
        DataLine(card, "Science goal", c.scienceGoal.ToString());
        DataLine(card, "Budget", $"${c.budget}M");
        StackedLine(card, "Orbits", orbits);
        StackedLine(card, "Real mission like it", c.similarMission);

        var spacer = UIFactory.Rect("Spacer", card);
        UIFactory.Layout(spacer.gameObject, -1f, -1f, -1f, 1f);

        hub.contractStatus[index] = UIFactory.Label("Status", card, "", 20, Palette.TextSecondary, FontKind.Title, TextAnchor.MiddleCenter);
        UIFactory.Layout(hub.contractStatus[index].gameObject, 28f);
        hub.contractStatus[index].verticalOverflow = VerticalWrapMode.Truncate;
        hub.contractStatus[index].resizeTextForBestFit = true;
        hub.contractStatus[index].resizeTextMinSize = 13;
        hub.contractStatus[index].resizeTextMaxSize = 20;

        var button = UIFactory.ButtonCard("Accept", card, "ACCEPT", 30);
        UIFactory.Layout(button.gameObject, 68f);
        UnityEventTools.AddIntPersistentListener(button.onClick, hub.AcceptContract, index);
        hub.contractButtons[index] = button;
    }

    static void DataLine(Transform card, string caption, string value, FontKind kind = FontKind.Mono)
    {
        var line = UIFactory.Rect("Line_" + caption, card);
        UIFactory.Layout(line.gameObject, 30f);
        var c = UIFactory.Caption("Caption", line, caption);
        UIFactory.Place(c.rectTransform, Vector2.zero, new Vector2(0.45f, 1f), Vector2.zero, Vector2.zero);
        var v = UIFactory.Label("Value", line, value, 24, Palette.Green, kind, TextAnchor.MiddleRight);
        UIFactory.Place(v.rectTransform, new Vector2(0.35f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
    }

    // Dato de texto largo: etiqueta arriba y valor abajo (el layout toma la altura del texto).
    static void StackedLine(Transform card, string caption, string value)
    {
        UIFactory.Caption("Caption_" + caption, card, caption);
        UIFactory.Label("Value_" + caption, card, value, 24, Palette.Green);
    }

    static void BuildInfoPanel(Transform root, HubController hub)
    {
        var modal = Modal("InfoPanel", root, new Vector2(820f, 440f), Palette.Blue, out var window);
        var caption = UIFactory.Caption("Caption", window, "Space Center");
        UIFactory.Place(caption.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(36f, -56f), new Vector2(-100f, -28f));
        hub.infoTitle = UIFactory.Label("Title", window, "Title", 36, Palette.TextMain, FontKind.Title);
        UIFactory.Place(hub.infoTitle.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(36f, -108f), new Vector2(-100f, -54f));
        hub.infoBody = UIFactory.Label("Body", window, "", 26, Palette.TextSecondary, FontKind.Body, TextAnchor.UpperLeft);
        UIFactory.Place(hub.infoBody.rectTransform, Vector2.zero, Vector2.one, new Vector2(36f, 120f), new Vector2(-36f, -126f));
        CloseButton(window, hub);

        var ok = UIFactory.ButtonCard("OK", window, "OK", 30, null, null, false);
        UIFactory.Pin((RectTransform)ok.transform, new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(220f, 64f));
        UnityEventTools.AddPersistentListener(ok.onClick, hub.CloseAll);

        hub.infoPanel = modal.gameObject;
        modal.gameObject.SetActive(false);
    }
}
