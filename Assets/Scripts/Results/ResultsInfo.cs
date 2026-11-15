using System;

// Todo lo que necesitan Lanzamiento y Resultados sobre la misión que se lanzó.
// La primera mitad es una foto del diseño (la llena el Hangar a partir de MissionDesign y MissionReport);
// la segunda es el resultado, que calcula MissionOutcome.Resolve.

public enum ResultKind
{
    Success,
    Partial,
    Failed
}

[Serializable]
public class ResultsInfo
{
    // ----- Contrato -----
    public string contractTitle = "Observe the Earth";
    public string objective = "Track changes in forests and ice.";
    public string destination = "EARTH"; // EARTH, DEEP SPACE o MOON
    public string similarMission = "Landsat";
    public int scienceGoal = 60;
    public int budget = 300;

    // ----- Diseño (solución de referencia "Tierra" del README) -----
    public string rocketId = "chico";
    public string rocketName = "Small rocket";
    public string orbitId = "leo";
    public string orbitName = "Low orbit";
    public int stations = 2;
    public float mass = 365f;
    public float capacity = 500f;
    public float consumption = 160f;
    public float generation = 250f;
    public float dataGenerated = 60f;
    public float downlink = 40f;
    public float detail = 1.5f;          // factor de detalle del contrato para esta órbita
    public int cost = 203;
    public int riskPercent = 5;
    public string hungriestInstrument = "Advanced camera"; // el instrumento que más consume
    public float hungriestInstrumentData = 60f;            // GB/día que aporta ese instrumento

    // ----- Resultado (lo completa MissionOutcome.Resolve) -----
    public bool resolved;
    public LaunchEventKind eventKind;
    public string eventTitle;
    public string eventText;
    public bool eventHurt;
    public bool riskFailure;
    public float received;
    public int science;
    public ResultKind result;
    public string headline;
    public string why;
    public string tryNext;
    public string capcomLine;
    public string similarMissionNote;

    public float EnergyMargin => generation > 0f ? (generation - consumption) / generation : -1f;
    public float MassMargin => capacity > 0f ? (capacity - mass) / capacity : -1f;
    public float BudgetMargin => budget > 0 ? (budget - cost) / (float)budget : -1f;
}

public static class ResultsContext
{
    // Lo setea el Hangar antes de cargar Lanzamiento. Si es null se usan datos de prueba.
    public static ResultsInfo Current;

    // True cuando Current salió de los datos de prueba y no de una partida real.
    public static bool IsTestData;

    public static ResultsInfo GetOrCreate()
    {
        if (Current == null)
        {
            Current = new ResultsInfo();
            IsTestData = true;
        }
        return Current;
    }

    // En el Editor los estáticos pueden sobrevivir entre un Play y el siguiente: se limpian al arrancar.
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        Current = null;
        IsTestData = false;
    }
}
