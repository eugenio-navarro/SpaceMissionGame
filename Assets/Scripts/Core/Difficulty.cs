using UnityEngine;

public enum DifficultyLevel { Cadet, Engineer, FlightDirector }

// Niveles de dificultad (los pide el challenge: accesible para quien empieza y desafiante para quien sabe).
// Cadet: más presupuesto, menos riesgo y un diseño inicial que ya vuela, para mejorar de a una pieza.
// Engineer: los valores normales. Flight Director: menos presupuesto y más riesgo.
public static class Difficulty
{
    static int[] baseBudgets;

    public static DifficultyLevel Level
    {
        get => (DifficultyLevel)Mathf.Clamp(PlayerPrefs.GetInt("difficulty", 1), 0, 2);
        set { PlayerPrefs.SetInt("difficulty", (int)value); PlayerPrefs.Save(); Apply(); }
    }

    public static string Name => Level == DifficultyLevel.Cadet ? "CADET" : Level == DifficultyLevel.Engineer ? "ENGINEER" : "FLIGHT DIRECTOR";

    public static string Summary => Level == DifficultyLevel.Cadet
        ? "Cadet (beginner): +25% budget, half the risk, and a starter design you can improve."
        : Level == DifficultyLevel.Engineer
            ? "Engineer (standard): normal budget and risk. You build from scratch."
            : "Flight Director (expert): 15% less budget and higher risk. Every margin counts.";

    public static float BudgetFactor => Level == DifficultyLevel.Cadet ? 1.25f : Level == DifficultyLevel.Engineer ? 1f : 0.85f;
    public static float RiskFactor => Level == DifficultyLevel.Cadet ? 0.5f : Level == DifficultyLevel.Engineer ? 1f : 1.5f;
    public static bool StarterDesign => Level == DifficultyLevel.Cadet;

    public static void Next() => Level = (DifficultyLevel)(((int)Level + 1) % 3);

    // Ajusta el presupuesto de cada contrato según el nivel (se llama al arrancar y al cambiar de nivel).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Apply()
    {
        var contracts = TestCatalog.Contracts;
        if (baseBudgets == null)
        {
            baseBudgets = new int[contracts.Count];
            for (int i = 0; i < contracts.Count; i++) baseBudgets[i] = contracts[i].budget;
        }
        for (int i = 0; i < contracts.Count; i++)
            contracts[i].budget = Mathf.RoundToInt(baseBudgets[i] * BudgetFactor / 5f) * 5;
    }

    // Diseño inicial del nivel Cadet: despega, pero no llega a la meta. El jugador lo mejora.
    public static void Seed(ContractSpec c, MissionDesign d)
    {
        string[] ids = c.id == "moon" ? new[] { "cam_b", "panel_l", "bat", "ant_s", "grande", "lunar" }
            : c.id == "telescope" ? new[] { "cam_a", "panel_l", "bat", "ant_l", "mediano", "leo" }
            : new[] { "cam_b", "panel_l", "bat", "ant_s", "chico", "leo" };
        foreach (var id in ids)
        {
            var p = TestCatalog.Get(id);
            switch (p.category)
            {
                case PartCategory.Instrument: d.instruments.Add(p); break;
                case PartCategory.SolarPanel: d.panel = p; break;
                case PartCategory.Battery: d.battery = p; break;
                case PartCategory.Antenna: d.antenna = p; break;
                case PartCategory.Rocket: d.rocket = p; break;
                case PartCategory.Orbit: d.orbit = p; break;
            }
        }
        d.stations = c.id == "telescope" ? 2 : 1;
    }
}
