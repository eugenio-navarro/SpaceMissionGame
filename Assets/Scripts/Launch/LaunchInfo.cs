using System;
using UnityEngine;

// Datos que necesita la escena Lanzamiento.
// Por ahora salen de datos de prueba; cuando exista MissionState, se llena LaunchContext.Current desde ahí.

public enum LaunchEventKind
{
    Calm,
    SolarStorm,
    StationInterference
}

[Serializable]
public class LaunchInfo
{
    public string missionName = "Observe the Earth";
    public string rocketName = "Small rocket";
    public string orbitName = "Low Earth orbit";
    public float orbitAltitudeKm = 500f;
    public string[] stageNames = { "Stage 1 - Booster", "Stage 2 - Upper stage", "Payload - Satellite" };
    public LaunchEventKind eventKind = LaunchEventKind.Calm;
    public string eventTitle = "ALL CALM";
    public string eventText = "No surprises this time. Space was quiet during the launch.";
}

public static class LaunchContext
{
    // Lo setea quien carga la escena (Hangar / MissionState). Si es null se usan datos de prueba.
    public static LaunchInfo Current;

    // Queda en true cuando la secuencia terminó o se saltó. Lo puede leer Resultados.
    public static bool LaunchFinished;

    public static LaunchInfo GetOrCreateTestData()
    {
        if (Current != null) return Current;

        var info = new LaunchInfo();
        int roll = UnityEngine.Random.Range(0, 3);
        if (roll == 1)
        {
            info.eventKind = LaunchEventKind.SolarStorm;
            info.eventTitle = "SOLAR STORM";
            info.eventText = "A burst of particles from the Sun hit the spacecraft. " +
                             "Your power margin is 36%, so everything stays on.";
        }
        else if (roll == 2)
        {
            info.eventKind = LaunchEventKind.StationInterference;
            info.eventTitle = "GROUND STATION INTERFERENCE";
            info.eventText = "One antenna on Earth lost the signal. " +
                             "With only 2 stations, 20% less data comes down.";
        }

        Current = info;
        return info;
    }
}
