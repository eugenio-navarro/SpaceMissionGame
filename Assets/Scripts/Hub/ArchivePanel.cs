using UnityEngine;
using UnityEngine.UI;

// Cúpula: archivo de misiones. Pestaña CONTRACTS: por cada contrato, estado, mejor resultado, ciencia
// contra la meta, último vuelo y ranking. Pestaña FLIGHT LOG: cada vuelo con qué salió bien, qué salió mal
// y la lección, para que el jugador aprenda de sus intentos.
public class ArchivePanel : MonoBehaviour
{
    class Row
    {
        public Image chip, fill;
        public Text title, status, science, last, top;
        public RectTransform fillRect;
    }

    Row[] rows;
    Text summary, unlocks;
    RectTransform contractsView, logView, logList;
    Text logDetail;
    Button contractsTab, logTab;
    int selected = -1;

    public static ArchivePanel Create(Transform canvasRoot, HubController hub)
    {
        var modal = HubModal.Create("ArchivePanel", canvasRoot, new Vector2(1320f, 760f), Palette.Green,
            "Space Center", "Mission Archive", hub.CloseAll, out var window);
        var panel = modal.gameObject.AddComponent<ArchivePanel>();
        panel.Build(window);
        modal.gameObject.SetActive(false);
        return panel;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Refresh();
        // Si hay vuelos, se abre directo en la bitácora con el último vuelo elegido.
        ShowLog(FlightLog.All().Count > 0);
    }

    void ShowLog(bool log)
    {
        contractsView.gameObject.SetActive(!log);
        logView.gameObject.SetActive(log);
        contractsTab.GetComponent<CardButtonFx>().SetActive(!log);
        logTab.GetComponent<CardButtonFx>().SetActive(log);
        if (log) RefreshLog(-1);
    }

    // ---------- Bitácora ----------

    void RefreshLog(int pick)
    {
        foreach (Transform child in logList) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        var flights = FlightLog.All();
        logTab.GetComponentInChildren<Text>().text = $"FLIGHT LOG ({flights.Count})";

        if (flights.Count == 0)
        {
            logDetail.text = "<b>No flights yet.</b>\n\nLaunch your first satellite. Every flight is saved here with what went well, what went wrong and what to try next, so you can learn from each attempt.";
            return;
        }

        // Los más nuevos primero (se ven los últimos 9)
        int shown = Mathf.Min(9, flights.Count);
        if (pick < 0) pick = flights.Count - 1;
        selected = pick;
        for (int k = 0; k < shown; k++)
        {
            int index = flights.Count - 1 - k;
            var f = flights[index];
            string result = ResultName(f.Result);
            var b = UIFactory.ButtonCard("Flight", logList, "", 18, null, null, false);
            UIFactory.Layout(b.gameObject, 52f);
            b.GetComponent<CardButtonFx>().SetActive(index == selected);
            var label = b.GetComponentInChildren<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            label.rectTransform.offsetMin = new Vector2(18f, 4f);
            label.text = $"<color={Palette.Tag(Palette.TextSecondary)}>#{f.number}</color>  {f.contract}   " +
                         $"<color={Palette.Tag(ResultColor(f.Result))}>{result} {f.science}/{f.goal}</color>";
            label.color = Palette.TextMain;
            b.GetComponent<CardButtonFx>().labelColor = Palette.TextMain;
            b.onClick.AddListener(() => RefreshLog(index));
        }

        logDetail.text = Analysis(flights[selected]);
    }

    static string Analysis(FlightRecord f)
    {
        string green = Palette.Tag(Palette.Green), orange = Palette.Tag(Palette.Orange), grey = Palette.Tag(Palette.TextSecondary);
        var sb = new System.Text.StringBuilder();
        sb.Append($"<size=30><b>Flight #{f.number}: {f.contract}</b></size>\n");
        sb.Append($"<color={Palette.Tag(ResultColor(f.Result))}><b>{ResultName(f.Result)}</b> · {f.science} of {f.goal} science · score {f.score}</color>\n");
        sb.Append($"<color={grey}>{f.mode} · {f.level} · {f.rocket} · {f.orbit} · {f.stations} station{(f.stations == 1 ? "" : "s")} · ${f.cost}M\n{f.parts}</color>\n\n");

        var well = FlightLog.WentWell(f);
        var wrong = FlightLog.WentWrong(f);
        sb.Append($"<color={green}><b>WHAT WENT WELL</b></color>\n");
        if (well.Count == 0) sb.Append($"<color={grey}>Nothing stood out this time.</color>\n");
        foreach (var line in well) sb.Append($"<color={green}>+</color> {line}\n");
        sb.Append($"\n<color={orange}><b>WHAT WENT WRONG</b></color>\n");
        if (wrong.Count == 0) sb.Append($"<color={grey}>Nothing. A clean flight!</color>\n");
        foreach (var line in wrong) sb.Append($"<color={orange}>-</color> {line}\n");
        sb.Append($"\n<b>LESSON FOR NEXT TIME</b>\n{f.tryNext}");
        return sb.ToString();
    }

