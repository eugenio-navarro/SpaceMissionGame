using System;
using System.Collections.Generic;
using UnityEngine;

// Probabilities are game balance values, independent of equipment mitigation.
public static class MissionForecast
{
    public static readonly LaunchEventKind[] Hazards = { LaunchEventKind.SolarStorm, LaunchEventKind.StationInterference, LaunchEventKind.CloudCover, LaunchEventKind.ExtendedEclipse };
    public class Row
    {
        public LaunchEventKind kind;
        public float probability;
        public int science;
        public bool mitigated;
        public string mitigation;
    }
    public class Forecast
    {
        public Row[] rows;
        public ResultsInfo baseline, combined;
        public float noneProbability, multipleProbability;
        public int hardwareRisk;
    }
    public static float EventProbability(ResultsInfo run, LaunchEventKind kind)
    {
        if (!MissionOutcome.IsEligible(run, kind)) return 0f;
        switch (kind)
        {
            case LaunchEventKind.SolarStorm:
            case LaunchEventKind.StationInterference: return 15f;
            case LaunchEventKind.CloudCover: return 30f;
            case LaunchEventKind.ExtendedEclipse: return 25f;
            default: return 0f;
        }
    }
    public static LaunchEventKind[] RollEvents(ResultsInfo run, Func<float> nextRoll)
    {
        var selected = new List<LaunchEventKind>();
        foreach (var kind in Hazards)
        {
            float probability = EventProbability(run, kind);
            if (probability > 0f && nextRoll() < probability) selected.Add(kind);
        }
        return selected.ToArray();
    }
    public static string Name(LaunchEventKind kind)
    {
        switch (kind)
        {
            case LaunchEventKind.SolarStorm: return "Solar storm";
            case LaunchEventKind.StationInterference: return "Ground station interference";
            case LaunchEventKind.CloudCover: return "Cloud cover";
            case LaunchEventKind.ExtendedEclipse: return "Extended lunar eclipse";
            default: return "Calm";
        }
    }
    public static string Mitigation(ResultsInfo run, LaunchEventKind kind)
    {
        switch (kind)
        {
            case LaunchEventKind.SolarStorm: return run.EnergyMargin >= MissionOutcome.StormSafeMargin ? "Spare power covers the modeled interruption (simplified game rule)." : "Below 20% spare power: highest-draw instrument shuts down (game rule).";
            case LaunchEventKind.StationInterference: return run.stations >= 3 ? "Backup stations cover the interruption." : "Fewer than 3 stations: downlink capacity drops 20%.";
            case LaunchEventKind.CloudCover: return "Camera production drops 50%; radar retains its contribution. Downlink limits can absorb the loss.";
            case LaunchEventKind.ExtendedEclipse: return $"Battery endurance retains {MissionOutcome.EclipseRetention(run) * 100f:0}% of production during this extended eclipse.";
            default: return "Normal operations.";
        }
    }
    public static Forecast Build(MissionDesign design, ContractSpec contract)
    {
        var report = MissionCalculator.Evaluate(design, contract);
        // Build even incomplete hangar designs without dereferencing missing parts.
        var snapshot = new ResultsInfo {
            contractTitle = contract.title, destination = contract.destination, scienceGoal = contract.scienceGoal, budget = contract.budget,
            orbitId = design.orbit?.id, design = TradeStudy.Clone(design), dataGenerated = report.dataGenerated,
            downlink = report.downlink, detail = contract.DetailFor(design.orbit?.id), cost = report.cost,
            generation = report.generation, consumption = report.consumption, riskPercent = report.riskPercent, stations = design.stations
        };
        var result = new Forecast { baseline = Preview(snapshot, Array.Empty<LaunchEventKind>()), hardwareRisk = report.riskPercent };
        var rows = new List<Row>();
        var eligible = new List<LaunchEventKind>();
        float none = 1f;
        foreach (var kind in Hazards)
        {
            float probability = EventProbability(snapshot, kind);
            if (probability <= 0f) continue;
            eligible.Add(kind);
            var preview = Preview(snapshot, new[] { kind });
            rows.Add(new Row { kind = kind, probability = probability, science = preview.science, mitigated = !preview.eventHurt, mitigation = Mitigation(snapshot, kind) });
            none *= 1f - probability / 100f;
        }
        float exactlyOne = 0f;
        foreach (var row in rows) exactlyOne += none * (row.probability / 100f) / (1f - row.probability / 100f);
        result.rows = rows.ToArray();
        result.noneProbability = none * 100f;
        result.multipleProbability = Mathf.Clamp01(1f - none - exactlyOne) * 100f;
        result.combined = Preview(snapshot, eligible);
        return result;
    }
    static ResultsInfo Preview(ResultsInfo source, IReadOnlyList<LaunchEventKind> events)
    {
        var copy = JsonUtility.FromJson<ResultsInfo>(JsonUtility.ToJson(source));
        copy.design = TradeStudy.Clone(source.design);
        MissionOutcome.Resolve(copy, events, 100f);
        return copy;
    }
}
