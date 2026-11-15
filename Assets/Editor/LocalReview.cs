using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Local deterministic regression checks and a reproducible review build.
public static class LocalReview
{
    static int checks;

    public static void Verify()
    {
        checks = 0;
        Check((int)LaunchEventKind.Calm == 0 && (int)LaunchEventKind.SolarStorm == 1 && (int)LaunchEventKind.StationInterference == 2, "Legacy event IDs");
        Check(TestCatalog.Parts.Select(p => p.id).Distinct().Count() == TestCatalog.Parts.Count, "Unique part IDs");
        Near(TestCatalog.Get("panel_m").generation, 180, "Medium panel");
        Near(TestCatalog.Get("ant_m").downlink, 70, "Medium antenna");
        Near(TestCatalog.Get("bat_extended").mass, 55, "Extended battery");

        var run = Snapshot("earth", "cam_a", "radar", "espec");
        MissionOutcome.Resolve(run, LaunchEventKind.CloudCover, 100);
        Near(run.received, 120, "Clouds affect optical camera only");
        run.downlink = 40;
        MissionOutcome.Resolve(run, LaunchEventKind.CloudCover, 100);
        Near(run.received, 40, "Downlink bottleneck absorbs production loss");
        run = Snapshot("earth", "cam_a");
        MissionOutcome.Resolve(run, LaunchEventKind.CloudCover, 100);
        Near(run.received, 30, "Camera cloud production");
        run = Snapshot("earth", "radar");
        MissionOutcome.Resolve(run, LaunchEventKind.CloudCover, 100);
        Near(run.received, 50, "Radar cloud production");

        run = Snapshot("moon", "cam_a");
        foreach (var item in new[] { (id: "", expected: 36f), (id: "bat", expected: 51f), (id: "bat_extended", expected: 60f) })
        {
            run.design.battery = TestCatalog.Get(item.id);
            MissionOutcome.Resolve(run, LaunchEventKind.ExtendedEclipse, 100);
            Near(run.received, item.expected, "Eclipse battery " + item.id);
        }

        run = Snapshot("earth", "cam_a", "radar");
        run.consumption = 240;
        run.downlink = 100;
        var events = new[] { LaunchEventKind.CloudCover, LaunchEventKind.SolarStorm, LaunchEventKind.StationInterference, LaunchEventKind.CloudCover };
        MissionOutcome.Resolve(run, events, 100);
        Near(run.received, 30, "Storm removes radar once, cloud halves remaining camera");
        Check(run.occurredEvents.Length == 3, "Deduplicated simultaneous events");
        MissionOutcome.Resolve(run, events.Reverse().ToArray(), 100);
        Near(run.received, 30, "Order independence and repeated resolve");
        MissionOutcome.Resolve(run, events, 0);
        Check(run.science == 15, "Hardware failure applied once");
        MissionOutcome.Resolve(run, LaunchEventKind.Calm, 100);
        Near(run.received, 100, "Calm after previous resolution uses original data");

        run = Snapshot("moon", "cam_a", "radar");
        run.consumption = 240;
        run.design.battery = TestCatalog.Get("bat");
        MissionOutcome.Resolve(run, new[] { LaunchEventKind.SolarStorm, LaunchEventKind.ExtendedEclipse }, 100);
        Near(run.received, 51, "Storm and eclipse compose");
        run = Snapshot("earth", "cam_a");
        MissionOutcome.Resolve(run, new[] { LaunchEventKind.ExtendedEclipse, (LaunchEventKind)999 }, 100);
        Check(run.occurredEvents.Length == 0 && run.eventKind == LaunchEventKind.Calm, "Ineligible forced events normalize");
        var standalone = new ResultsInfo();
        MissionOutcome.Resolve(standalone, new[] { LaunchEventKind.CloudCover, LaunchEventKind.ExtendedEclipse }, 100);
        Check(standalone.occurredEvents.Length == 0, "Designless scene fallback");
        Check(MissionForecast.RollEvents(run, () => 0).Length == 3, "All eligible independent rolls hit");
        Check(MissionForecast.RollEvents(run, () => 100).Length == 0, "No hazard roll hit");
        Check(MissionForecast.RollEvents(run, () => 15).SequenceEqual(new[] { LaunchEventKind.CloudCover }), "Probability threshold uses strict comparison");
        var forecast = MissionForecast.Build(Design("earth"), TestCatalog.Contract("earth"));
        Near(forecast.noneProbability, 50.575f, "No-event probability");
        Near(forecast.multipleProbability, 9.9f, "Simultaneous probability");
        Check(MissionForecast.Build(new MissionDesign(), TestCatalog.Contract("earth")).rows.Length > 0, "Empty design forecast");

        Near(SaveData.EfficiencyBonus(300, 203, true, false), 97, "Successful savings");
        Near(SaveData.EfficiencyBonus(300, 100, false, false), 0, "Failed mission no savings");
        Near(SaveData.EfficiencyBonus(300, 100, true, true), 0, "Free mode no savings");
        Near(SaveData.EfficiencyBonus(300, 400, true, false), 0, "Overbudget no savings");
        Near(SaveData.EfficiencyBonus(300, -100, true, false), 300, "Savings capped at budget");

        string saveId = "codex-review-" + Guid.NewGuid().ToString("N");
        try
        {
            var design = Design("moon");
            design.panel = TestCatalog.Get("panel_m");
            design.antenna = TestCatalog.Get("ant_m");
            design.battery = TestCatalog.Get("bat_extended");
            SaveData.SaveDesign(saveId, design);
            var loaded = new MissionDesign();
            Check(SaveData.LoadDesign(saveId, loaded), "Load saved design");
            Check(loaded.panel.id == "panel_m" && loaded.antenna.id == "ant_m" && loaded.battery.id == "bat_extended", "New IDs round trip");
            Check(TradeStudy.Clone(loaded).battery.id == "bat_extended", "Redesign clone");
            Check(!string.IsNullOrEmpty(TradeStudy.Best(loaded, TestCatalog.Contract("moon"))), "Trade study evaluates alternatives");
        }
        finally { PlayerPrefs.DeleteKey("design_" + saveId); PlayerPrefs.Save(); }

        foreach (var item in new[] { (id: "earth", science: 60), (id: "telescope", science: 80), (id: "moon", science: 58) })
        {
            var design = Design(item.id);
            var report = MissionCalculator.Evaluate(design, TestCatalog.Contract(item.id));
            Check(report.CanLaunch, "Reference launch " + item.id);
            Check(report.science == item.science, "Reference science " + item.id + " got " + report.science);
        }
        var lunar = Design("moon");
        lunar.rocket = TestCatalog.Get("mediano");
        Check(!MissionCalculator.Evaluate(lunar, TestCatalog.Contract("moon")).CanLaunch, "Lunar mass rejection");
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/local-review-checks.txt", checks + " deterministic checks passed.\n");
        Debug.Log("LOCAL_REVIEW_PASS " + checks);
    }

