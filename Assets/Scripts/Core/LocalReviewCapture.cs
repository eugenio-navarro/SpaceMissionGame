#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Explicit opt-in local evidence runner, excluded from release builds.
public class LocalReviewCapture : MonoBehaviour
{
    class Preference { public string key, value; public bool exists, integer; }
    readonly List<Preference> preferences = new List<Preference>();
    readonly List<string> errors = new List<string>();
    string output;
    bool restored;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--visual-review") < 0) return;
        var go = new GameObject("LocalReviewCapture");
        DontDestroyOnLoad(go);
        go.AddComponent<LocalReviewCapture>();
    }

    void Awake()
    {
        output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures"));
        Directory.CreateDirectory(output);
        foreach (string key in new[] { "contract", "design_earth", "design_telescope", "design_moon" }) Backup(key, false);
        foreach (string key in new[] { "seen_tut_hub", "seen_tut_hangar", "seen_tut_free", "difficulty", "free_mode", "log_seen" }) Backup(key, true);
        PlayerPrefs.SetInt("seen_tut_hub", 1);
        PlayerPrefs.SetInt("seen_tut_hangar", 1);
        PlayerPrefs.SetInt("seen_tut_free", 1);
        PlayerPrefs.SetInt("free_mode", 0);
        PlayerPrefs.SetInt("difficulty", 1);
        Difficulty.Apply();
        Application.runInBackground = true;
        Application.logMessageReceived += OnLog;
        Application.quitting += Restore;
    }

    void Backup(string key, bool integer)
    {
        preferences.Add(new Preference { key = key, integer = integer, exists = PlayerPrefs.HasKey(key), value = integer ? PlayerPrefs.GetInt(key).ToString() : PlayerPrefs.GetString(key) });
    }

    void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack);
    }

    IEnumerator Start()
    {
        try
        {
            var sizes = new[] { new Vector2Int(1920, 1080), new Vector2Int(1366, 768), new Vector2Int(1280, 800), new Vector2Int(1024, 768) };
            foreach (var size in sizes)
            {
                Screen.SetResolution(size.x, size.y, false);
                yield return new WaitForSecondsRealtime(0.6f);
                string prefix = size.x + "x" + size.y + "_";
                yield return Load("MenuPrincipal");
                yield return Capture(prefix + "menu");
                GameShell.Instance.OpenOptions(); yield return Capture(prefix + "options"); GameShell.Instance.CloseMenu();
                GameShell.Instance.OpenGuide(); yield return Capture(prefix + "guide"); GameShell.Instance.CloseCredits();
                GameShell.Instance.OpenCredits(); yield return Capture(prefix + "credits"); GameShell.Instance.CloseCredits();
                yield return Load("CentroEspacial");
                yield return Capture(prefix + "hub");
                var hub = FindAnyObjectByType<HubController>();
                hub.OnBuildingClicked(BuildingKind.MissionControl); yield return Capture(prefix + "contracts"); hub.CloseAll();
                hub.OnBuildingClicked(BuildingKind.Antennas); yield return Capture(prefix + "stations"); hub.CloseAll();
                hub.OnBuildingClicked(BuildingKind.Dome); yield return Capture(prefix + "archive"); hub.CloseAll();
                foreach (string id in new[] { "earth", "moon" })
                {
                    ResultsContext.Clear();
                    SaveData.AcceptedContract = id;
                    var design = ReferenceDesign(id);
                    SaveData.SaveDesign(id, design);
                    yield return Load("Hangar");
                    var ui = FindAnyObjectByType<HangarUI>();
                    var controller = FindAnyObjectByType<HangarController>();
                    yield return Capture(prefix + id + "_hangar");
                    if (size.x == 1920 && id == "earth") yield return MeasurePerformance();
                    if (id == "earth")
                    {
                        for (int tab = 0; tab < 4; tab++)
                        {
                            ui.ShowTab(tab); yield return Capture(prefix + "tab" + tab);
                            var scroll = ui.listContent.GetComponentInParent<ScrollRect>();
                            scroll.verticalNormalizedPosition = 0f; yield return Capture(prefix + "tab" + tab + "_bottom");
                        }
                    }
                    var tablet = FindAnyObjectByType<MissionTablet>(FindObjectsInactive.Include);
                    tablet.Open(); yield return Capture(prefix + id + "_forecast");
                    if (id == "earth")
                    {
                        controller.Toggle(TestCatalog.Get("ant_m"));
                        yield return Capture(prefix + "forecast_changed_antenna");
                        controller.Toggle(TestCatalog.Get("ant_l"));
                        yield return null;
                    }
                    tablet.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 0f;
                    yield return Capture(prefix + id + "_forecast_bottom"); tablet.Close();
                    var review = FindAnyObjectByType<MissionReview>(FindObjectsInactive.Include);
                    review.Open(); yield return Capture(prefix + id + "_review"); review.Close();
                    controller.Launch();
                    ResultsContext.IsTestData = true;
                    MissionOutcome.Resolve(ResultsContext.Current, id == "earth" ? new[] { LaunchEventKind.SolarStorm, LaunchEventKind.StationInterference, LaunchEventKind.CloudCover } : new[] { LaunchEventKind.SolarStorm, LaunchEventKind.StationInterference, LaunchEventKind.ExtendedEclipse }, 99f);
                    yield return new WaitForSecondsRealtime(2f);
                    yield return Capture(prefix + id + "_launch");
                    var hud = FindAnyObjectByType<LaunchHud>();
                    var run = ResultsContext.Current;
                    hud.ShowEvent(run.eventTitle, run.eventText, !run.eventHurt);
                    yield return Capture(prefix + id + "_events");
                    FindAnyObjectByType<LaunchSequence>().Skip();
                    yield return new WaitForSecondsRealtime(6f);
                    yield return Capture(prefix + id + "_results");
                    ResultsContext.IsTestData = false;
                    FindAnyObjectByType<ResultsController>().redesignButton.onClick.Invoke();
                    yield return new WaitForSecondsRealtime(1.5f);
                    var redesigned = FindAnyObjectByType<HangarController>();
                    if (redesigned == null || redesigned.Design.battery?.id != "bat_extended")
                        errors.Add("Redesign did not preserve the extended battery for " + id);
                    if (id == "earth" && redesigned != null)
                    {
                        redesigned.ClearDesign();
                        yield return Capture(prefix + "empty_hangar");
                    }
                }
            }
            File.WriteAllLines(Path.Combine(output, "errors.txt"), errors);
            File.WriteAllText(Path.Combine(output, "complete.txt"), "Capture completed. Frames=" + Time.frameCount + "; Memory=" + UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong());
        }
        finally { Restore(); }
        Application.Quit(errors.Count == 0 ? 0 : 1);
    }

    static MissionDesign ReferenceDesign(string id)
    {
        var design = new MissionDesign { panel = TestCatalog.Get("panel_l"), battery = TestCatalog.Get("bat_extended"), antenna = TestCatalog.Get("ant_l"), rocket = TestCatalog.Get(id == "moon" ? "grande" : "chico"), orbit = TestCatalog.Get(id == "moon" ? "lunar" : "leo"), stations = 2 };
        design.instruments.Add(TestCatalog.Get("cam_a"));
        return design;
    }

    IEnumerator Load(string scene)
    {
        SceneManager.LoadScene(scene);
        yield return new WaitForSecondsRealtime(1.2f);
    }

    IEnumerator Capture(string name)
    {
        yield return new WaitForSecondsRealtime(0.4f);
        Canvas.ForceUpdateCanvases();
        ScreenCapture.CaptureScreenshot(Path.Combine(output, name + ".png"));
        yield return new WaitForSecondsRealtime(0.15f);
    }

    IEnumerator MeasurePerformance()
    {
        var frames = new List<float>();
        float end = Time.realtimeSinceStartup + 10f;
        while (Time.realtimeSinceStartup < end)
        {
            yield return null;
            frames.Add(Time.unscaledDeltaTime * 1000f);
        }
        frames.Sort();
        float sum = 0f;
        foreach (float frame in frames) sum += frame;
        File.WriteAllText(Path.Combine(output, "performance.txt"),
            "Windows development player, 1920x1080 hangar, 10 seconds after warmup\nFrames=" + frames.Count +
            "\nMeanMs=" + sum / frames.Count + "\nP95Ms=" + frames[Mathf.Min(frames.Count - 1, Mathf.CeilToInt(frames.Count * 0.95f) - 1)] +
            "\nAllocatedBytes=" + UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong());
    }

    void Restore()
    {
        if (restored) return;
        restored = true;
        foreach (var p in preferences)
        {
            if (!p.exists) PlayerPrefs.DeleteKey(p.key);
            else if (p.integer) PlayerPrefs.SetInt(p.key, int.Parse(p.value));
            else PlayerPrefs.SetString(p.key, p.value);
        }
        PlayerPrefs.Save();
        Application.logMessageReceived -= OnLog;
        Application.quitting -= Restore;
    }
}
#endif