    static string ResultName(ResultKind r) => r == ResultKind.Success ? "SUCCESS" : r == ResultKind.Partial ? "PARTIAL" : "FAILED";
    static Color ResultColor(ResultKind r) => r == ResultKind.Success ? Palette.Green : Palette.Orange;

    void Refresh()
    {
        summary.text = $"MISSIONS FLOWN  <color={Palette.Tag(Palette.TextMain)}>{SaveData.MissionsFlown}</color>";

        var unlocked = new System.Text.StringBuilder();
        for (int i = 0; i < rows.Length; i++)
        {
            var c = TestCatalog.Contracts[i];
            var row = rows[i];
            bool open = Campaign.IsUnlocked(c);
            int rank = SaveData.BestRank(c.title);
            int best = SaveData.BestScience(c.title);
            var last = SaveData.Last(c.title);

            row.chip.color = open ? Campaign.DestinationColor(c) : Palette.Dim;
            row.title.color = open ? Palette.TextMain : Palette.TextSecondary;

            if (!open) { row.status.text = "LOCKED"; row.status.color = Palette.Dim; }
            else if (rank == 0) { row.status.text = "NOT FLOWN YET"; row.status.color = Palette.TextSecondary; }
            else
            {
                row.status.text = rank == 3 ? "BEST: SUCCESS" : rank == 2 ? "BEST: PARTIAL" : "BEST: FAILED";
                row.status.color = rank == 3 ? Palette.Green : Palette.Orange;
            }

            float scale = c.scienceGoal * 1.5f;
            row.fillRect.anchorMax = new Vector2(Mathf.Clamp01(best / scale), 1f);
            row.fill.color = best >= c.scienceGoal ? Palette.Green : Palette.Orange;
            row.science.text = $"{best} / {c.scienceGoal}";

            if (!open) row.last.text = Campaign.LockedReason(c) + ".";
            else if (last == null) row.last.text = "Accept this contract at Mission Control and launch your first satellite.";
            else
            {
                string result = last.result == ResultKind.Success ? "SUCCESS" : last.result == ResultKind.Partial ? "PARTIAL" : "FAILED";
                string color = Palette.Tag(last.result == ResultKind.Success ? Palette.Green : Palette.Orange);
                row.last.text = $"Last flight: {last.rocket} · {last.orbit} · <color={color}>{result}</color>, {last.science} science. {last.headline}";
            }

            var top = SaveData.TopScores(c.title);
            row.top.text = top.Length == 0 ? "" : "TOP SCORES  " + string.Join(" · ", top);

            if (i > 0 && open) unlocked.Append(unlocked.Length > 0 ? ", " : "").Append(c.title);
        }

        unlocks.text = unlocked.Length > 0
            ? $"UNLOCKED SO FAR  <color={Palette.Tag(Palette.Green)}>{unlocked}</color>"
            : "Complete a contract with MISSION SUCCESS to unlock the next one.";
    }

    void Build(RectTransform window)
    {
        summary = UIFactory.Label("Summary", window, "", 18, Palette.TextSecondary, FontKind.Body, TextAnchor.MiddleRight);
        UIFactory.Place(summary.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(36f, -112f), new Vector2(-100f, -84f));

        // Pestañas
        logTab = UIFactory.ButtonCard("LogTab", window, "FLIGHT LOG", 20, null, null, false);
        UIFactory.Pin((RectTransform)logTab.transform, Vector2.one, new Vector2(-92f, -20f), new Vector2(240f, 52f));
        logTab.onClick.AddListener(() => ShowLog(true));
        contractsTab = UIFactory.ButtonCard("ContractsTab", window, "CONTRACTS", 20, null, null, false);
        UIFactory.Pin((RectTransform)contractsTab.transform, Vector2.one, new Vector2(-92f - 252f, -20f), new Vector2(200f, 52f));
        contractsTab.onClick.AddListener(() => ShowLog(false));

        BuildLog(window);

        contractsView = UIFactory.Rect("ContractsView", window);
        UIFactory.Stretch(contractsView);
        window = contractsView;

        var list = UIFactory.Rect("List", window);
        UIFactory.Place(list, Vector2.zero, Vector2.one, new Vector2(36f, 84f), new Vector2(-36f, -128f));
        var v = UIFactory.Vertical(list.gameObject, 0, 0, 0, 0, 16f);
        v.childForceExpandHeight = true;

        rows = new Row[TestCatalog.Contracts.Count];
        for (int i = 0; i < rows.Length; i++) rows[i] = BuildRow(list, TestCatalog.Contracts[i]);

        unlocks = UIFactory.Label("Unlocks", window, "", 20, Palette.TextSecondary, FontKind.Body);
        UIFactory.Place(unlocks.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(40f, 28f), new Vector2(-40f, 70f));
    }

