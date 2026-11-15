using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Arma la escena Lanzamiento desde cero: cohete, plataforma, partículas, cámara y HUD.
// Menú: Space Mission > Build Lanzamiento Scene. Se puede volver a correr: borra lo anterior y lo rehace.
public static class LaunchSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Lanzamiento.unity";
    const string ModelDir = "Assets/Art/kenney_space-kit/Models/FBX format/";
    const string ParticleDir = "Assets/Art/kenney_particle-pack/PNG (Transparent)/";
    const string GeneratedDir = "Assets/Art/Launch";

    const float RocketHeight = 8f;

    static readonly Color Green = new Color(0.48f, 0.85f, 0.56f);
    static readonly Color Orange = new Color(0.94f, 0.47f, 0.16f);
    static readonly Color PanelColor = new Color(0.07f, 0.09f, 0.12f, 1f);

    [MenuItem("Space Mission/Build Lanzamiento Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Limpia lo generado antes (deja la cámara y la luz originales).
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "Main Camera" || root.name == "Directional Light") continue;
            Object.DestroyImmediate(root);
        }

        if (!AssetDatabase.IsValidFolder(GeneratedDir)) AssetDatabase.CreateFolder("Assets/Art", "Launch");

        Material groundMat = LitMaterial("Launch_Ground", new Color(0.24f, 0.36f, 0.22f));
        Material flameMat = ParticleMaterial("Launch_Flame", "circle_05.png", false);
        Material smokeMat = ParticleMaterial("Launch_Smoke", "smoke_07.png", false);
        Material starMat = ParticleMaterial("Launch_Star", "star_01.png", true);

        // ---------- Mundo ----------
        GameObject world = new GameObject("World");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(world.transform);
        ground.transform.localScale = new Vector3(200f, 1f, 200f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;
        Object.DestroyImmediate(ground.GetComponent<Collider>());

        GameObject pad = Model("platform_large", world.transform);
        if (pad != null)
        {
            pad.name = "LaunchPad";
            FitWidth(pad, 10f);
            PlaceBottomAt(pad, 0f);
        }
        float padTop = pad != null ? WorldBounds(pad).max.y : 0f;

        // ---------- Cohete ----------
        GameObject rocket = new GameObject("Rocket");
        GameObject stage1 = new GameObject("Stage1");
        GameObject stage2 = new GameObject("Stage2");
        stage1.transform.SetParent(rocket.transform);
        stage2.transform.SetParent(rocket.transform);

        float y = 0f;
        y = Stack("rocket_baseA", stage1.transform, y);
        y = Stack("rocket_fuelB", stage1.transform, y);
        float stage2Bottom = y;
        y = Stack("rocket_sidesA", stage2.transform, y);
        y = Stack("rocket_fuelA", stage2.transform, y);
        y = Stack("rocket_topA", stage2.transform, y);

        float scale = y > 0.001f ? RocketHeight / y : 1f;
        rocket.transform.localScale = Vector3.one * scale;
        rocket.transform.position = new Vector3(0f, padTop, 0f);
        stage2Bottom *= scale;

        // ---------- Partículas ----------
        ParticleSystem stage1Flame = Flame("Stage1Flame", stage1.transform, flameMat, 4.2f);
        stage1Flame.transform.position = rocket.transform.position + Vector3.up * 0.4f;

        ParticleSystem stage1Smoke = Smoke("Stage1Smoke", stage1.transform, smokeMat);
        stage1Smoke.transform.position = rocket.transform.position + Vector3.up * 0.2f;

        ParticleSystem stage2Flame = Flame("Stage2Flame", stage2.transform, flameMat, 2.8f);
        stage2Flame.transform.position = rocket.transform.position + Vector3.up * stage2Bottom;

        ParticleSystem padSmoke = PadSmoke("PadSmoke", world.transform, smokeMat);
        padSmoke.transform.position = new Vector3(0f, padTop + 0.3f, 0f);

        GameObject starsGo = new GameObject("Stars");
        ParticleSystem stars = Stars(starsGo, starMat);

        // ---------- Cámara ----------
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.70f, 0.95f);
        cam.fieldOfView = 55f;
        cam.farClipPlane = 3000f;

        LaunchCamera launchCam = cam.GetComponent<LaunchCamera>();
        if (launchCam == null) launchCam = cam.gameObject.AddComponent<LaunchCamera>();
        launchCam.target = rocket.transform;
        launchCam.lookHeight = RocketHeight * 0.5f;
        launchCam.offset = new Vector3(7f, 1.5f, -12f);
        launchCam.positionFollower = starsGo.transform;
        launchCam.minY = padTop + 1f;
        cam.transform.position = rocket.transform.position + launchCam.offset;
        cam.transform.LookAt(rocket.transform.position + Vector3.up * launchCam.lookHeight);

        // ---------- HUD ----------
        LaunchHud hud = LaunchHudBuilder.Build();

        // ---------- Secuencia ----------
        GameObject director = new GameObject("LaunchSequence");
        LaunchSequence seq = director.AddComponent<LaunchSequence>();
        seq.rocket = rocket.transform;
        seq.stage1 = stage1.transform;
        seq.stage1Flame = stage1Flame;
        seq.stage1Smoke = stage1Smoke;
        seq.stage2Flame = stage2Flame;
        seq.padSmoke = padSmoke;
        seq.launchCamera = launchCam;
        seq.cam = cam;
        seq.hud = hud;
        seq.world = world;
        seq.starsRenderer = stars.GetComponent<ParticleSystemRenderer>();

        EditorUtility.SetDirty(cam.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[LaunchSceneBuilder] Escena Lanzamiento armada. Altura del cohete sin escalar: " + y);
    }

    // ================= Modelos =================

    static GameObject Model(string name, Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelDir + name + ".fbx");
        if (prefab == null)
        {
            Debug.LogWarning("[LaunchSceneBuilder] No se encontró el modelo " + name);
            return null;
        }
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.SetParent(parent, false);
        return go;
    }

    // Apila una pieza con su base en bottomY y devuelve la altura de su tope.
    static float Stack(string name, Transform parent, float bottomY)
    {
        GameObject go = Model(name, parent);
        if (go == null) return bottomY;
        PlaceBottomAt(go, bottomY);
        return WorldBounds(go).max.y;
    }

    static Bounds WorldBounds(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    static void PlaceBottomAt(GameObject go, float bottomY)
    {
        Bounds b = WorldBounds(go);
        go.transform.position += new Vector3(-b.center.x, bottomY - b.min.y, -b.center.z);
    }

    static void FitWidth(GameObject go, float width)
    {
        Bounds b = WorldBounds(go);
        float w = Mathf.Max(b.size.x, b.size.z);
        if (w > 0.0001f) go.transform.localScale *= width / w;
    }

    // ================= Materiales =================

    static Material LitMaterial(string name, Color color)
    {
        string path = GeneratedDir + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Smoothness", 0.05f);
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material ParticleMaterial(string name, string texture, bool additive)
    {
        string path = GeneratedDir + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            AssetDatabase.CreateAsset(m, path);
        }

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ParticleDir + texture);
        if (tex == null) Debug.LogWarning("[LaunchSceneBuilder] No se encontró la textura " + texture);
        m.SetTexture("_BaseMap", tex);
        m.SetColor("_BaseColor", Color.white);

        // Transparente; aditivo para fuego y estrellas, alpha normal para humo.
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", additive ? 2f : 0f);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", additive
            ? (float)UnityEngine.Rendering.BlendMode.One
            : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (m.HasProperty("_SrcBlendAlpha")) m.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
        if (m.HasProperty("_DstBlendAlpha"))
            m.SetFloat("_DstBlendAlpha", additive
                ? (float)UnityEngine.Rendering.BlendMode.One
                : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.SetOverrideTag("RenderType", "Transparent");
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        EditorUtility.SetDirty(m);
        return m;
    }

    // ================= Partículas =================

    static ParticleSystem NewSystem(string name, Transform parent, Material mat)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
        return ps;
    }

    static Gradient Fade(Color from, Color to, float startAlpha)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(from, 0f), new GradientColorKey(to, 1f) },
            new[] { new GradientAlphaKey(startAlpha, 0f), new GradientAlphaKey(startAlpha, 0.3f), new GradientAlphaKey(0f, 1f) });
        return g;
    }

    static ParticleSystem Flame(string name, Transform parent, Material mat, float size)
    {
        ParticleSystem ps = NewSystem(name, parent, mat);
        ps.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // el cono apunta hacia abajo

        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 11f;
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.7f, size * 1.2f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 300;

        var emission = ps.emission;
        emission.rateOverTime = 140f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 6f;
        shape.radius = size * 0.06f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = new ParticleSystem.MinMaxGradient(
            Fade(new Color(1f, 0.92f, 0.45f), new Color(1f, 0.30f, 0.02f), 1f));

        var sizeOver = ps.sizeOverLifetime;
        sizeOver.enabled = true;
        sizeOver.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));
        return ps;
    }

    static ParticleSystem Smoke(string name, Transform parent, Material mat)
    {
        ParticleSystem ps = NewSystem(name, parent, mat);
        ps.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(2f, 3.5f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World; // deja la estela atrás
        main.maxParticles = 400;

        var emission = ps.emission;
        emission.rateOverTime = 45f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 14f;
        shape.radius = 0.4f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = new ParticleSystem.MinMaxGradient(
            Fade(new Color(0.95f, 0.93f, 0.9f), new Color(0.6f, 0.6f, 0.62f), 0.55f));

        var sizeOver = ps.sizeOverLifetime;
        sizeOver.enabled = true;
        sizeOver.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 2.4f));
        return ps;
    }

    // Nube que se abre hacia los costados sobre la plataforma al encender.
    static ParticleSystem PadSmoke(string name, Transform parent, Material mat)
    {
        ParticleSystem ps = NewSystem(name, parent, mat);
        ps.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // el círculo queda horizontal

        var main = ps.main;
        main.loop = false;
        main.duration = 3.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(3f, 5f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 400;

        var emission = ps.emission;
        emission.rateOverTime = 70f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1.5f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = new ParticleSystem.MinMaxGradient(
            Fade(new Color(0.97f, 0.95f, 0.92f), new Color(0.7f, 0.7f, 0.72f), 0.6f));

        var sizeOver = ps.sizeOverLifetime;
        sizeOver.enabled = true;
        sizeOver.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.5f, 1f, 2.2f));
        return ps;
    }

    static ParticleSystem Stars(GameObject go, Material mat)
    {
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;

        var main = ps.main;
        main.playOnAwake = true;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = 100000f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 700;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 600) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 250f;
        shape.radiusThickness = 0f;
        return ps;
    }
}
