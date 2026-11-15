using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Arma la escena Resultados desde cero: planeta, satélite, cámara y panel de resultados.
// Menú: Space Mission > Build Resultados Scene. Usa el mismo estilo de UI que Lanzamiento (LaunchHudBuilder).
public static class ResultsSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Resultados.unity";
    const string TextureDir = "Assets/Art/Textures/Planets/";
    const string GeneratedDir = "Assets/Art/Results";

    [MenuItem("Space Mission/Build Resultados Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Main Camera" || root.name == "Directional Light") continue;
            Object.DestroyImmediate(root);
        }
        if (!AssetDatabase.IsValidFolder(GeneratedDir)) AssetDatabase.CreateFolder("Assets/Art", "Results");

        Texture earth = AssetDatabase.LoadAssetAtPath<Texture>(TextureDir + "2k_earth_daymap.jpg");
        Texture moon = AssetDatabase.LoadAssetAtPath<Texture>(TextureDir + "2k_moon.jpg");
        Texture stars = AssetDatabase.LoadAssetAtPath<Texture>(TextureDir + "2k_stars_milky_way.jpg");

        // ---------- Cielo y luz ----------
        Material sky = Asset("Results_Sky", "Skybox/Panoramic");
        sky.SetTexture("_MainTex", stars);
        sky.SetFloat("_Exposure", 0.75f);
        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.10f, 0.12f, 0.18f);

        Light sun = Object.FindAnyObjectByType<Light>();
        if (sun == null)
        {
            sun = new GameObject("Directional Light").AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        sun.transform.rotation = Quaternion.Euler(22f, -48f, 0f);
        sun.intensity = 1.6f;
        sun.color = new Color(1f, 0.97f, 0.92f);

        // ---------- Planeta ----------
        Material planetMat = Lit("Results_Planet", Color.white, 0f, 0.12f);
        planetMat.SetTexture("_BaseMap", earth);

        GameObject planet = new GameObject("Planet", typeof(MeshFilter), typeof(MeshRenderer));
        planet.GetComponent<MeshFilter>().sharedMesh = SphereMesh();
        planet.GetComponent<MeshRenderer>().sharedMaterial = planetMat;
        planet.transform.position = new Vector3(4f, -62f, 34f);
        planet.transform.localScale = Vector3.one * 56f; // la malla tiene radio 1
        planet.transform.rotation = Quaternion.Euler(18f, 40f, 12f);

        // ---------- Satélite ----------
        List<Transform> wings = new List<Transform>();
        List<Renderer> panelRenderers = new List<Renderer>();
        GameObject satellite = Satellite(wings, panelRenderers);

        // ---------- Cámara ----------
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 45f;
        cam.farClipPlane = 2000f;
        ResultsCamera orbitCam = cam.GetComponent<ResultsCamera>();
        if (orbitCam == null) orbitCam = cam.gameObject.AddComponent<ResultsCamera>();
        orbitCam.target = satellite.transform;
        orbitCam.distance = 12.5f;
        orbitCam.height = 2.6f;
        orbitCam.frameShift = 1.2f; // la nave queda en el hueco entre las tarjetas de la izquierda y el panel
        cam.transform.position = new Vector3(-3.2f, 1.6f, -6.8f);
        cam.transform.LookAt(new Vector3(2.2f, 0f, 1f));

        // ---------- Controlador y UI ----------
        ResultsController controller = new GameObject("ResultsController").AddComponent<ResultsController>();
        controller.satellite = satellite.transform;
        controller.solarPanels = wings.ToArray();
        controller.panelRenderers = panelRenderers.ToArray();
        controller.sun = sun;
        controller.planet = planet.transform;
        controller.planetRenderer = planet.GetComponent<Renderer>();
        controller.earthTexture = earth;
        controller.moonTexture = moon;
        BuildUi(controller);

        EditorUtility.SetDirty(cam.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[ResultsSceneBuilder] Escena Resultados armada.");
    }

    // ================= Satélite =================

    static GameObject Satellite(List<Transform> wings, List<Renderer> panelRenderers)
    {
        Material gold = Lit("Results_Foil", new Color(0.86f, 0.66f, 0.22f), 0.7f, 0.55f);
        Material metal = Lit("Results_Metal", new Color(0.72f, 0.75f, 0.80f), 0.6f, 0.45f);
        Material dark = Lit("Results_Dark", new Color(0.10f, 0.11f, 0.14f), 0.3f, 0.5f);
        Material white = Lit("Results_White", new Color(0.93f, 0.94f, 0.96f), 0f, 0.3f);
        Material cells = Lit("Results_SolarCells", new Color(0.09f, 0.20f, 0.55f), 0.4f, 0.8f);

        GameObject sat = new GameObject("Satellite");
        sat.transform.rotation = Quaternion.Euler(8f, 25f, -6f);
        Transform t = sat.transform;

        Part(PrimitiveType.Cube, "Body", t, new Vector3(0f, 0f, 0f), new Vector3(1.1f, 1.4f, 1.1f), gold);
        Part(PrimitiveType.Cylinder, "Ring", t, new Vector3(0f, -0.78f, 0f), new Vector3(0.95f, 0.08f, 0.95f), metal);
        Part(PrimitiveType.Cylinder, "Instrument", t, new Vector3(0f, -1.12f, 0f), new Vector3(0.55f, 0.28f, 0.55f), dark);
        Part(PrimitiveType.Cylinder, "Lens", t, new Vector3(0f, -1.42f, 0f), new Vector3(0.36f, 0.03f, 0.36f), cells);
        Part(PrimitiveType.Cylinder, "Mast", t, new Vector3(0f, 0.9f, 0f), new Vector3(0.08f, 0.2f, 0.08f), metal);
        Part(PrimitiveType.Sphere, "Dish", t, new Vector3(0f, 1.12f, 0f), new Vector3(1.0f, 0.22f, 1.0f), white);
        Part(PrimitiveType.Cylinder, "Feed", t, new Vector3(0f, 1.32f, 0f), new Vector3(0.05f, 0.14f, 0.05f), metal);

        for (int side = 0; side < 2; side++)
        {
            // El pivote está pegado al cuerpo: al escalarlo en X el panel se despliega hacia afuera.
            GameObject wing = new GameObject(side == 0 ? "WingRight" : "WingLeft");
            wing.transform.SetParent(t, false);
            wing.transform.localPosition = new Vector3(side == 0 ? 0.55f : -0.55f, 0.1f, 0f);
            wing.transform.localRotation = Quaternion.Euler(0f, side == 0 ? 0f : 180f, 0f);
            wings.Add(wing.transform);

            Part(PrimitiveType.Cube, "Arm", wing.transform, new Vector3(0.3f, 0f, 0f), new Vector3(0.6f, 0.06f, 0.06f), metal);
            for (int i = 0; i < 2; i++)
            {
                GameObject cell = Part(PrimitiveType.Cube, "Panel" + i, wing.transform,
                    new Vector3(1.35f + i * 1.6f, 0f, 0f), new Vector3(1.5f, 0.04f, 1.0f), cells);
                cell.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
                panelRenderers.Add(cell.GetComponent<Renderer>());
            }
        }
        return sat;
    }

    static GameObject Part(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    // ================= Materiales y malla =================

    static Material Asset(string name, string shader)
    {
        string path = GeneratedDir + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(m, path);
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material Lit(string name, Color color, float metallic, float smoothness)
    {
        Material m = Asset(name, "Universal Render Pipeline/Lit");
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Smoothness", smoothness);
        return m;
    }

    // Esfera con más polígonos que la de Unity, para que el horizonte del planeta se vea redondo.
    static Mesh SphereMesh()
    {
        string path = GeneratedDir + "/PlanetSphere.asset";
        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;

        const int lon = 96, lat = 48;
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        for (int y = 0; y <= lat; y++)
        {
            float v = y / (float)lat;
            float phi = v * Mathf.PI;
            for (int x = 0; x <= lon; x++)
            {
                float u = x / (float)lon;
                float theta = u * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta));
                vertices.Add(p);
                normals.Add(p);
                uvs.Add(new Vector2(1f - u, 1f - v));
            }
        }
        for (int y = 0; y < lat; y++)
        {
            for (int x = 0; x < lon; x++)
            {
                int a = y * (lon + 1) + x;
                int b = a + lon + 1;
                triangles.Add(a); triangles.Add(a + 1); triangles.Add(b);
                triangles.Add(a + 1); triangles.Add(b + 1); triangles.Add(b);
            }
        }

        Mesh mesh = new Mesh { name = "PlanetSphere" };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    // ================= UI =================

    static void BuildUi(ResultsController c)
    {
        LaunchHudBuilder.LoadAssets();
        Font bold = LaunchHudBuilder.fontBold, semi = LaunchHudBuilder.fontSemi, mono = LaunchHudBuilder.fontMono;
        Color green = LaunchHudBuilder.Green, orange = LaunchHudBuilder.Orange, white = LaunchHudBuilder.White;
        Color muted = LaunchHudBuilder.Muted;
        Color ink = new Color(0.08f, 0.06f, 0.04f);

        GameObject canvasGo = new GameObject("ResultsCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        Transform root = canvasGo.transform;

        // ---------- Misión (arriba a la izquierda) ----------
        RectTransform mission = LaunchHudBuilder.Card("MissionCard", root, new Vector2(0f, 1f), new Vector2(36f, -36f),
            new Vector2(460f, 108f), LaunchHudBuilder.Blue);
        LaunchHudBuilder.Label("Label", mission, semi, 15, muted, TextAnchor.UpperLeft, "MISSION REPORT", 28f, 0f, 0f, 14f);
        c.missionName = LaunchHudBuilder.Label("MissionName", mission, bold, 28, white, TextAnchor.UpperLeft, "Mission", 28f, 0f, 12f, 34f);
        c.missionDetail = LaunchHudBuilder.Label("MissionDetail", mission, semi, 17, muted, TextAnchor.LowerLeft, "Rocket / Orbit", 28f, 12f, 12f, 0f);

        // ---------- Evento de la simulación y dado de riesgo (izquierda) ----------
        RectTransform ev = LaunchHudBuilder.Card("EventCard", root, new Vector2(0f, 1f), new Vector2(36f, -164f),
            new Vector2(460f, 236f), LaunchHudBuilder.Blue);
        c.eventCard = ev.gameObject.AddComponent<CanvasGroup>();
        Txt("EventLabel", ev, semi, 15, muted, TextAnchor.UpperLeft, "LAUNCH EVENT", 28f, 18f, 410f, 20f);
        c.eventTitleText = Txt("EventTitle", ev, bold, 28, white, TextAnchor.UpperLeft, "", 28f, 42f, 410f, 36f);
        c.eventBodyText = Txt("EventBody", ev, semi, 20, muted, TextAnchor.UpperLeft, "", 28f, 82f, 410f, 100f);
        BestFit(c.eventBodyText, 15, 20);
        c.riskText = Txt("RiskRoll", ev, mono, 21, white, TextAnchor.UpperLeft, "", 28f, 192f, 410f, 28f);
        c.riskText.supportRichText = true;

        // ---------- Trade study (izquierda) ----------
        RectTransform trade = LaunchHudBuilder.Card("TradeCard", root, new Vector2(0f, 1f), new Vector2(36f, -420f),
            new Vector2(460f, 170f), green);
        c.tradeCard = trade.gameObject.AddComponent<CanvasGroup>();
        Txt("TradeLabel", trade, semi, 15, muted, TextAnchor.UpperLeft, "WITH A DIFFERENT CHOICE", 28f, 18f, 410f, 20f);
        c.tradeText = Txt("TradeText", trade, semi, 22, white, TextAnchor.UpperLeft, "", 28f, 46f, 410f, 106f);
        BestFit(c.tradeText, 16, 22);

        // ---------- CAPCOM (abajo a la izquierda) ----------
        RectTransform capcom = LaunchHudBuilder.Card("CapcomBar", root, new Vector2(0f, 0f), new Vector2(36f, 56f),
            new Vector2(1150f, 68f), Color.clear);
        c.capcomGroup = capcom.gameObject.AddComponent<CanvasGroup>();
        Image tag = LaunchHudBuilder.Img("Tag", capcom, LaunchHudBuilder.rounded, orange, true);
        LaunchHudBuilder.Anchor(tag.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(14f, 0f), new Vector2(126f, 40f));
        LaunchHudBuilder.Label("TagText", tag.transform, bold, 18, ink, TextAnchor.MiddleCenter, "CAPCOM", 0f, 0f, 0f, 0f);
        c.capcomText = LaunchHudBuilder.Label("CapcomText", capcom, semi, 26, white, TextAnchor.MiddleLeft, "", 160f, 0f, 24f, 0f);
        c.capcomText.horizontalOverflow = HorizontalWrapMode.Overflow;

        // ---------- Panel de resultados (a la derecha) ----------
        const float W = 640f, pad = 36f, inner = W - pad * 2f;
        RectTransform panel = LaunchHudBuilder.Card("ResultPanel", root, new Vector2(1f, 0.5f), new Vector2(-36f, 0f),
            new Vector2(W, 1008f), Color.clear);
        panel.Find("Fill").GetComponent<Image>().color = new Color(0.035f, 0.055f, 0.095f, 0.92f);
        c.panel = panel;

        // Resultado
        Image icon = LaunchHudBuilder.Img("ResultIcon", panel, LaunchHudBuilder.circle, green, false);
        Place(icon.rectTransform, pad, 32f, 68f, 68f);
        c.resultIcon = icon;
        c.resultIconText = LaunchHudBuilder.Label("IconText", icon.transform, bold, 26, ink, TextAnchor.MiddleCenter, "OK", 0f, 0f, 0f, 0f);
        c.resultTitle = Txt("ResultTitle", panel, bold, 42, green, TextAnchor.UpperLeft, "MISSION SUCCESS", 122f, 28f, 490f, 52f);
        c.resultHeadline = Txt("ResultHeadline", panel, semi, 20, muted, TextAnchor.UpperLeft, "", 122f, 80f, 490f, 28f);
        Divider(panel, 126f);

        // Ciencia
        Txt("ScienceLabel", panel, semi, 15, muted, TextAnchor.UpperLeft, "SCIENCE", pad, 144f, inner, 20f);
        c.scienceText = Txt("ScienceText", panel, mono, 84, white, TextAnchor.UpperLeft, "0", pad, 158f, inner, 96f);
        c.scienceText.supportRichText = true;
        Image track = LaunchHudBuilder.Img("ScienceTrack", panel, LaunchHudBuilder.rounded, new Color(1f, 1f, 1f, 0.12f), true);
        Place(track.rectTransform, pad, 262f, inner, 10f);
        Image fill = LaunchHudBuilder.Img("Fill", track.transform, LaunchHudBuilder.rounded, green, true);
        LaunchHudBuilder.Stretch(fill.rectTransform, 0f, 0f, 0f, 0f);
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        c.scienceFill = fill.rectTransform;
        c.scienceFillImage = fill;
        Image tick = LaunchHudBuilder.Img("GoalTick", track.transform, null, white, false);
        LaunchHudBuilder.Anchor(tick.rectTransform, new Vector2(0.66f, 0f), new Vector2(0.66f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(2f, 14f));
        c.goalTick = tick.rectTransform;
        Text goalLabel = LaunchHudBuilder.Label("GoalLabel", tick.transform, semi, 13, muted, TextAnchor.MiddleCenter, "GOAL", 0f, 0f, 0f, 0f);
        LaunchHudBuilder.Anchor(goalLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f),
            new Vector2(0f, -6f), new Vector2(80f, 18f));

        // Recursos
        string[] names = { "MASS", "POWER", "BUDGET", "DATA TO EARTH" };
        c.statValues = new Text[names.Length];
        c.statFills = new RectTransform[names.Length];
        c.statFillImages = new Image[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            float y = 312f + i * 50f;
            Txt("StatLabel" + i, panel, semi, 17, muted, TextAnchor.UpperLeft, names[i], pad, y, 220f, 24f);
            c.statValues[i] = Txt("StatValue" + i, panel, mono, 21, white, TextAnchor.UpperRight, "-", pad + 200f, y - 2f, inner - 200f, 26f);
            Image bar = LaunchHudBuilder.Img("StatTrack" + i, panel, LaunchHudBuilder.rounded, new Color(1f, 1f, 1f, 0.10f), true);
            Place(bar.rectTransform, pad, y + 30f, inner, 6f);
            Image barFill = LaunchHudBuilder.Img("Fill", bar.transform, LaunchHudBuilder.rounded, green, true);
            LaunchHudBuilder.Stretch(barFill.rectTransform, 0f, 0f, 0f, 0f);
            barFill.rectTransform.anchorMax = new Vector2(0f, 1f);
            barFill.gameObject.AddComponent<StatTarget>();
            c.statFills[i] = barFill.rectTransform;
            c.statFillImages[i] = barFill;
        }
        Divider(panel, 526f);

        // Por qué
        Txt("WhyLabel", panel, semi, 15, muted, TextAnchor.UpperLeft, "WHY IT WENT THIS WAY", pad, 542f, inner, 20f);
        c.whyText = Txt("WhyText", panel, semi, 20, white, TextAnchor.UpperLeft, "", pad, 568f, inner, 142f);
        BestFit(c.whyText, 15, 20);

        // Qué probar
        Image tryTag = LaunchHudBuilder.Img("TryTag", panel, LaunchHudBuilder.rounded, orange, true);
        Place(tryTag.rectTransform, pad, 722f, 116f, 30f);
        LaunchHudBuilder.Label("TryTagText", tryTag.transform, bold, 15, ink, TextAnchor.MiddleCenter, "TRY NEXT", 0f, 0f, 0f, 0f);
        c.tryNextText = Txt("TryNextText", panel, semi, 20, white, TextAnchor.UpperLeft, "", pad, 760f, inner, 58f);
        BestFit(c.tryNextText, 15, 20);
        Divider(panel, 828f);

        // Misión real parecida
        Txt("SimilarLabel", panel, semi, 15, muted, TextAnchor.UpperLeft, "SIMILAR REAL MISSION", pad, 840f, inner, 20f);
        c.similarName = Txt("SimilarName", panel, bold, 24, white, TextAnchor.UpperLeft, "", pad, 860f, inner, 32f);
        c.similarNote = Txt("SimilarNote", panel, semi, 16, muted, TextAnchor.UpperLeft, "", pad, 892f, inner, 24f);
        c.similarNote.horizontalOverflow = HorizontalWrapMode.Overflow;

        // Botones
        float half = (inner - 16f) * 0.5f;
        c.hubButton = MakeButton("HubButton", panel, pad, 934f, half, 56f, "SPACE CENTER", bold, orange, ink);
        c.redesignButton = MakeButton("RedesignButton", panel, pad + half + 16f, 934f, half, 56f, "REDESIGN", bold,
            new Color(0.12f, 0.16f, 0.23f, 1f), orange);

        // ---------- Fundido a negro, encima de todo ----------
        Image fade = LaunchHudBuilder.Img("Fade", root, null, Color.black, false);
        LaunchHudBuilder.Stretch(fade.rectTransform, 0f, 0f, 0f, 0f);
        c.fade = fade;
    }

    // Posición desde la esquina superior izquierda del padre.
    static void Place(RectTransform rt, float x, float top, float width, float height)
    {
        Vector2 topLeft = new Vector2(0f, 1f);
        LaunchHudBuilder.Anchor(rt, topLeft, topLeft, topLeft, new Vector2(x, -top), new Vector2(width, height));
    }

    static Text Txt(string name, Transform parent, Font font, int size, Color color, TextAnchor anchor, string text,
        float x, float top, float width, float height)
    {
        Text t = LaunchHudBuilder.Label(name, parent, font, size, color, anchor, text, 0f, 0f, 0f, 0f);
        Place(t.rectTransform, x, top, width, height);
        return t;
    }

    static void BestFit(Text t, int min, int max)
    {
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = min;
        t.resizeTextMaxSize = max;
        t.verticalOverflow = VerticalWrapMode.Truncate;
    }

    static void Divider(Transform parent, float top)
    {
        Image line = LaunchHudBuilder.Img("Divider", parent, null, new Color(1f, 1f, 1f, 0.14f), false);
        LaunchHudBuilder.Anchor(line.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -top), new Vector2(-72f, 1.5f));
    }

    static Button MakeButton(string name, Transform parent, float x, float top, float width, float height, string label,
        Font font, Color fillColor, Color textColor)
    {
        Image img = LaunchHudBuilder.Img(name, parent, LaunchHudBuilder.rounded, Color.white, true);
        Place(img.rectTransform, x, top, width, height);
        img.raycastTarget = true;
        Button button = img.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        ColorBlock colors = button.colors;
        colors.normalColor = fillColor;
        colors.highlightedColor = Color.Lerp(fillColor, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(fillColor, Color.black, 0.25f);
        colors.selectedColor = fillColor;
        button.colors = colors;
        LaunchHudBuilder.Label("Label", img.transform, font, 20, textColor, TextAnchor.MiddleCenter, label, 0f, 0f, 0f, 0f);
        return button;
    }
}
