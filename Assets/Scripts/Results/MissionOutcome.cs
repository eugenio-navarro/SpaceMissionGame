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

    // riskRoll: número entre 0 y 100. Si es menor que el riesgo del diseño, algo falla.
    public static void Resolve(ResultsInfo r, LaunchEventKind eventKind, float riskRoll)
    {
        float generated = r.dataGenerated;
        float downlink = r.downlink;
        int marginPercent = Mathf.RoundToInt(r.EnergyMargin * 100f);

        r.eventKind = eventKind;
        r.eventHurt = false;

        if (eventKind == LaunchEventKind.SolarStorm)
        {
            r.eventTitle = "SOLAR STORM";
            if (r.EnergyMargin < StormSafeMargin)
            {
                r.eventHurt = true;
                generated = Mathf.Max(0f, generated - r.hungriestInstrumentData);
                r.eventText = "A burst of particles from the Sun hit the spacecraft. With only " + marginPercent +
                              "% power margin, the " + r.hungriestInstrument.ToLower() + " had to shut down.";
            }
            else
            {
                r.eventText = "A burst of particles from the Sun hit the spacecraft. Your " + marginPercent +
                              "% power margin kept every instrument on.";
            }
        }
        else if (eventKind == LaunchEventKind.StationInterference)
        {
            r.eventTitle = "GROUND STATION INTERFERENCE";
            if (r.stations < InterferenceSafeStations)
            {
                r.eventHurt = true;
                downlink *= 1f - InterferenceLoss;
                r.eventText = "One antenna on Earth lost the signal. With only " + r.stations +
                              (r.stations == 1 ? " station" : " stations") + ", 20% less data came down.";
            }
            else
            {
                r.eventText = "One antenna on Earth lost the signal, but your other " + (r.stations - 1) +
                              " stations covered for it.";
            }
        }
        else
        {
            r.eventTitle = "ALL CALM";
            r.eventText = "No surprises this time. Space was quiet during the launch.";
        }

        r.received = Mathf.Min(generated, downlink);
        int science = Mathf.FloorToInt(r.received * r.detail + 0.5f);

        r.riskFailure = riskRoll < r.riskPercent;
        if (r.riskFailure) science = Mathf.FloorToInt(science * (1f - RiskFailureLoss) + 0.5f);
        r.science = science;

        if (science >= r.scienceGoal) r.result = ResultKind.Success;
        else if (science >= r.scienceGoal * PartialThreshold) r.result = ResultKind.Partial;
        else r.result = ResultKind.Failed;

        bool dataJam = r.dataGenerated > r.downlink + 0.01f;
        r.headline = Headline(r);
        r.why = Why(r, r.dataGenerated, r.downlink, dataJam);
        r.tryNext = TryNext(r, dataJam);
        r.capcomLine = Capcom(r);
        r.similarMissionNote = SimilarNote(r.similarMission);
        r.resolved = true;
    }

    static string Headline(ResultsInfo r)
    {
        if (r.result == ResultKind.Success) return "The science goal was reached.";
        if (r.result == ResultKind.Partial) return "Some science came back, but not enough.";
        if (r.riskFailure) return "A part failed on the way up, and too little science reached Earth.";
        return "The spacecraft works, but too little science reached Earth.";
    }

    static string Why(ResultsInfo r, float generated, float downlink, bool dataJam)
    {
        string text;
        if (dataJam)
        {
            int lost = Mathf.RoundToInt((1f - downlink / generated) * 100f);
            text = "Your instruments made " + generated.ToString("0") + " GB/day, but the antenna could only bring down " +
                   downlink.ToString("0") + ". " + lost + "% of the data never reached Earth.";
        }
        else
        {
            text = "Your instruments made " + generated.ToString("0") + " GB/day and all of it reached Earth.";
        }

        if (r.eventHurt && r.eventKind == LaunchEventKind.SolarStorm)
            text += " Then a solar storm switched off the " + r.hungriestInstrument.ToLower() + " because the power margin was too thin, so its data was lost.";
        else if (r.eventHurt && r.eventKind == LaunchEventKind.StationInterference)
            text += " Then a ground station lost the signal and there were too few stations to cover for it, so even less came down.";

        if (r.riskFailure)
            text += " A part also failed on the way up (the risk was " + r.riskPercent + "%), and half of the science was lost.";

        return text;
    }

    static string TryNext(ResultsInfo r, bool dataJam)
    {
        if (r.riskFailure && r.riskPercent > 5)
            return "Leave more spare power and mass. Thin margins are what push the risk up.";
        if (r.eventHurt && r.eventKind == LaunchEventKind.SolarStorm)
            return "Keep at least 20% power margin: a bigger solar panel or a lighter instrument.";
        if (r.eventHurt && r.eventKind == LaunchEventKind.StationInterference)
            return "Use 3 or more ground stations, so the others cover when one fails.";
        if (dataJam && r.design != null)
            return MissionCalculator.DataFix(r.design) + " That way more data comes down.";
        if (dataJam)
            return "Add a ground station or a bigger antenna so more data comes down.";
        if (r.result == ResultKind.Success)
            return "Try to reach the same goal with a cheaper or lighter design.";
        return "Add an instrument that makes more data, or pick an orbit with more detail.";
    }

    static string Capcom(ResultsInfo r)
    {
        if (r.result == ResultKind.Success) return "Nice work. The scientists are already fighting over the data.";
        if (r.result == ResultKind.Partial) return "Not bad. Some data made it home, and now we know what to fix.";
        return "Well... space is hard. Let's look at what happened and try again.";
    }

    static string SimilarNote(string mission)
    {
        switch (mission)
        {
            case "Landsat": return "NASA and USGS satellites that have watched Earth's land since 1972.";
            case "Hubble": return "A space telescope in low Earth orbit, working since 1990.";
            case "Lunar Reconnaissance Orbiter": return "A NASA orbiter that has mapped the Moon since 2009.";
            default: return "";
        }
    }
}
