using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Estado del armado en el hangar: qué piezas eligió el jugador y cómo da el cálculo.
public class HangarController : MonoBehaviour
{
    public string launchScene = "Lanzamiento";
    public string hubScene = "CentroEspacial";

    public MissionDesign Design { get; } = new MissionDesign();
    public ContractSpec Contract { get; private set; }
    public MissionReport Report { get; private set; }

    // TEMPORAL hasta que exista MissionState: el último diseño lanzado (lo leen Resultados y "Try again").
    public static MissionDesign LastDesign;

    public event Action Changed;

    void Awake()
    {
        Contract = TestCatalog.Contract(HubController.AcceptedContractId);
        if (LastDesign != null)
        {
            var d = LastDesign;
            Design.instruments.AddRange(d.instruments);
            Design.panel = d.panel;
            Design.battery = d.battery;
            Design.antenna = d.antenna;
            Design.rocket = d.rocket;
            Design.orbit = d.orbit;
            Design.stations = d.stations;
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
        if (p.category == PartCategory.Instrument && Contract.IsLocked(p.id)) return "locked";
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

    public void Launch()
    {
        if (!Report.CanLaunch) return;
        LastDesign = OutcomeCalculator.Clone(Design);
        SceneManager.LoadScene(launchScene);
    }

    public void BackToHub() => SceneManager.LoadScene(hubScene);

    void Recalculate()
    {
        Report = MissionCalculator.Evaluate(Design, Contract);
        Changed?.Invoke();
    }
}
