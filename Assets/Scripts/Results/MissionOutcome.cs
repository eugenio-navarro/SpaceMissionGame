using UnityEngine;

// Resuelve qué pasó con la misión: evento de la simulación, dado de riesgo, ciencia final
// y los textos que explican por qué. Clase pura, sin escena (reglas de la sección 3 y 4 del README).
public static class MissionOutcome
{
    public const float StormSafeMargin = 0.20f;      // con menos margen de energía, la tormenta apaga un instrumento
    public const int InterferenceSafeStations = 3;    // con menos estaciones, la interferencia baja menos datos
    public const float InterferenceLoss = 0.20f;
    public const float RiskFailureLoss = 0.50f;
    public const float PartialThreshold = 0.60f;

    public static bool IsEligible(ResultsInfo r, LaunchEventKind kind)
    {
        switch (kind)
        {
            case LaunchEventKind.Calm:
            case LaunchEventKind.SolarStorm:
            case LaunchEventKind.StationInterference: return true;
            case LaunchEventKind.CloudCover: return r.design != null && r.destination == "EARTH";
            case LaunchEventKind.ExtendedEclipse: return r.design != null && r.destination == "MOON" && r.orbitId == "lunar";
            default: return false;
        }
    }

    public static void Resolve(ResultsInfo r, LaunchEventKind kind, float riskRoll) =>
        Resolve(r, new[] { kind }, riskRoll);

    public static void Resolve(ResultsInfo r, System.Collections.Generic.IReadOnlyList<LaunchEventKind> events, float riskRoll)
    {
        var selected = new System.Collections.Generic.List<LaunchEventKind>();
        foreach (var kind in MissionForecast.Hazards)
            if (IsEligible(r, kind) && events != null)
                for (int i = 0; i < events.Count; i++)
                    if (events[i] == kind) { selected.Add(kind); break; }
        r.occurredEvents = selected.ToArray();
        r.eventKind = selected.Count > 0 ? selected[0] : LaunchEventKind.Calm;
        bool storm = selected.Contains(LaunchEventKind.SolarStorm) && r.EnergyMargin < StormSafeMargin;
        bool clouds = selected.Contains(LaunchEventKind.CloudCover);
        float generated = r.dataGenerated;
        if (r.design != null)
        {
            PartSpec disabled = null;
            if (storm)
                foreach (var part in r.design.instruments)
                    if (disabled == null || part.power > disabled.power) disabled = part;
            generated = 0f;
            foreach (var part in r.design.instruments)
                if (part != disabled)
                    generated += part.dataRate * (clouds && (part.id == "cam_b" || part.id == "cam_a") ? 0.5f : 1f);
        }
        else if (storm) generated = Mathf.Max(0f, generated - r.hungriestInstrumentData);
        if (selected.Contains(LaunchEventKind.ExtendedEclipse)) generated *= EclipseRetention(r);
        float downlink = r.downlink;
        if (selected.Contains(LaunchEventKind.StationInterference) && r.stations < InterferenceSafeStations)
            downlink *= 1f - InterferenceLoss;
        r.received = Mathf.Min(generated, downlink);
        r.eventHurt = r.received < Mathf.Min(r.dataGenerated, r.downlink) - 0.001f;
        r.riskFailure = riskRoll < r.riskPercent;
        r.science = Mathf.FloorToInt(r.received * r.detail + 0.5f);
        if (r.riskFailure) r.science = Mathf.FloorToInt(r.science * (1f - RiskFailureLoss) + 0.5f);
        r.result = r.science >= r.scienceGoal ? ResultKind.Success : r.science >= r.scienceGoal * PartialThreshold ? ResultKind.Partial : ResultKind.Failed;
        var notes = new System.Collections.Generic.List<string>();
        foreach (var kind in selected) notes.Add(MissionForecast.Name(kind) + ": " + MissionForecast.Mitigation(r, kind));
        r.eventTitle = selected.Count == 0 ? "ALL CALM" : selected.Count == 1 ? MissionForecast.Name(selected[0]).ToUpperInvariant() : selected.Count + " MISSION EVENTS";
        r.eventText = selected.Count == 0 ? "No environmental events occurred this flight." : string.Join("\n", notes);
        r.headline = Headline(r);
        r.why = $"After events, instruments produced {generated:0.#} GB/day; available downlink was {downlink:0.#}. Earth received {r.received:0.#} GB/day.";
        if (r.riskFailure) r.why += " A hardware failure then halved the science return.";
        r.tryNext = selected.Count > 0 ? "Check the forecast tablet: compare radar, battery endurance, spare power and ground stations before flying again." : TryNext(r, generated > downlink);
        r.capcomLine = Capcom(r);
        r.similarMissionNote = SimilarNote(r.similarMission);
        r.resolved = true;
    }

    public static float EclipseRetention(ResultsInfo r) => r.design?.battery == null ? 0.6f :
        r.design.battery.extendedEclipseRetention > 0f ? Mathf.Clamp01(r.design.battery.extendedEclipseRetention) : 0.85f;

    static string Headline(ResultsInfo r)
    {
        if (r.result == ResultKind.Success) return "The science goal was reached.";
        if (r.result == ResultKind.Partial) return "Some science came back, but not enough.";
        if (r.riskFailure) return "A part failed on the way up, and too little science reached Earth.";
        return "The spacecraft works, but too little science reached Earth.";
    }

    static string TryNext(ResultsInfo r, bool dataJam)
    {
        // Una falla con márgenes holgados fue mala suerte: el consejo no debe culpar al diseño.
        if (r.riskFailure && (r.EnergyMargin < 0.30f || r.MassMargin < 0.10f))
            return "Leave more spare power and mass. Thin margins are what push the risk up.";
        if (r.riskFailure)
            return "That was bad luck: every launch has some risk, even with good margins. Fly it again!";
        if (r.eventHurt && r.eventKind == LaunchEventKind.SolarStorm)
            return "Keep at least 20% power margin: a bigger solar panel or a lighter instrument.";
        if (r.eventHurt && r.eventKind == LaunchEventKind.StationInterference)
            return "Use 3 or more ground stations, so the others cover when one fails.";
        if (dataJam && r.design != null)
            return MissionCalculator.DataFix(r.design);
        if (dataJam)
            return "Add a ground station or a bigger antenna so more data comes down.";
        if (r.result == ResultKind.Success)
            return "Try to reach the same goal with a cheaper or lighter design.";
        return "Add an instrument that makes more data, or pick an orbit with more detail.";
    }

    static string Capcom(ResultsInfo r)
    {
        // P6: regla real de NASA Goddard (GOLD Rules): dejar margen de energía y masa al principio del diseño.
        float thinnest = Mathf.Min(r.EnergyMargin, r.MassMargin);
        if (r.result != ResultKind.Success && thinnest >= 0f && thinnest < 0.25f)
            return "NASA Goddard's design rules ask for spare power and mass early on. Your thinnest margin was " + Mathf.RoundToInt(thinnest * 100f) + "%.";
        if (r.result == ResultKind.Success) return "Nice work. The scientists are already fighting over the data.";
        if (r.result == ResultKind.Partial) return "Not bad. Some data made it home, and now we know what to fix.";
        return "Well... space is hard. Let's look at what happened and try again.";
    }

    static string SimilarNote(string mission)
    {
        var m = RealMissions.Find(mission);
        return m != null ? m.fact : "";
    }
}
