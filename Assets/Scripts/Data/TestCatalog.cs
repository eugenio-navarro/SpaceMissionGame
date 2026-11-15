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
            description = "Simple photos. Cheap and light. Real example: the small cameras on CubeSats, satellites built from 10 cm cubes." },
        new PartSpec { id = "cam_a", displayName = "Advanced camera", category = PartCategory.Instrument, mass = 120, power = 80, cost = 50, dataRate = 60,
            description = "Lots of detail, but heavy and power-hungry. Real example: Landsat 9's OLI camera sees Earth in 9 bands of light." },
        new PartSpec { id = "espec", displayName = "Spectrometer", category = PartCategory.Instrument, mass = 90, power = 60, cost = 40, dataRate = 40,
            description = "Splits light to find out what things are made of. Real example: LRO's LAMP hunts for ice in the Moon's dark craters." },
        new PartSpec { id = "radar", displayName = "Radar", category = PartCategory.Instrument, mass = 150, power = 120, cost = 60, dataRate = 50,
            description = "Sees through clouds and at night. Very heavy. Real example: the L-band radar on Argentina's SAOCOM satellites (CONAE)." },

        // Energía
        new PartSpec { id = "panel_s", displayName = "Small solar panel", category = PartCategory.SolarPanel, mass = 20, generation = 120, cost = 10,
            description = "Makes 120 W in sunlight." },
        new PartSpec { id = "panel_l", displayName = "Large solar panel", category = PartCategory.SolarPanel, mass = 40, generation = 250, cost = 20,
            description = "Makes 250 W in sunlight. Real example: Hubble's panels make up to 5,500 W." },
        new PartSpec { id = "bat", displayName = "Battery", category = PartCategory.Battery, mass = 30, cost = 8,
            description = "Keeps everything running during eclipses, when the satellite is in shadow and the panels get no sunlight. Real example: Hubble carries six batteries." },

        // Antenas
        new PartSpec { id = "ant_s", displayName = "Small antenna", category = PartCategory.Antenna, mass = 10, power = 15, cost = 5, downlink = 40,
            description = "Sends up to 40 GB/day to Earth." },
        new PartSpec { id = "ant_l", displayName = "Large antenna", category = PartCategory.Antenna, mass = 25, power = 40, cost = 15, downlink = 100,
            description = "Sends up to 100 GB/day to Earth. Real example: LRO sends home about 900 gigabits (~112 GB) a day." },

        // Cohetes
        new PartSpec { id = "chico", displayName = "Small rocket", category = PartCategory.Rocket, capacity = 500, cost = 60,
            description = "Lifts 500 kg to low orbit. Less to higher orbits. Similar class: Rocket Lab's Electron (a few hundred kg)." },
        new PartSpec { id = "mediano", displayName = "Medium rocket", category = PartCategory.Rocket, capacity = 1200, cost = 110,
            description = "Lifts 1,200 kg to low orbit. Less to higher orbits. Similar class: Europe's Vega (about 1,500 kg to a 700 km polar orbit)." },
        new PartSpec { id = "grande", displayName = "Large rocket", category = PartCategory.Rocket, capacity = 3000, cost = 180,
            description = "Lifts 3,000 kg to low orbit. Less to higher orbits. Similar class: Europe's Vega-C (about 3,300 kg to low orbit)." },

        // Órbitas
        new PartSpec { id = "leo", displayName = "Low orbit", category = PartCategory.Orbit, contact = 0.5f, capacityMultiplier = 1f, hasShadow = true,
            lifeYears = 3, altitude = "~500 km", description = "Close to Earth, like Hubble (~540 km) or Landsat (~705 km): sharp images, but a ground station only hears it during short passes overhead." },
        new PartSpec { id = "geo", displayName = "Geostationary", category = PartCategory.Orbit, contact = 1f, capacityMultiplier = 0.4f, hasShadow = false,
            lifeYears = 8, altitude = "~36,000 km", description = "35,786 km up, it circles once a day, so it stays above the same spot: constant contact, but far away and hard to reach. Eclipses only happen for a few weeks around the equinoxes." },
        new PartSpec { id = "lunar", displayName = "Lunar orbit", category = PartCategory.Orbit, contact = 0.6f, capacityMultiplier = 0.3f, hasShadow = true,
            lifeYears = 3, altitude = "~50 km (Moon)", description = "Around the Moon, about 384,000 km from Earth, flying ~50 km above the surface like LRO. Rockets lift much less this far." },
    };

    public static readonly List<ContractSpec> Contracts = new List<ContractSpec>
    {
        new ContractSpec { id = "earth", title = "Landsat: Earth Watch", destination = "EARTH", objective = "Track changes in forests and ice.",
            scienceGoal = 60, budget = 300, unlocked = true, similarMission = "Landsat 9",
            allowedOrbits = new[] { "leo", "geo" }, lockedInstruments = new string[0],
            detail = new Dictionary<string, float> { { "leo", 1.5f }, { "geo", 0.8f } } },
        new ContractSpec { id = "telescope", title = "Hubble: Deep Field", destination = "DEEP SPACE", objective = "Study faraway galaxies.",
            scienceGoal = 70, budget = 400, unlocked = false, similarMission = "Hubble Space Telescope",
            allowedOrbits = new[] { "leo", "geo" }, lockedInstruments = new[] { "radar" },
            detail = new Dictionary<string, float> { { "leo", 0.9f }, { "geo", 1.0f } } },
        new ContractSpec { id = "moon", title = "LRO: Lunar Mapping", destination = "MOON", objective = "Map the lunar south pole.",
            scienceGoal = 40, budget = 500, unlocked = false, similarMission = "Lunar Reconnaissance Orbiter",
            allowedOrbits = new[] { "lunar" }, lockedInstruments = new string[0],
            detail = new Dictionary<string, float> { { "lunar", 1.2f } } },
    };

    public static PartSpec Get(string id) => Parts.Find(p => p.id == id);

    public static IEnumerable<PartSpec> ByCategory(PartCategory category) => Parts.Where(p => p.category == category);

    public static ContractSpec Contract(string id) => Contracts.Find(c => c.id == id) ?? Contracts[0];
}
