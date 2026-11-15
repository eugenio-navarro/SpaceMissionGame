using System.Text;
using UnityEngine;

// Guardado con PlayerPrefs (funciona en WebGL): contrato aceptado, diseño del hangar por contrato,
// mejores resultados, tutoriales vistos y opciones de sonido y pantalla.
public static class SaveData
{
    const string ContractKey = "contract";

    public static string AcceptedContract
    {
        get => PlayerPrefs.GetString(ContractKey, "");
        set { PlayerPrefs.SetString(ContractKey, value ?? ""); PlayerPrefs.Save(); }
    }

    public static bool HasProgress => PlayerPrefs.HasKey(ContractKey) || MissionsFlown > 0;

    // ---------- Diseño del hangar ----------
    // Formato: "inst1,inst2|panel|bateria|antena|cohete|orbita|estaciones"

    public static void SaveDesign(string contractId, MissionDesign d)
    {
        var ids = new StringBuilder();
        for (int i = 0; i < d.instruments.Count; i++) ids.Append(i > 0 ? "," : "").Append(d.instruments[i].id);
        string value = string.Join("|", ids.ToString(), Id(d.panel), Id(d.battery), Id(d.antenna), Id(d.rocket), Id(d.orbit), d.stations.ToString());
        PlayerPrefs.SetString("design_" + contractId, value);
        PlayerPrefs.Save();
    }

    // Devuelve false si no había un diseño guardado para ese contrato.
    public static bool LoadDesign(string contractId, MissionDesign d)
    {
        string value = PlayerPrefs.GetString("design_" + contractId, "");
        var parts = value.Split('|');
        if (parts.Length != 7) return false;

        d.instruments.Clear();
        foreach (var id in parts[0].Split(','))
        {
            var p = Part(id);
            if (p != null) d.instruments.Add(p);
        }
        d.panel = Part(parts[1]);
        d.battery = Part(parts[2]);
        d.antenna = Part(parts[3]);
        d.rocket = Part(parts[4]);
        d.orbit = Part(parts[5]);
        d.stations = int.TryParse(parts[6], out int s) ? Mathf.Clamp(s, 1, MissionCalculator.MaxStations) : 1;
        return true;
    }

    static string Id(PartSpec p) => p != null ? p.id : "";
    static PartSpec Part(string id) => string.IsNullOrEmpty(id) ? null : TestCatalog.Get(id);

    // ---------- Resultados (archivo de misiones) ----------

    public static int MissionsFlown => PlayerPrefs.GetInt("flown", 0);

    // Guarda una misión volada y el mejor resultado de cada contrato (0 = nada, 1 = falló, 2 = parcial, 3 = éxito).
    public static void RecordMission(string contractTitle, ResultKind result, int science)
    {
        PlayerPrefs.SetInt("flown", MissionsFlown + 1);
        int rank = result == ResultKind.Success ? 3 : result == ResultKind.Partial ? 2 : 1;
        string key = "best_" + contractTitle;
        if (rank > PlayerPrefs.GetInt(key, 0)) PlayerPrefs.SetInt(key, rank);
        if (science > PlayerPrefs.GetInt(key + "_science", -1)) PlayerPrefs.SetInt(key + "_science", science);
        PlayerPrefs.Save();
    }

    public static int BestRank(string contractTitle) => PlayerPrefs.GetInt("best_" + contractTitle, 0);
    public static int BestScience(string contractTitle) => PlayerPrefs.GetInt("best_" + contractTitle + "_science", 0);

    // ---------- Tutoriales ----------

    public static bool Seen(string key) => PlayerPrefs.GetInt("seen_" + key, 0) == 1;
    public static void MarkSeen(string key) { PlayerPrefs.SetInt("seen_" + key, 1); PlayerPrefs.Save(); }

    public static void ResetTutorials()
    {
        PlayerPrefs.DeleteKey("seen_tut_hub");
        PlayerPrefs.DeleteKey("seen_tut_hangar");
        PlayerPrefs.Save();
    }

    // ---------- Opciones ----------

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat("music", 0.6f);
        set { PlayerPrefs.SetFloat("music", value); PlayerPrefs.Save(); }
    }

    public static float EffectsVolume
    {
        get => PlayerPrefs.GetFloat("effects", 0.8f);
        set { PlayerPrefs.SetFloat("effects", value); PlayerPrefs.Save(); }
    }

    // Borra la partida (contrato, diseños, resultados y tutoriales) pero conserva las opciones.
    public static void NewGame()
    {
        float music = MusicVolume, effects = EffectsVolume;
        PlayerPrefs.DeleteAll();
        MusicVolume = music;
        EffectsVolume = effects;
    }
}
