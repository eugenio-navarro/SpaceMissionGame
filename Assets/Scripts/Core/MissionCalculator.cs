using System.Collections.Generic;
using UnityEngine;

// Diseño elegido por el jugador en el hangar.
public class MissionDesign
{
    public readonly List<PartSpec> instruments = new List<PartSpec>();
    public PartSpec panel, battery, antenna, rocket, orbit;
    public int stations = 1;

    public IEnumerable<PartSpec> SatelliteParts()
    {
        foreach (var i in instruments) yield return i;
        if (panel != null) yield return panel;
        if (battery != null) yield return battery;
        if (antenna != null) yield return antenna;
    }
}

// Resultado de evaluar un diseño: totales, márgenes, bloqueos y advertencias.
public class MissionReport
{
    public float mass, consumption, generation, capacity;
    public float dataGenerated, downlink, received;
    public float energyMargin, massMargin;
    public int cost, science, riskPercent;
    public readonly List<string> blockers = new List<string>();
    public readonly List<string> warnings = new List<string>();
    public bool CanLaunch => blockers.Count == 0;
}

// Fórmulas del README (sección 4). Clase pura: no depende de escenas.
public static class MissionCalculator
{
    public const float BaseMass = 150f;
    public const float BasePower = 40f;
    public const int BaseCost = 30;
    public const int StationCost = 10;
    public const int MaxInstruments = 3;
    public const int MaxStations = 4;
    public static readonly float[] StationFactor = { 0.5f, 0.8f, 1f, 1.15f };

    public static MissionReport Evaluate(MissionDesign d, ContractSpec contract, bool freeMode = false)
    {
        var r = new MissionReport { mass = BaseMass, consumption = BasePower, cost = BaseCost };

        foreach (var p in d.SatelliteParts())
        {
            r.mass += p.mass;
            r.consumption += p.power;
            r.cost += p.cost;
        }

        int stations = Mathf.Clamp(d.stations, 1, MaxStations);
        if (d.rocket != null) r.cost += d.rocket.cost;
        r.cost += StationCost * stations;

        bool shadowWithoutBattery = d.orbit != null && d.orbit.hasShadow && d.battery == null;
        r.generation = d.panel != null ? d.panel.generation * (shadowWithoutBattery ? 0.65f : 1f) : 0f;
        r.capacity = d.rocket != null ? d.rocket.capacity * (d.orbit != null ? d.orbit.capacityMultiplier : 1f) : 0f;

        foreach (var i in d.instruments) r.dataGenerated += i.dataRate;
        r.downlink = d.antenna != null && d.orbit != null
            ? d.antenna.downlink * d.orbit.contact * StationFactor[stations - 1]
            : 0f;
        r.received = Mathf.Min(r.dataGenerated, r.downlink);
        r.science = d.orbit != null ? Mathf.FloorToInt(r.received * contract.DetailFor(d.orbit.id) + 0.5f) : 0;

        r.energyMargin = r.generation > 0f ? (r.generation - r.consumption) / r.generation : -1f;
        r.massMargin = r.capacity > 0f ? (r.capacity - r.mass) / r.capacity : -1f;
        r.riskPercent = 5
            + (r.energyMargin < 0.10f ? 25 : r.energyMargin < 0.30f ? 10 : 0)
            + (r.massMargin < 0.10f ? 10 : 0);
        r.riskPercent = Mathf.Max(1, Mathf.RoundToInt(r.riskPercent * Difficulty.RiskFactor));

        AddBlockers(d, contract, r, freeMode);
        AddWarnings(d, contract, r);
        return r;
    }

    static void AddBlockers(MissionDesign d, ContractSpec c, MissionReport r, bool freeMode)
    {
        if (d.instruments.Count == 0) r.blockers.Add("Add at least one instrument (Science tab).");
        if (d.panel == null) r.blockers.Add("Add a solar panel (Power tab).");
        if (d.antenna == null) r.blockers.Add("Add an antenna (Comms tab).");
        if (d.rocket == null) r.blockers.Add("Choose a rocket (Rocket tab).");
        if (d.orbit == null) r.blockers.Add("Choose an orbit (Rocket tab).");
        else if (!c.AllowsOrbit(d.orbit.id)) r.blockers.Add($"This contract doesn't allow {d.orbit.displayName.ToLower()}.");

        foreach (var i in d.instruments)
            if (c.IsLocked(i.id)) r.blockers.Add($"The {i.displayName.ToLower()} is locked in this contract.");

        if (d.rocket != null && d.orbit != null && r.mass > r.capacity)
            r.blockers.Add($"Too heavy: the satellite weighs {r.mass:0} kg, but the {d.rocket.displayName.ToLower()} " +
                           $"can only lift {r.capacity:0} kg to {d.orbit.displayName.ToLower()}.");

        if (d.panel != null && r.consumption > r.generation)
            r.blockers.Add($"Not enough power: parts use {r.consumption:0} W, but the panels make {r.generation:0} W.");

        if (!freeMode && r.cost > c.budget)
            r.blockers.Add($"Over budget: ${r.cost}M of ${c.budget}M.");
    }

    // Solo sugiere arreglos que todavía se pueden hacer con este diseño.
    public static string DataFix(MissionDesign d)
    {
        bool moreStations = d.stations < MaxStations;
        bool biggerAntenna = d.antenna != null && d.antenna.downlink < BiggestDownlink();
        if (moreStations && biggerAntenna) return "Add ground stations or a bigger antenna so more data comes down.";
        if (moreStations) return "Add ground stations so more data comes down.";
        if (biggerAntenna) return "Use a bigger antenna so more data comes down.";
        return "Your antenna and stations are maxed out: use instruments that make less data, or an orbit with more contact time.";
    }

    static float BiggestDownlink()
    {
        float best = 0f;
        foreach (var p in TestCatalog.ByCategory(PartCategory.Antenna)) best = Mathf.Max(best, p.downlink);
        return best;
    }

    static void AddWarnings(MissionDesign d, ContractSpec c, MissionReport r)
    {
        if (d.orbit != null && d.orbit.hasShadow && d.panel != null && d.battery == null)
            r.warnings.Add("No battery: in shadow (eclipse) the panels make no power, so on average you only get 65%.");

        if (d.antenna != null && d.orbit != null && r.dataGenerated > r.downlink)
            r.warnings.Add($"Data jam: you make {r.dataGenerated:0} GB/day, but only {r.downlink:0} reach Earth. " +
                           DataFix(d));

        // Mismo umbral que el evento de tormenta solar (MissionOutcome): con menos, apaga un instrumento.
        if (r.energyMargin >= 0f && r.energyMargin < MissionOutcome.StormSafeMargin)
            r.warnings.Add($"Power margin under {MissionOutcome.StormSafeMargin * 100f:0}%: a solar storm could switch off an instrument.");

        if (r.massMargin >= 0f && r.massMargin < 0.10f)
            r.warnings.Add("Mass margin under 10%: a risky launch.");

        if (d.orbit != null && d.antenna != null && d.instruments.Count > 0 && r.science < c.scienceGoal)
            r.warnings.Add($"Estimated science ({r.science}) is below the goal ({c.scienceGoal}).");
    }
}