    public static MissionDesign Design(string contract)
    {
        var d = new MissionDesign {
            panel = TestCatalog.Get("panel_l"), battery = TestCatalog.Get("bat"), antenna = TestCatalog.Get("ant_l"),
            rocket = TestCatalog.Get(contract == "moon" ? "grande" : contract == "telescope" ? "mediano" : "chico"),
            orbit = TestCatalog.Get(contract == "moon" ? "lunar" : contract == "telescope" ? "geo" : "leo"), stations = 2
        };
        d.instruments.Add(TestCatalog.Get("cam_a"));
        if (contract == "telescope") { d.instruments.Add(TestCatalog.Get("espec")); d.battery = null; }
        return d;
    }

    static ResultsInfo Snapshot(string contract, params string[] instruments)
    {
        var d = Design(contract);
        d.instruments.Clear();
        foreach (string id in instruments) d.instruments.Add(TestCatalog.Get(id));
        d.battery = null;
        return new ResultsInfo {
            destination = TestCatalog.Contract(contract).destination, orbitId = d.orbit.id, design = d,
            dataGenerated = d.instruments.Sum(p => p.dataRate), downlink = 1000, detail = 1,
            generation = 250, consumption = 100, riskPercent = 5, scienceGoal = 60, stations = 2
        };
    }

    static void Near(float actual, float expected, string name) => Check(Mathf.Abs(actual - expected) < 0.002f, name + " expected " + expected + " got " + actual);
    static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("LOCAL_REVIEW_FAIL " + name);
        checks++;
    }

    public static void BuildWeb()
    {
        Verify();
        const string output = "Builds/WebReview";
        Directory.CreateDirectory(output);
        var compression = PlayerSettings.WebGL.compressionFormat;
        try
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.WebGL, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Web build failed: " + report.summary.result);
            // Keep the internal product name stable for existing saved games.
            string index = Path.Combine(output, "index.html");
            File.WriteAllText(index, File.ReadAllText(index).Replace(PlayerSettings.productName, "Mission Control: Build to Launch"));
            Debug.Log("WEB_BUILD_PASS " + report.summary.totalSize + " bytes");
        }
        finally { PlayerSettings.WebGL.compressionFormat = compression; }
    }

    public static void BuildWindows()
    {
        Verify();
        Directory.CreateDirectory("Builds/LocalReview");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = "Builds/LocalReview/Mission Control - Build to Launch.exe",
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Local build failed: " + report.summary.result);
        Debug.Log("LOCAL_BUILD_PASS " + report.summary.totalSize + " bytes");
    }
}
