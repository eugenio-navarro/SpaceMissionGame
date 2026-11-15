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
    public static void RecordMission(string contractTitle, ResultKind result, int science, string rocket, string orbit, string headline)
    {
        PlayerPrefs.SetString("last_" + contractTitle, $"{rocket}|{orbit}|{(int)result}|{science}|{headline}");
        PlayerPrefs.SetInt("flown", MissionsFlown + 1);
        int rank = result == ResultKind.Success ? 3 : result == ResultKind.Partial ? 2 : 1;
        string key = "best_" + contractTitle;
        if (rank > PlayerPrefs.GetInt(key, 0)) PlayerPrefs.SetInt(key, rank);
        if (science > PlayerPrefs.GetInt(key + "_science", -1)) PlayerPrefs.SetInt(key + "_science", science);
        PlayerPrefs.Save();
    }

    // Último vuelo de un contrato, o null si nunca se voló.
    public static LastFlight Last(string contractTitle)
    {
        var p = PlayerPrefs.GetString("last_" + contractTitle, "").Split('|');
        if (p.Length < 5) return null;
        return new LastFlight
        {
            rocket = p[0], orbit = p[1],
            result = (ResultKind)(int.TryParse(p[2], out int r) ? r : 0),
            science = int.TryParse(p[3], out int sc) ? sc : 0,
            headline = string.Join("|", p, 4, p.Length - 4),
        };
    }

    public class LastFlight
    {
        public string rocket, orbit, headline;
        public ResultKind result;
        public int science;
    }

    // ---------- Ranking local ----------
    // Puntaje = ciencia x 10 + millones que sobraron del presupuesto (premia hacer más con menos).

    public static int Score(int science, int budget, int cost) => science * 10 + Mathf.Max(0, budget - cost);

    // Guarda el puntaje en el top 5 del contrato. Devuelve el puesto (1 a 5) o 0 si no entró.
    public static int AddScore(string contractTitle, int score)
    {
        var list = new System.Collections.Generic.List<int>(TopScores(contractTitle)) { score };
        list.Sort((a, b) => b.CompareTo(a));
        if (list.Count > 5) list.RemoveRange(5, list.Count - 5);
        PlayerPrefs.SetString("top_" + contractTitle, string.Join(",", list));
        PlayerPrefs.Save();
        return list.IndexOf(score) + 1;
    }

    public static int[] TopScores(string contractTitle)
    {
        string raw = PlayerPrefs.GetString("top_" + contractTitle, "");
        if (raw.Length == 0) return new int[0];
        var parts = raw.Split(',');
        var result = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++) int.TryParse(parts[i], out result[i]);
        return result;
    }

    public static int BestRank(string contractTitle) => PlayerPrefs.GetInt("best_" + contractTitle, 0);
    public static int BestScience(string contractTitle) => PlayerPrefs.GetInt("best_" + contractTitle + "_science", 0);

    // ---------- Tutoriales ----------

    public static bool Seen(string key) => PlayerPrefs.GetInt("seen_" + key, 0) == 1;
    public static void MarkSeen(string key) { PlayerPrefs.SetInt("seen_" + key, 1); PlayerPrefs.Save(); }

    // Tutorial encendido = todavía queda alguna guía por mostrar. Se prende y apaga desde el menú principal.
    public static bool TutorialOn
    {
        get => !Seen("tut_hub") || !Seen("tut_hangar");
        set
        {
            if (value) ResetTutorials();
            else { MarkSeen("tut_hub"); MarkSeen("tut_hangar"); }
        }
    }

    public static void ResetTutorials()
    {
        PlayerPrefs.DeleteKey("seen_tut_hub");
        PlayerPrefs.DeleteKey("seen_tut_hangar");
        PlayerPrefs.DeleteKey("seen_tut_free");
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
