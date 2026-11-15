using System;
using System.Collections.Generic;
using UnityEngine;

// Un vuelo guardado en la bitácora: el diseño, el resultado y por qué salió así.
[Serializable]
public class FlightRecord
{
    public int number;
    public string contract, mode, level;
    public string rocket, orbit, parts;
    public int stations;
    public int result;               // ResultKind
    public int science, goal, cost, budget, risk, score;
    public bool riskFailure, eventHurt;
    public string eventTitle;
    public float massMargin, powerMargin, dataMade, dataReceived;
    public string why, tryNext;

    public ResultKind Result => (ResultKind)result;
}

// Bitácora de vuelos (PlayerPrefs, funciona en WebGL): guarda los últimos vuelos y, para cada uno,
// arma qué salió bien, qué salió mal y la lección. La muestra el Mission Archive.
public static class FlightLog
{
    const int Max = 30;

    [Serializable] class Store { public List<FlightRecord> flights = new List<FlightRecord>(); }

    public static List<FlightRecord> All()
    {
        string json = PlayerPrefs.GetString("flight_log", "");
        if (json.Length == 0) return new List<FlightRecord>();
        var store = JsonUtility.FromJson<Store>(json);
        return store != null && store.flights != null ? store.flights : new List<FlightRecord>();
    }

    public static void Record(ResultsInfo run, int score)
    {
        var flights = All();
        int number = flights.Count > 0 ? flights[flights.Count - 1].number + 1 : 1;
        var d = run.design;
        var parts = new List<string>();
        if (d != null)
        {
            foreach (var i in d.instruments) parts.Add(i.displayName);
            if (d.panel != null) parts.Add(d.panel.displayName);
            if (d.battery != null) parts.Add(d.battery.displayName);
            if (d.antenna != null) parts.Add(d.antenna.displayName);
        }

        flights.Add(new FlightRecord
        {
            number = number,
            contract = run.contractTitle,
            mode = FreeMode.On ? "Free mode" : "Campaign",
            level = Difficulty.Name,
            rocket = run.rocketName, orbit = run.orbitName, parts = string.Join(", ", parts),
            stations = run.stations,
            result = (int)run.result,
            science = run.science, goal = run.scienceGoal, cost = run.cost, budget = run.budget,
            risk = run.riskPercent, score = score,
            riskFailure = run.riskFailure, eventHurt = run.eventHurt, eventTitle = run.eventTitle,
            massMargin = run.MassMargin, powerMargin = run.EnergyMargin,
            dataMade = run.dataGenerated, dataReceived = run.received,
            why = run.why, tryNext = run.tryNext,
        });
        if (flights.Count > Max) flights.RemoveRange(0, flights.Count - Max);

        PlayerPrefs.SetString("flight_log", JsonUtility.ToJson(new Store { flights = flights }));
        PlayerPrefs.Save();
    }

    // ---------- Análisis del vuelo ----------

    public static List<string> WentWell(FlightRecord f)
    {
        var list = new List<string>();
        if (f.science >= f.goal) list.Add($"Science goal reached: {f.science} of {f.goal}.");
        if (f.dataMade > 0f && f.dataReceived >= f.dataMade - 0.01f) list.Add("All the data your instruments made reached Earth.");
        if (f.massMargin >= 0.10f) list.Add($"Mass margin of {Pct(f.massMargin)}: the rocket had room to spare.");
        if (f.powerMargin >= MissionOutcome.StormSafeMargin) list.Add($"Power margin of {Pct(f.powerMargin)}: enough to ride out a solar storm.");
        if (!f.eventHurt && f.eventTitle != null && f.eventTitle != "ALL CALM") list.Add($"Your design survived the {f.eventTitle.ToLower()}.");
        if (f.mode != "Free mode" && f.cost <= f.budget) list.Add($"Stayed within budget: ${f.cost}M of ${f.budget}M.");
        if (!f.riskFailure && f.risk > 10) list.Add($"No part failed, even with {f.risk}% risk (lucky!).");
        return list;
    }

    public static List<string> WentWrong(FlightRecord f)
    {
        var list = new List<string>();
        if (f.science < f.goal) list.Add($"Science fell short: {f.science} of {f.goal}.");
        if (f.dataMade > 0f && f.dataReceived < f.dataMade - 0.01f)
            list.Add($"{Mathf.RoundToInt((1f - f.dataReceived / f.dataMade) * 100f)}% of the data never reached Earth ({f.dataReceived:0} of {f.dataMade:0} GB/day).");
        if (f.eventHurt) list.Add($"The {f.eventTitle.ToLower()} hurt the mission.");
        if (f.riskFailure) list.Add($"A part failed on the way up (risk was {f.risk}%), and half the science was lost.");
        if (f.powerMargin >= 0f && f.powerMargin < MissionOutcome.StormSafeMargin) list.Add($"Thin power margin ({Pct(f.powerMargin)}): a solar storm could switch off an instrument.");
        if (f.massMargin >= 0f && f.massMargin < 0.10f) list.Add($"Thin mass margin ({Pct(f.massMargin)}): a riskier launch.");
        return list;
    }

    static string Pct(float v) => Mathf.RoundToInt(v * 100f) + "%";
}
