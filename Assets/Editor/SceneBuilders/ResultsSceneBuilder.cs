using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static BuilderUtil;

// Arma la escena Resultados desde cero. Ojo: borra lo que haya en la escena.
// Usarlo para el armado inicial; si después se edita a mano, no volver a correrlo.
public static class ResultsSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Resultados.unity";
    const string GeneratedFolder = "Assets/Art/Generated";
    const float M = UIFactory.Margin;

    [MenuItem("Tools/Escenas/Construir Resultados")]
    public static string Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);

        var controller = new GameObject("ResultsController").AddComponent<ResultsController>();
        BuildSpace(controller);
        BuildUI(controller);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Resultados built";
    }

    // ---------- Espacio, planeta y satélite ----------

    static void BuildSpace(ResultsController controller)
    {
        var sky = new Material(Shader.Find("Skybox/Panoramic"));
        sky.SetTexture("_MainTex", GeneratedTexture("Starfield", 2048, 1024, StarPixel));
        sky.SetFloat("_Exposure", 1f);
        sky = SaveMaterial(sky, "StarSky");
        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.2f);

        controller.sun = AddLight("Sun", LightType.Directional, Vector3.zero, new Vector3(25f, -60f, 0f), 1.6f, new Color(1f, 0.97f, 0.92f));

        var world = new GameObject("World").transform;
        var earthMat = Mat("PlanetEarth", Color.white, 0f, 0.35f);
        earthMat.SetTexture("_BaseMap", GeneratedTexture("PlanetEarth", 1024, 512, EarthPixel));
        var moonMat = Mat("PlanetMoon", Color.white, 0f, 0.1f);
        moonMat.SetTexture("_BaseMap", GeneratedTexture("PlanetMoon", 1024, 512, MoonPixel));

        controller.earth = Prim(PrimitiveType.Sphere, "Earth", world, new Vector3(0f, -68f, 34f), Vector3.one * 120f, earthMat, new Vector3(20f, 0f, 10f));
        controller.moon = Prim(PrimitiveType.Sphere, "Moon", world, new Vector3(0f, -68f, 34f), Vector3.one * 120f, moonMat);
        controller.moon.SetActive(false);

        var ship = new GameObject("Ship").transform;
        var visual = ship.gameObject.AddComponent<SatelliteVisual>();
        visual.satellite = Group("Satellite", ship, Vector3.zero);
        HangarSceneBuilder.BuildSatellite(visual);
        controller.satellite = visual;

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 40f;
        cam.farClipPlane = 1000f;
        cam.clearFlags = CameraClearFlags.Skybox;
        camGo.AddComponent<AudioListener>();
        var orbit = camGo.AddComponent<ResultsCamera>();
        orbit.target = visual.satellite.Find("Body");
        camGo.transform.position = new Vector3(3f, 2.4f, -6.5f);
        camGo.transform.LookAt(new Vector3(0f, 0.3f, 0f));
    }

    static Material SaveMaterial(Material m, string name)
    {
        string path = $"Assets/Art/Materials/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) { existing.shader = m.shader; existing.CopyPropertiesFromMaterial(m); EditorUtility.SetDirty(existing); return existing; }
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    delegate Color PixelFn(int x, int y, int w, int h);

    // Genera (una sola vez) una textura procedural y la guarda como PNG.
    static Texture2D GeneratedTexture(string name, int w, int h, PixelFn fn)
    {
        string path = $"{GeneratedFolder}/{name}.png";
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(GeneratedFolder);
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            Random.InitState(name.GetHashCode());
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, fn(x, y, w, h));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapModeU = TextureWrapMode.Repeat;
            importer.wrapModeV = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static Color StarPixel(int x, int y, int w, int h)
    {
        var bg = new Color(0.016f, 0.024f, 0.05f);
        // Franja tenue tipo Vía Láctea
        float band = Mathf.Exp(-Mathf.Pow((y - h * (0.5f + 0.15f * Mathf.Sin(x * 6.283f / w))) / (h * 0.08f), 2f));
        float cloud = Mathf.PerlinNoise(x * 0.01f, y * 0.01f);
        bg += new Color(0.05f, 0.05f, 0.08f) * band * cloud;
        float r = Random.value;
        if (r > 0.9975f) return Color.Lerp(bg, new Color(0.85f, 0.9f, 1f), Random.Range(0.4f, 1f));
        if (r > 0.993f) return Color.Lerp(bg, Color.white, Random.Range(0.1f, 0.35f));
        return bg;
    }

    static float Fbm(float x, float y)
    {
        float v = 0f, amp = 0.5f, f = 1f;
        for (int i = 0; i < 5; i++) { v += amp * Mathf.PerlinNoise(x * f + 31.7f * i, y * f + 17.3f * i); amp *= 0.5f; f *= 2f; }
        return v;
    }

    static Color EarthPixel(int x, int y, int w, int h)
    {
        float u = (float)x / w, v = (float)y / h;
        float lat = Mathf.Abs(v - 0.5f) * 2f;
        // Ruido "envuelto" en X para que no se note la costura
        float n = Mathf.Lerp(Fbm(u * 6f, v * 3f), Fbm((u - 1f) * 6f, v * 3f), u);
        Color ocean = Color.Lerp(Palette.Hex("#1B4F9C"), Palette.Hex("#2E6FC0"), n);
        Color land = Color.Lerp(Palette.Hex("#3E7A3A"), Palette.Hex("#9C8A55"), Mathf.Clamp01((n - 0.55f) * 4f));
        Color c = n > 0.52f ? land : ocean;
        if (lat > 0.82f) c = Color.Lerp(c, Color.white, Mathf.Clamp01((lat - 0.82f) * 8f)); // hielo en los polos
        float clouds = Mathf.Lerp(Fbm(u * 10f + 5f, v * 5f), Fbm((u - 1f) * 10f + 5f, v * 5f), u);
        return Color.Lerp(c, Color.white, Mathf.Clamp01((clouds - 0.55f) * 3f) * 0.8f);
    }

    static Color MoonPixel(int x, int y, int w, int h)
    {
        float u = (float)x / w, v = (float)y / h;
        float n = Mathf.Lerp(Fbm(u * 8f, v * 4f), Fbm((u - 1f) * 8f, v * 4f), u);
        float maria = Mathf.Lerp(Fbm(u * 3f + 9f, v * 1.5f), Fbm((u - 1f) * 3f + 9f, v * 1.5f), u);
        float g = 0.55f + (n - 0.5f) * 0.5f - Mathf.Clamp01((maria - 0.55f) * 3f) * 0.2f;
        return new Color(g, g, g * 1.02f);
    }

    // ---------- UI ----------

    static void BuildUI(ResultsController c)
    {
        var root = CreateCanvas().transform;
        const float W = 480f, Gap = 20f;

        // Columna izquierda: resultado, por qué, qué probar
        var result = Card(root, "ResultCard", Palette.Green, new Vector2(0f, 1f), new Vector2(M, -M), new Vector2(W, 230f), 0f);
        c.resultAccent = result.Find("Accent").GetComponent<Image>();
        c.resultCaption = Caption(result, "Mission result");
        c.statusText = UIFactory.Label("Status", result, "SUCCESS", 56, Palette.Green, FontKind.Title);
        UIFactory.Place(c.statusText.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -120f), new Vector2(-20f, -46f));
        c.scienceText = UIFactory.Label("Science", result, "0", 52, Palette.Green, FontKind.Mono);
        UIFactory.Place(c.scienceText.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(28f, 18f), new Vector2(-20f, 96f));
        c.scienceText.gameObject.AddComponent<CountUp>();

        float whyTop = M + 230f + Gap;
        var why = Card(root, "WhyCard", Palette.Blue, new Vector2(0f, 1f), new Vector2(M, -whyTop), new Vector2(W, 480f), 0.12f);
        Caption(why, "Why it went this way");
        c.whyText = Body(why, 24);

        float tipTop = whyTop + 480f + Gap;
        var tip = UIFactory.Card("TipCard", root, Palette.Orange);
        UIFactory.Place(tip, Vector2.zero, new Vector2(0f, 1f), new Vector2(M, M), new Vector2(M + W, -tipTop));
        UIFactory.Appear(tip.gameObject, 0.24f);
        Caption(tip, "Try this");
        c.tipText = Body(tip, 24);

        // Columna derecha: evento, misión real, trade study, botones
        var ev = Card(root, "EventCard", Palette.Blue, Vector2.one, new Vector2(-M, -M), new Vector2(W, 230f), 0.06f);
        Caption(ev, "Launch event");
        c.eventTitle = UIFactory.Label("Title", ev, "", 32, Palette.TextMain, FontKind.Title);
        UIFactory.Place(c.eventTitle.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -92f), new Vector2(-20f, -46f));
        c.eventText = UIFactory.Label("Text", ev, "", 24, Palette.TextSecondary, FontKind.Body, TextAnchor.UpperLeft);
        UIFactory.Place(c.eventText.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 52f), new Vector2(-20f, -98f));
        c.riskText = UIFactory.Label("Risk", ev, "", 22, Palette.TextMain, FontKind.Mono);
        UIFactory.Place(c.riskText.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(28f, 14f), new Vector2(-20f, 46f));

        float realTop = M + 230f + Gap;
        var real = Card(root, "RealMissionCard", Palette.Blue, Vector2.one, new Vector2(-M, -realTop), new Vector2(W, 200f), 0.18f);
        Caption(real, "Real mission like yours");
        c.missionName = UIFactory.Label("Name", real, "", 30, Palette.TextMain, FontKind.Title);
        UIFactory.Place(c.missionName.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -90f), new Vector2(-20f, -46f));
        c.missionInfo = UIFactory.Label("Info", real, "", 22, Palette.TextSecondary, FontKind.Body, TextAnchor.UpperLeft);
        UIFactory.Place(c.missionInfo.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 14f), new Vector2(-20f, -96f));

        float tradeTop = realTop + 200f + Gap;
        float buttonsHeight = 76f + 16f + 64f;
        var trade = UIFactory.Card("TradeCard", root, Palette.Green);
        UIFactory.Place(trade, new Vector2(1f, 0f), Vector2.one, new Vector2(-M - W, M + buttonsHeight + Gap), new Vector2(-M, -tradeTop));
        UIFactory.Appear(trade.gameObject, 0.3f);
        Caption(trade, "With a different choice");
        c.tradeText = Body(trade, 24);

        c.tryAgainButton = UIFactory.ButtonCard("TryAgain", root, "TRY AGAIN", 34, null, Palette.Orange);
        UIFactory.Pin((RectTransform)c.tryAgainButton.transform, new Vector2(1f, 0f), new Vector2(-M, M + 64f + 16f), new Vector2(W, 76f));
        c.hubButton = UIFactory.ButtonCard("SpaceCenter", root, "<  SPACE CENTER", 26);
        UIFactory.Pin((RectTransform)c.hubButton.transform, new Vector2(1f, 0f), new Vector2(-M, M), new Vector2(W, 64f));
    }

    static RectTransform Card(Transform root, string name, Color accent, Vector2 anchor, Vector2 pos, Vector2 size, float delay)
    {
        var card = UIFactory.Card(name, root, accent);
        UIFactory.Pin(card, anchor, pos, size);
        UIFactory.Appear(card.gameObject, delay);
        return card;
    }

    static Text Caption(RectTransform card, string text)
    {
        var t = UIFactory.Caption("Caption", card, text);
        UIFactory.Place(t.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -42f), new Vector2(-20f, -18f));
        return t;
    }

    static Text Body(RectTransform card, int size)
    {
        var t = UIFactory.Label("Body", card, "", size, Palette.TextMain, FontKind.Body, TextAnchor.UpperLeft);
        UIFactory.Place(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 18f), new Vector2(-22f, -52f));
        return t;
    }
}
