using System.Collections.Generic;
using System.Linq;

// Trade study: prueba cambios de una sola pieza y cuenta el que habría dado más ciencia
// (o la misma ciencia con menos riesgo). Clase pura, usa MissionCalculator.
public static class TradeStudy
{
    public static string Best(MissionDesign d, ContractSpec c)
    {
        var current = MissionCalculator.Evaluate(d, c);
        string bestText = null;
        MissionReport best = null;

        foreach (var (label, alt) in Alternatives(d, c))
        {
            var r = MissionCalculator.Evaluate(alt, c);
            if (!r.CanLaunch) continue;
            var reference = best ?? current;
            bool better = r.science > reference.science
                          || (r.science == reference.science && r.riskPercent < reference.riskPercent);
            if (better) { best = r; bestText = label; }
        }

        if (best == null) return "Your design was already the best one-change option. Nice work!";
        string cost = best.cost > current.cost ? $"${best.cost - current.cost}M more"
            : best.cost < current.cost ? $"${current.cost - best.cost}M less" : "same cost";
        if (best.science > current.science)
            return $"{bestText}: {best.science} science instead of {current.science} ({cost}).";
        return $"{bestText}: same science, but {best.riskPercent}% risk instead of {current.riskPercent}% ({cost}).";
    }

    static IEnumerable<(string, MissionDesign)> Alternatives(MissionDesign d, ContractSpec c)
    {
        if (d.stations < MissionCalculator.MaxStations)
        {
            var a = Clone(d);
            a.stations++;
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

        var battery = TestCatalog.ByCategory(PartCategory.Battery).First();
        var b = Clone(d);
        b.battery = d.battery == null ? battery : null;
        yield return (d.battery == null ? "Adding a battery" : "Without the battery", b);

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
}
