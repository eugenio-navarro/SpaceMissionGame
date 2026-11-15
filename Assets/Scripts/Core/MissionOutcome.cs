using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum LaunchEvent { Calm, SolarStorm, StationInterference }
public enum MissionStatus { Success, Partial, Failed }

// Qué pasó en la misión: evento, dado de riesgo, ciencia final y explicaciones.
public class MissionOutcome
{
    public MissionReport report;          // cálculo base del diseño (sin imprevistos)
    public LaunchEvent launchEvent;
    public bool eventHurt;                // el evento afectó a la misión
    public string eventTitle, eventText;
    public bool riskFailed;
    public int finalScience;
    public MissionStatus status;
    public string why, tip, tradeStudy;
}

// Reglas del README (secciones 3 y 5): eventos, dado de riesgo, resultado y feedback explicativo.
public static class OutcomeCalculator
{
    public const float StormMarginLimit = 0.20f;
    public const int StationsToCover = 3;
    public const float InterferenceLoss = 0.20f;

    // Sorteo de evento y dado de riesgo. TEMPORAL: esto lo va a hacer la escena Lanzamiento.
    public static MissionOutcome Roll(MissionDesign d, ContractSpec c, System.Random rng)
    {
        var report = MissionCalculator.Evaluate(d, c);
        var ev = (LaunchEvent)rng.Next(3);
        bool riskFailed = rng.Next(100) < report.riskPercent;
        return Evaluate(d, c, ev, riskFailed);
    }

    public static MissionOutcome Evaluate(MissionDesign d, ContractSpec c, LaunchEvent ev, bool riskFailed)
    {
        var r = MissionCalculator.Evaluate(d, c);
        var o = new MissionOutcome { report = r, launchEvent = ev, riskFailed = riskFailed };
        float detail = d.orbit != null ? c.DetailFor(d.orbit.id) : 0f;

        int science = r.science;
        float received = r.received;
        float dataMade = r.dataGenerated;
        string shutDown = null;

        switch (ev)
        {
            case LaunchEvent.SolarStorm:
                o.eventTitle = "Solar storm";
                if (r.energyMargin < StormMarginLimit && d.instruments.Count > 0)
                {
                    var hungriest = d.instruments.OrderByDescending(i => i.power).First();
                    var reduced = Clone(d);
                    reduced.instruments.Remove(hungriest);
                    var rr = MissionCalculator.Evaluate(reduced, c);
                    science = rr.science;
                    received = rr.received;
                    dataMade = rr.dataGenerated;
                    shutDown = hungriest.displayName.ToLower();
                    o.eventHurt = true;
                    o.eventText = $"Power margin was {Pct(r.energyMargin)}: the {shutDown} shut down to save energy.";
                }
                else o.eventText = $"Your {Pct(r.energyMargin)} power margin kept everything running.";
                break;

            case LaunchEvent.StationInterference:
                o.eventTitle = "Station interference";
                if (d.stations < StationsToCover)
                {
                    received = Mathf.Min(r.dataGenerated, r.downlink * (1f - InterferenceLoss));
                    science = Mathf.FloorToInt(received * detail + 0.5f);
                    o.eventHurt = true;
                    o.eventText = $"With only {d.stations} ground station{(d.stations == 1 ? "" : "s")}, 20% less data came down.";
                }
                else o.eventText = $"Your {d.stations} ground stations covered for the one that failed.";
                break;

            default:
                o.eventTitle = "Calm flight";
                o.eventText = "No surprises this time.";
                break;
        }

        o.finalScience = riskFailed ? Mathf.FloorToInt(science * 0.5f) : science;
        o.status = o.finalScience >= c.scienceGoal ? MissionStatus.Success
            : o.finalScience >= 0.6f * c.scienceGoal ? MissionStatus.Partial
            : MissionStatus.Failed;

        o.why = BuildWhy(o, c, dataMade, received, shutDown);
        o.tip = BuildTip(o, d, dataMade, received);
        o.tradeStudy = TradeStudy(d, c);
        return o;
    }

    static string BuildWhy(MissionOutcome o, ContractSpec c, float dataMade, float received, string shutDown)
    {
        var r = o.report;
        var lines = new List<string>();
        switch (o.status)
        {
            case MissionStatus.Success: lines.Add($"You got {o.finalScience} science, the goal was {c.scienceGoal}."); break;
            case MissionStatus.Partial: lines.Add($"Partial mission: {o.finalScience} of {c.scienceGoal} science."); break;
            default: lines.Add($"Only {o.finalScience} of {c.scienceGoal} science came back."); break;
        }

        if (dataMade > received + 0.5f)
        {
            int lost = Mathf.RoundToInt((1f - received / dataMade) * 100f);
            lines.Add($"Your instruments made {dataMade:0} GB/day, but only {received:0} reached Earth: {lost}% of the data was lost.");
        }

        if (o.eventHurt && shutDown != null)
            lines.Add($"A solar storm hit with only {Pct(r.energyMargin)} power margin, so the {shutDown} was switched off.");
        else if (o.eventHurt)
            lines.Add("A ground station had interference and there weren't enough others to cover it.");

        if (o.riskFailed)
            lines.Add($"Bad luck: the {r.riskPercent}% failure risk came true and half the science was lost.");
        else if (r.riskPercent > 10)
            lines.Add($"Your risk was {r.riskPercent}%, but this time luck was on your side.");

        return string.Join("\n\n", lines);
    }

