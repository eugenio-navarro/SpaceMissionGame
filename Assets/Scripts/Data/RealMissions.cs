using System.Collections.Generic;

// Misiones reales para comparar con la del jugador (ficha en Resultados y tarjetas de contrato).
// Cada dato está verificado en la fuente indicada (ver REFERENCES.md).
public class RealMission
{
    public string name;    // nombre que se muestra
    public string agency;  // agencia y año de lanzamiento
    public string fact;    // un dato corto y verificable (una línea)
    public string source;  // de dónde sale el dato
}

public static class RealMissions
{
    // science.nasa.gov/mission/landsat-9
    public static readonly RealMission Landsat9 = new RealMission
    {
        name = "Landsat 9", agency = "NASA / USGS · 2021",
        fact = "705 km up. Images all of Earth's land every 16 days, over 700 scenes a day.",
        source = "science.nasa.gov/mission/landsat-9",
    };

    // eoportal.org/satellite-missions/saocom
    public static readonly RealMission Saocom = new RealMission
    {
        name = "SAOCOM 1A & 1B", agency = "CONAE, Argentina · 2018/2020",
        fact = "620 km up. Their L-band radar sees through clouds to map floods and soil moisture.",
        source = "eoportal.org/satellite-missions/saocom",
    };

    // science.nasa.gov (Hubble facts for the 25th anniversary)
    public static readonly RealMission Hubble = new RealMission
    {
        name = "Hubble Space Telescope", agency = "NASA / ESA · 1990",
        fact = "547 km up. Makes up to 5,500 W and uses about 2,100: that spare power is margin.",
        source = "science.nasa.gov (Hubble 25th anniversary facts)",
    };

    // science.nasa.gov/mission/lro/spacecraft-and-instruments
    public static readonly RealMission Lro = new RealMission
    {
        name = "Lunar Reconnaissance Orbiter", agency = "NASA · 2009",
        fact = "Mapped the Moon from about 50 km up and sends home about 900 gigabits a day.",
        source = "science.nasa.gov/mission/lro",
    };

    static readonly List<RealMission> All = new List<RealMission> { Landsat9, Saocom, Hubble, Lro };

    public static RealMission Find(string name) => All.Find(m => m.name == name);

    // La misión real más parecida al diseño del jugador: un satélite de Tierra con radar se parece
    // al SAOCOM argentino; el resto, a la misión del contrato.
    public static RealMission For(ContractSpec contract, MissionDesign design)
    {
        if (contract.id == "earth" && design != null && design.instruments.Exists(p => p.id == "radar")) return Saocom;
        return Find(contract.similarMission) ?? Landsat9;
    }
}
