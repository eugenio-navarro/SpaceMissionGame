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
        GameObject satellite = DesignSatellite(wings, panelRenderers, out SatelliteVisual partsVisual);

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
        controller.parts = partsVisual;
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

    // El mismo satélite del Hangar (HangarSceneBuilder.BuildSatellite): en Play muestra solo las piezas
    // que eligió el jugador. Las alas (paneles chico y grande) se despliegan escalándolas en X.
    static GameObject DesignSatellite(List<Transform> wings, List<Renderer> panelRenderers, out SatelliteVisual visual)
    {
        GameObject sat = new GameObject("Satellite");
        sat.transform.rotation = Quaternion.Euler(8f, 25f, -6f);
        visual = sat.AddComponent<SatelliteVisual>();
        var parts = new GameObject("Parts").transform;
        parts.SetParent(sat.transform, false);
        parts.localPosition = new Vector3(0f, -0.85f, 0f); // el cuerpo queda centrado en el satélite
        parts.localScale = Vector3.one * 1.3f;
        visual.satellite = parts;
        HangarSceneBuilder.BuildSatellite(visual);

        foreach (var entry in visual.parts)
        {
            if (!entry.id.StartsWith("panel_")) continue;
            wings.Add(entry.root.transform);
            foreach (Renderer r in entry.root.GetComponentsInChildren<Renderer>())
                if (r.name == "Cells") panelRenderers.Add(r);
        }
        return sat;
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
        BestFit(c.eventTitleText, 16, 28); // "GROUND STATION INTERFERENCE" entra en una línea
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
        BestFit(c.capcomText, 16, 26);
        // Se estira hasta el panel de la derecha: en 4:3 o 16:10 no se encima con él.
        capcom.anchorMin = Vector2.zero;
        capcom.anchorMax = new Vector2(1f, 0f);
        capcom.pivot = Vector2.zero;
        capcom.offsetMin = new Vector2(36f, 56f);
        capcom.offsetMax = new Vector2(-(36f + 640f + 24f), 124f);

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
        c.hubButton = MakeButton("HubButton", panel, pad, 934f, half, 56f, "<  SPACE CENTER", bold, orange);
        c.redesignButton = MakeButton("RedesignButton", panel, pad + half + 16f, 934f, half, 56f, "REDESIGN", bold, orange);

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

    // Botón = tarjeta con texto naranja en negrita (igual que en el resto del juego):
    // al pasar el mouse el borde se pone naranja y hace un rebote chico (CardButtonFx).
    static Button MakeButton(string name, Transform parent, float x, float top, float width, float height, string label,
        Font font, Color textColor)
    {
        Image img = LaunchHudBuilder.Img(name, parent, LaunchHudBuilder.rounded, new Color(0.12f, 0.16f, 0.23f, 1f), true);
        Place(img.rectTransform, x, top, width, height);
        img.raycastTarget = true;
        Image border = LaunchHudBuilder.Img("Border", img.transform, LaunchHudBuilder.outline, LaunchHudBuilder.PanelLine, true);
        LaunchHudBuilder.Stretch(border.rectTransform, 0f, 0f, 0f, 0f);

        Button button = img.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        button.transition = Selectable.Transition.None;
        Text text = LaunchHudBuilder.Label("Label", img.transform, font, 22, textColor, TextAnchor.MiddleCenter, label, 0f, 0f, 0f, 0f);

        CardButtonFx fx = img.gameObject.AddComponent<CardButtonFx>();
        fx.button = button;
        fx.border = border;
        fx.label = text;
        fx.labelColor = textColor;
        fx.borderColor = LaunchHudBuilder.PanelLine;
        return button;
    }
}