    static string BuildTip(MissionOutcome o, MissionDesign d, float dataMade, float received)
    {
        var r = o.report;
        if (o.eventHurt && o.launchEvent == LaunchEvent.SolarStorm)
            return "Keep at least 20% power margin: use a larger solar panel or fewer power-hungry instruments.";
        if (o.eventHurt)
            return "Contract 3 or more ground stations so one failure doesn't cut your data.";
        if (dataMade > received + 0.5f)
            return "Add a ground station or a bigger antenna so all your data reaches Earth.";
        if (o.riskFailed || r.riskPercent > 10)
            return "Leave more margin (power above 30%, mass above 10%) to lower the failure risk.";
        if (o.status != MissionStatus.Success)
            return "Add an instrument that makes more data, or pick an orbit that sees more detail.";
        return "Great design! Can you do it cheaper or with less risk?";
    }

    // Prueba cambios de una sola pieza y cuenta el que más ciencia habría dado.
    static string TradeStudy(MissionDesign d, ContractSpec c)
    {
        var current = MissionCalculator.Evaluate(d, c);
        string bestText = null;
        MissionReport best = null;

        foreach (var (label, alt) in Alternatives(d, c))
        {
            var r = MissionCalculator.Evaluate(alt, c);
            if (!r.CanLaunch) continue;
            bool better = best == null
                ? r.science > current.science || (r.science == current.science && r.riskPercent < current.riskPercent)
                : r.science > best.science || (r.science == best.science && r.riskPercent < best.riskPercent);
            if (better) { best = r; bestText = label; }
        }

        if (best == null) return "Your design was already the best one-change option. Nice work!";
        string cost = best.cost > current.cost ? $"${best.cost - current.cost}M more" : best.cost < current.cost ? $"${current.cost - best.cost}M less" : "same cost";
        if (best.science > current.science)
            return $"{bestText}: {best.science} science instead of {current.science} ({cost}).";
        return $"{bestText}: same science, risk {best.riskPercent}% instead of {current.riskPercent}% ({cost}).";
    }

    static IEnumerable<(string, MissionDesign)> Alternatives(MissionDesign d, ContractSpec c)
    {
        if (d.stations < MissionCalculator.MaxStations)
        {
            var a = Clone(d); a.stations++;
            yield return ("One more ground station", a);
        }
        foreach (var p in TestCatalog.ByCategory(PartCategory.Antenna).Where(p => p != d.antenna))
        { var a = Clone(d); a.antenna = p; yield return ($"The {p.displayName.ToLower()}", a); }
        foreach (var p in TestCatalog.ByCategory(PartCategory.SolarPanel).Where(p => p != d.panel))
        { var a = Clone(d); a.panel = p; yield return ($"The {p.displayName.ToLower()}", a); }
        foreach (var p in TestCatalog.ByCategory(PartCategory.Rocket).Where(p => p != d.rocket))
        { var a = Clone(d); a.rocket = p; yield return ($"The {p.displayName.ToLower()}", a); }
        foreach (var p in TestCatalog.ByCategory(PartCategory.Orbit).Where(p => p != d.orbit && c.AllowsOrbit(p.id)))
        { var a = Clone(d); a.orbit = p; yield return ($"{p.displayName} instead", a); }
        {
            var battery = TestCatalog.ByCategory(PartCategory.Battery).First();
            var a = Clone(d); a.battery = d.battery == null ? battery : null;
            yield return (d.battery == null ? "Adding a battery" : "Without the battery", a);
        }
        foreach (var p in TestCatalog.ByCategory(PartCategory.Instrument).Where(p => !c.IsLocked(p.id)))
        {
            var a = Clone(d);
            if (d.instruments.Contains(p)) { a.instruments.Remove(p); yield return ($"Without the {p.displayName.ToLower()}", a); }
            else if (d.instruments.Count < MissionCalculator.MaxInstruments) { a.instruments.Add(p); yield return ($"Adding the {p.displayName.ToLower()}", a); }
        }
    }

    public static MissionDesign Clone(MissionDesign d)
    {
        var c = new MissionDesign
        {
            panel = d.panel, battery = d.battery, antenna = d.antenna,
            rocket = d.rocket, orbit = d.orbit, stations = d.stations,
        };
        c.instruments.AddRange(d.instruments);
        return c;
    }

    static string Pct(float margin) => $"{Mathf.RoundToInt(margin * 100f)}%";
}
