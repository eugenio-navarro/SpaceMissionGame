using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Utilidades para armar escenas por código (modelos de Kenney, materiales, primitivas, UI y capturas).
public static class BuilderUtil
{
    public const string SpaceKit = "Assets/Art/kenney_space-kit/Models/FBX format/";
    public const string CityKit = "Assets/Art/kenney_city-kit-industrial_2.0/Models/FBX format/";
    const string MaterialFolder = "Assets/Art/Materials";

    // Instancia un modelo con el pivote corregido: centrado en X/Z y apoyado en Y = 0.
    public static GameObject Model(string name, string path, Transform parent, Vector3 position, float scale = 1f, float rotationY = 0f)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path + ".fbx");
        if (prefab == null) throw new System.Exception("Model not found: " + path);

        var wrapper = new GameObject(name);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, wrapper.transform);
        var b = WorldBounds(wrapper);
        instance.transform.localPosition -= new Vector3(b.center.x, b.min.y, b.center.z);

        wrapper.transform.SetParent(parent, false);
        wrapper.transform.localPosition = position;
        wrapper.transform.localRotation = Quaternion.Euler(0f, rotationY, 0f);
        wrapper.transform.localScale = Vector3.one * scale;
        return wrapper;
    }

    public static Bounds WorldBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    public static float Height(GameObject go) => WorldBounds(go).size.y;

    public static BoxCollider FitCollider(GameObject go)
    {
        var b = WorldBounds(go);
        var col = go.AddComponent<BoxCollider>();
        var s = go.transform.lossyScale;
        col.center = go.transform.InverseTransformPoint(b.center);
        col.size = new Vector3(b.size.x / s.x, b.size.y / s.y, b.size.z / s.z);
        return col;
    }

    public static Material Mat(string name, Color color, float metallic = 0f, float smoothness = 0.3f, Color? emission = null)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder("Assets/Art", "Materials");
        string path = $"{MaterialFolder}/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Smoothness", smoothness);
        if (emission.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission.Value);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    public static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material,
        Vector3? euler = null, bool keepCollider = false)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero);
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    public static Transform Group(string name, Transform parent, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        return go.transform;
    }

    public static Light AddLight(string name, LightType type, Vector3 position, Vector3 euler, float intensity, Color color,
        float range = 10f, float spotAngle = 30f, LightShadows shadows = LightShadows.Soft)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(euler);
        var l = go.AddComponent<Light>();
        l.type = type;
        l.intensity = intensity;
        l.color = color;
        l.range = range;
        l.spotAngle = spotAngle;
        l.shadows = shadows;
        return l;
    }

    // Canvas 1920x1080 escalable + EventSystem con el Input System nuevo.
    public static Canvas CreateCanvas()
    {
        var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.layer = 5;
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var style = go.AddComponent<UIStyle>();
        style.titleFont = FindFont("ChakraPetch-Bold");
        style.bodyFont = FindFont("ChakraPetch-SemiBold");
        style.monoFont = FindFont("ShareTechMono");
        style.roundedFill = RoundedSprite("RoundedFill", 64, 32f, false, 0f, 20);
        style.roundedOutline = RoundedSprite("RoundedOutline", 64, 32f, true, 0f, 20);
        style.softShadow = RoundedSprite("SoftShadow", 128, 32f, false, 14f, 52);
        style.Apply();

        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        return canvas;
    }

    // ---------- Estilo: fuentes y sprites redondeados ----------

    const string FontFolder = "Assets/UI/Fonts";
    const string SpriteFolder = "Assets/UI/HubHangar";
    const float CornerRadius = 18f;

    // Busca la fuente en Assets/UI/Fonts ignorando guiones y espacios. Si no está, null (se usa la fuente por defecto).
    static Font FindFont(string name)
    {
        if (!AssetDatabase.IsValidFolder(FontFolder)) return null;
        string key = name.Replace("-", "").Replace(" ", "").ToLowerInvariant();
        foreach (var guid in AssetDatabase.FindAssets("t:Font", new[] { FontFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string file = Path.GetFileNameWithoutExtension(path).Replace("-", "").Replace(" ", "").Replace("_", "").ToLowerInvariant();
            if (file == key || file == key + "regular") return AssetDatabase.LoadAssetAtPath<Font>(path);
        }
        return null;
    }

    // Genera (una sola vez) un sprite de rectángulo redondeado: relleno, contorno de 1,5 px o sombra difusa.
    static Sprite RoundedSprite(string name, int size, float inset, bool outline, float blur, int border)
    {
        string path = $"{SpriteFolder}/{name}.png";
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(SpriteFolder);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float half = size / 2f;
            float boxHalf = half - (blur > 0f ? inset : 0f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x + 0.5f - half, y + 0.5f - half);
                    var q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - Vector2.one * (boxHalf - CornerRadius);
                    float d = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - CornerRadius;
                    float a;
                    if (blur > 0f) { a = Mathf.Clamp01(0.5f - d / (2f * blur)); a *= a; }
                    else if (outline) a = Mathf.Clamp01(1.25f - Mathf.Abs(d + 0.75f));
                    else a = Mathf.Clamp01(0.5f - d);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var b = new Vector4(border, border, border, border);
        if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != b)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = b;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // Captura la cámara principal incluyendo la UI (pasa los Canvas a modo cámara por un momento).
    public static string Capture(string outPath, int width = 1600, int height = 900)
    {
        var cam = Camera.main;
        if (cam == null) return "no camera";
        var canvases = Object.FindObjectsByType<Canvas>()
            .Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToList();
        foreach (var c in canvases)
        {
            c.renderMode = RenderMode.ScreenSpaceCamera;
            c.worldCamera = cam;
            c.planeDistance = cam.nearClipPlane + 0.05f;
        }

        var rt = new RenderTexture(width, height, 24);
        var previous = cam.targetTexture;
        cam.targetTexture = rt;
        Canvas.ForceUpdateCanvases();
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        cam.targetTexture = previous;
        RenderTexture.active = null;

        foreach (var c in canvases)
        {
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.worldCamera = null;
        }

        File.WriteAllBytes(outPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        rt.Release();
        Object.DestroyImmediate(rt);
        return outPath;
    }
}
