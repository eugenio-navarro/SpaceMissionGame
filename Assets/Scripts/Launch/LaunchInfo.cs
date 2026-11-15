using System;

// Datos que necesita la escena Lanzamiento para mostrarse.
// Se arman a partir de ResultsInfo (la foto de la misión que deja el Hangar, o datos de prueba).

public enum LaunchEventKind
{
    Calm,
    SolarStorm,
    StationInterference,
    CloudCover,
    ExtendedEclipse
}

[Serializable]
public class LaunchInfo
{
    public string missionName = "Landsat: Earth Watch";
    public string rocketId = "mediano"; // chico, mediano o grande (ids del README)
    public string rocketName = "Medium rocket";
    public string orbitId = "leo";
    public string orbitName = "Low orbit";
    public float orbitAltitudeKm = 500f;
    public string[] stageNames = { "Stage 1 - Booster", "Stage 2 - Upper stage", "Payload - Satellite" };
    public LaunchEventKind eventKind = LaunchEventKind.Calm;
    public string eventTitle = "ALL CALM";
    public string eventText = "No surprises this time. Space was quiet during the launch.";

    public static LaunchInfo FromRun(ResultsInfo run)
    {
        return new LaunchInfo
        {
            missionName = run.contractTitle,
            rocketId = run.rocketId,
            rocketName = run.rocketName,
            orbitId = run.orbitId,
            orbitName = run.orbitName,
            orbitAltitudeKm = AltitudeKm(run.orbitId),
            eventKind = run.eventKind,
            eventTitle = run.eventTitle,
            eventText = run.eventText
        };
    }

    static float AltitudeKm(string orbitId)
    {
        if (orbitId == "geo") return 36000f;
        if (orbitId == "lunar") return 384000f;
        return 500f;
    }
}

public static class LaunchContext
{
    // Queda en true cuando la secuencia terminó o se saltó. Lo puede leer Resultados.
    public static bool LaunchFinished;

    // Cómo le fue al jugador en el mini desafío de separación de etapas.
    public static SeparationRating Separation = SeparationRating.Automatic;
}
