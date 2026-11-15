using System;
using System.Collections.Generic;

// Especificaciones de piezas y contratos.
// TEMPORAL: al final se reemplazan por ScriptableObjects (PartData, RocketData, OrbitData, ContractData).

public enum PartCategory { Instrument, SolarPanel, Battery, Antenna, Rocket, Orbit }

[Serializable]
public class PartSpec
{
    public string id;
    public string displayName;
    public PartCategory category;
    public float mass;                     // kg
    public float power;                    // W que consume
    public float generation;               // W que genera (paneles)
    public int cost;                       // millones de USD
    public float dataRate;                 // GB/día que genera (instrumentos)
    public float downlink;                 // GB/día que baja (antenas)
    public float capacity;                 // kg a órbita baja (cohetes)
    public float contact;                  // factor de contacto (órbitas)
    public float capacityMultiplier = 1f;  // multiplicador de capacidad del cohete (órbitas)
    public bool hasShadow;                 // la órbita pasa por la sombra de la Tierra
    public int lifeYears;                  // vida útil (órbitas)
    public string altitude;                // texto de altura (órbitas)
    public string description;
}

[Serializable]
public class ContractSpec
{
    public string id;
    public string title;
    public string objective;
    public string destination;
    public string similarMission;
    public string similarMissionInfo;
    public int scienceGoal;
    public int budget;
    public bool unlocked;
    public string[] allowedOrbits;
    public string[] lockedInstruments;
    [NonSerialized] public Dictionary<string, float> detail; // factor de detalle por órbita

    public float DetailFor(string orbitId) =>
        detail != null && orbitId != null && detail.TryGetValue(orbitId, out var d) ? d : 0f;

    public bool AllowsOrbit(string orbitId) => Array.IndexOf(allowedOrbits, orbitId) >= 0;

    public bool IsLocked(string partId) => lockedInstruments != null && Array.IndexOf(lockedInstruments, partId) >= 0;
}
