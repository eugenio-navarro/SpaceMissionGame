using System.Collections.Generic;
using System.Linq;

// Datos de prueba con los valores del README (sección 4).
// TEMPORAL: al final se pasan a ScriptableObjects y se borra esta clase.
public static class TestCatalog
{
    public static readonly List<PartSpec> Parts = new List<PartSpec>
    {
        // Instrumentos
        new PartSpec { id = "cam_b", displayName = "Basic camera", category = PartCategory.Instrument, mass = 60, power = 30, cost = 20, dataRate = 20,
            description = "Simple photos. Cheap and light." },
        new PartSpec { id = "cam_a", displayName = "Advanced camera", category = PartCategory.Instrument, mass = 120, power = 80, cost = 50, dataRate = 60,
            description = "Lots of detail, but heavy and power-hungry." },
        new PartSpec { id = "espec", displayName = "Spectrometer", category = PartCategory.Instrument, mass = 90, power = 60, cost = 40, dataRate = 40,
            description = "Splits light to find out what things are made of." },
        new PartSpec { id = "radar", displayName = "Radar", category = PartCategory.Instrument, mass = 150, power = 120, cost = 60, dataRate = 50,
            description = "Sees through clouds and at night. Very heavy." },

        // Energía
        new PartSpec { id = "panel_s", displayName = "Small solar panel", category = PartCategory.SolarPanel, mass = 20, generation = 120, cost = 10,
            description = "Makes 120 W in sunlight." },
        new PartSpec { id = "panel_l", displayName = "Large solar panel", category = PartCategory.SolarPanel, mass = 40, generation = 250, cost = 20,
            description = "Makes 250 W in sunlight." },
        new PartSpec { id = "bat", displayName = "Battery", category = PartCategory.Battery, mass = 30, cost = 8,
            description = "Keeps everything running while the satellite is in Earth's shadow." },

        // Antenas
        new PartSpec { id = "ant_s", displayName = "Small antenna", category = PartCategory.Antenna, mass = 10, power = 15, cost = 5, downlink = 40,
            description = "Sends up to 40 GB/day to Earth." },
        new PartSpec { id = "ant_l", displayName = "Large antenna", category = PartCategory.Antenna, mass = 25, power = 40, cost = 15, downlink = 100,
            description = "Sends up to 100 GB/day to Earth." },

        // Cohetes
        new PartSpec { id = "chico", displayName = "Small rocket", category = PartCategory.Rocket, capacity = 500, cost = 60,
            description = "Lifts 500 kg to low orbit. Less to higher orbits." },
        new PartSpec { id = "mediano", displayName = "Medium rocket", category = PartCategory.Rocket, capacity = 1200, cost = 110,
            description = "Lifts 1,200 kg to low orbit. Less to higher orbits." },
        new PartSpec { id = "grande", displayName = "Large rocket", category = PartCategory.Rocket, capacity = 3000, cost = 180,
            description = "Lifts 3,000 kg to low orbit. Less to higher orbits." },

        // Órbitas
        new PartSpec { id = "leo", displayName = "Low orbit", category = PartCategory.Orbit, contact = 0.5f, capacityMultiplier = 1f, hasShadow = true,
            lifeYears = 3, altitude = "~500 km", description = "Close to Earth: sharp images, but stations see it only part of the day." },
        new PartSpec { id = "geo", displayName = "Geostationary", category = PartCategory.Orbit, contact = 1f, capacityMultiplier = 0.4f, hasShadow = false,
            lifeYears = 8, altitude = "~36,000 km", description = "Always above the same spot: constant contact, but far away and hard to reach." },
        new PartSpec { id = "lunar", displayName = "Lunar orbit", category = PartCategory.Orbit, contact = 0.6f, capacityMultiplier = 0.3f, hasShadow = true,
            lifeYears = 3, altitude = "~384,000 km", description = "Around the Moon. Rockets lift much less this far." },
    };

    public static readonly List<ContractSpec> Contracts = new List<ContractSpec>
    {
        new ContractSpec { id = "earth", title = "Observe the Earth", destination = "EARTH", objective = "Track changes in forests and ice.",
            scienceGoal = 60, budget = 300, unlocked = true, similarMission = "Landsat", similarMissionInfo = "NASA and USGS satellites watching Earth's forests, ice and cities since 1972.",
            allowedOrbits = new[] { "leo", "geo" }, lockedInstruments = new string[0],
            detail = new Dictionary<string, float> { { "leo", 1.5f }, { "geo", 0.8f } } },
        new ContractSpec { id = "telescope", title = "Space Telescope", destination = "DEEP SPACE", objective = "Study faraway galaxies.",
            scienceGoal = 70, budget = 400, unlocked = false, similarMission = "Hubble", similarMissionInfo = "A space telescope in low orbit since 1990, looking at faraway galaxies.",
            allowedOrbits = new[] { "leo", "geo" }, lockedInstruments = new[] { "radar" },
            detail = new Dictionary<string, float> { { "leo", 0.9f }, { "geo", 1.0f } } },
        new ContractSpec { id = "moon", title = "Explore the Moon", destination = "MOON", objective = "Map the lunar south pole.",
            scienceGoal = 40, budget = 500, unlocked = false, similarMission = "Lunar Reconnaissance Orbiter", similarMissionInfo = "NASA orbiter mapping the Moon in detail since 2009, poles included.",
            allowedOrbits = new[] { "lunar" }, lockedInstruments = new string[0],
            detail = new Dictionary<string, float> { { "lunar", 1.2f } } },
    };

    public static PartSpec Get(string id) => Parts.Find(p => p.id == id);

    public static IEnumerable<PartSpec> ByCategory(PartCategory category) => Parts.Where(p => p.category == category);

    public static ContractSpec Contract(string id) => Contracts.Find(c => c.id == id) ?? Contracts[0];
}
