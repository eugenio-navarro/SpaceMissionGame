using System;
using UnityEngine;

// Estado del armado en el hangar: qué piezas eligió el jugador y cómo da el cálculo.
public class HangarController : MonoBehaviour
{
    public string launchScene = "Lanzamiento";
    public string hubScene = "CentroEspacial";

    public MissionDesign Design { get; } = new MissionDesign();
    public ContractSpec Contract { get; private set; }
    public MissionReport Report { get; private set; }

    public event Action Changed;
    public bool StartedFromSeed { get; private set; }

    void Awake()
    {
        Contract = TestCatalog.Contract(HubController.AcceptedContractId);

        // Volviendo de Resultados con "Redesign": se recupera el diseño anterior.
        var previous = ResultsContext.Current != null ? ResultsContext.Current.design : null;
        if (previous != null && !ResultsContext.IsTestData)
        {
            Design.instruments.AddRange(previous.instruments);
            Design.panel = previous.panel;
            Design.battery = previous.battery;
            Design.antenna = previous.antenna;
            Design.rocket = previous.rocket;
            Design.orbit = previous.orbit;
            Design.stations = previous.stations;
        }
        else if (!SaveData.LoadDesign(Contract.id, Design) && Difficulty.StarterDesign) // lo que quedó armado la última vez
        {
            Difficulty.Seed(Contract, Design); // nivel Cadet: arranca con un diseño que vuela, para mejorarlo
            StartedFromSeed = true;
        }
        Recalculate();
    }

    public bool IsSelected(PartSpec p)
    {
        switch (p.category)
        {
            case PartCategory.Instrument: return Design.instruments.Contains(p);
            case PartCategory.SolarPanel: return Design.panel == p;
            case PartCategory.Battery: return Design.battery == p;
            case PartCategory.Antenna: return Design.antenna == p;
            case PartCategory.Rocket: return Design.rocket == p;
            case PartCategory.Orbit: return Design.orbit == p;
            default: return false;
        }
    }

    // Por qué una pieza no se puede elegir en este contrato, o null si se puede.
    public string UnavailableReason(PartSpec p)
    {
        if (p.category == PartCategory.Instrument && Contract.IsLocked(p.id)) return "not in this contract";
        if (p.category == PartCategory.Orbit && !Contract.AllowsOrbit(p.id)) return "not allowed";
        return null;
    }

    // Agrega o quita una pieza. Devuelve un mensaje si no se pudo.
    public string Toggle(PartSpec p)
    {
        if (UnavailableReason(p) != null) return $"{p.displayName} isn't available in this contract.";

        switch (p.category)
        {
            case PartCategory.Instrument:
                if (Design.instruments.Contains(p)) Design.instruments.Remove(p);
                else if (Design.instruments.Count >= MissionCalculator.MaxInstruments)
                    return $"Max {MissionCalculator.MaxInstruments} instruments. Remove one first.";
                else Design.instruments.Add(p);
                break;
            case PartCategory.SolarPanel: Design.panel = Design.panel == p ? null : p; break;
            case PartCategory.Battery: Design.battery = Design.battery == p ? null : p; break;
            case PartCategory.Antenna: Design.antenna = Design.antenna == p ? null : p; break;
            case PartCategory.Rocket: Design.rocket = Design.rocket == p ? null : p; break;
            case PartCategory.Orbit: Design.orbit = Design.orbit == p ? null : p; break;
        }
        Recalculate();
        return null;
    }

    public void SetStations(int count)
    {
        Design.stations = Mathf.Clamp(count, 1, MissionCalculator.MaxStations);
        Recalculate();
    }

    // Saca todas las piezas (botón CLEAR ALL).
    public void ClearDesign()
    {
        Design.instruments.Clear();
        Design.panel = Design.battery = Design.antenna = Design.rocket = Design.orbit = null;
        Design.stations = 1;
        Recalculate();
    }

    public void Launch()
    {
        if (!Report.CanLaunch) return;
        // Foto de la misión para Lanzamiento y Resultados (el evento y el dado se sortean en Lanzamiento).
        ResultsContext.Current = MissionHandoff.Build(Design, Report, Contract);
        ResultsContext.IsTestData = false;
        GameShell.Load(launchScene);
    }

    public void BackToHub()
    {
        ResultsContext.Clear(); // el diseño guardado era para este contrato
        GameShell.Load(hubScene);
    }

    void Recalculate()
    {
        Report = MissionCalculator.Evaluate(Design, Contract, FreeMode.On);
        SaveData.SaveDesign(Contract.id, Design);
        Changed?.Invoke();
    }
}