    void BuildLog(RectTransform window)
    {
        logView = UIFactory.Rect("LogView", window);
        UIFactory.Stretch(logView);

        logList = UIFactory.Rect("Flights", logView);
        UIFactory.Place(logList, Vector2.zero, new Vector2(0f, 1f), new Vector2(36f, 36f), new Vector2(36f + 500f, -128f));
        var v = UIFactory.Vertical(logList.gameObject, 0, 0, 0, 0, 8f);
        v.childAlignment = TextAnchor.UpperLeft;

        var detailCard = UIFactory.Card("Detail", logView, Palette.Blue, false);
        UIFactory.Place(detailCard, Vector2.zero, Vector2.one, new Vector2(36f + 500f + 24f, 36f), new Vector2(-36f, -128f));
        logDetail = UIFactory.Label("Analysis", detailCard, "", 21, Palette.TextMain, FontKind.Body, TextAnchor.UpperLeft);
        UIFactory.Place(logDetail.rectTransform, Vector2.zero, Vector2.one, new Vector2(34f, 20f), new Vector2(-24f, -20f));
        logDetail.verticalOverflow = VerticalWrapMode.Truncate;
        logDetail.resizeTextForBestFit = true;
        logDetail.resizeTextMinSize = 13;
        logDetail.resizeTextMaxSize = 21;
    }

    static Row BuildRow(RectTransform parent, ContractSpec c)
    {
        var card = UIFactory.Card("Row_" + c.id, parent, null, false);
        var row = new Row();

        row.chip = UIFactory.Image("Chip", card, UIFactory.RoundedFill, Campaign.DestinationColor(c));
        UIFactory.Place(row.chip.rectTransform, Vector2.zero, new Vector2(0f, 1f), new Vector2(14f, 14f), new Vector2(14f + 190f, -14f));
        var dest = UIFactory.Label("Destination", row.chip.transform, c.destination, 24, Color.white, FontKind.Title, TextAnchor.MiddleCenter);
        UIFactory.Stretch(dest.rectTransform, 6f);

        const float x = 230f;
        row.title = UIFactory.Label("Title", card, c.title, 28, Palette.TextMain, FontKind.Title);
        UIFactory.Place(row.title.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(x, -54f), new Vector2(-260f, -14f));

        row.status = UIFactory.Label("Status", card, "", 20, Palette.TextSecondary, FontKind.Title, TextAnchor.MiddleRight);
        UIFactory.Place(row.status.rectTransform, new Vector2(1f, 1f), Vector2.one, new Vector2(-260f, -54f), new Vector2(-24f, -14f));

        var track = UIFactory.Image("Track", card, UIFactory.RoundedFill, new Color(1f, 1f, 1f, 0.08f));
        UIFactory.Place(track.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(x, -7f), new Vector2(-220f, 7f));
        row.fill = UIFactory.Image("Fill", track.transform, UIFactory.RoundedFill, Palette.Green);
        row.fillRect = row.fill.rectTransform;
        UIFactory.Place(row.fillRect, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
        // Marca de la meta (el ancho total de la barra es 1,5 veces la meta)
        var goal = UIFactory.Image("Goal", track.transform, null, Palette.TextMain);
        UIFactory.Place(goal.rectTransform, new Vector2(1f / 1.5f, -0.6f), new Vector2(1f / 1.5f, 1.6f), new Vector2(-1.5f, 0f), new Vector2(1.5f, 0f));

        row.science = UIFactory.Label("Science", card, "", 24, Palette.TextMain, FontKind.Mono, TextAnchor.MiddleRight);
        UIFactory.Place(row.science.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-200f, -18f), new Vector2(-24f, 18f));
        var scienceCaption = UIFactory.Caption("ScienceCaption", card, "best science / goal");
        scienceCaption.alignment = TextAnchor.MiddleRight;
        UIFactory.Place(scienceCaption.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-260f, 16f), new Vector2(-24f, 38f));

        row.last = UIFactory.Label("Last", card, "", 19, Palette.TextSecondary, FontKind.Body, TextAnchor.LowerLeft);
        UIFactory.Place(row.last.rectTransform, Vector2.zero, new Vector2(1f, 0.5f), new Vector2(x, 12f), new Vector2(-330f, -14f));

        // Ranking local: los 5 mejores puntajes del contrato
        row.top = UIFactory.Label("Top", card, "", 17, Palette.Blue, FontKind.Title, TextAnchor.LowerRight);
        UIFactory.Place(row.top.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0.5f), new Vector2(-320f, 12f), new Vector2(-24f, -14f));
        row.top.verticalOverflow = VerticalWrapMode.Truncate;
        row.top.resizeTextForBestFit = true;
        row.top.resizeTextMinSize = 12;
        row.top.resizeTextMaxSize = 17;
        row.last.verticalOverflow = VerticalWrapMode.Truncate;
        row.last.resizeTextForBestFit = true;
        row.last.resizeTextMinSize = 14;
        row.last.resizeTextMaxSize = 19;
        return row;
    }
}
