using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Arma el HUD del lanzamiento: genera los sprites redondeados, usa las fuentes de Assets/UI/Fonts
// y deja todas las referencias de LaunchHud conectadas.
public static class LaunchHudBuilder
{
    const string SpriteDir = "Assets/UI/Launch";
    const string FontDir = "Assets/UI/Fonts/";

    public static readonly Color Green = new Color(0.48f, 0.85f, 0.56f);
    public static readonly Color Orange = new Color(0.94f, 0.47f, 0.16f);
    public static readonly Color Blue = new Color(0.35f, 0.64f, 0.94f);
    public static readonly Color White = new Color(0.93f, 0.96f, 1f);
    public static readonly Color Muted = new Color(0.56f, 0.64f, 0.72f);
    public static readonly Color PanelFill = new Color(0.035f, 0.055f, 0.095f, 0.84f);
    public static readonly Color PanelLine = new Color(1f, 1f, 1f, 0.16f);

    public static Sprite rounded, outline, shadow, circle, ring, gradient;
    public static Font fontBold, fontSemi, fontMono;

    public static LaunchHud Build()
    {
        LoadAssets();

        GameObject canvasGo = new GameObject("LaunchCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        LaunchHud hud = canvasGo.AddComponent<LaunchHud>();
        Transform root = canvasGo.transform;

        // Sombras arriba y abajo para que el HUD se lea sobre el cielo claro.
        Image topShade = Img("TopShade", root, gradient, new Color(0f, 0.02f, 0.06f, 0.55f), false);
        Anchor(topShade.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 280f));
        Image bottomShade = Img("BottomShade", root, gradient, new Color(0f, 0.02f, 0.06f, 0.7f), false);
        Anchor(bottomShade.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 340f));
        bottomShade.rectTransform.localScale = new Vector3(1f, -1f, 1f);
        bottomShade.rectTransform.pivot = new Vector2(0.5f, 1f);

        // ---------- Reloj de misión (arriba a la izquierda) ----------
        RectTransform clock = Card("ClockCard", root, new Vector2(0f, 1f), new Vector2(36f, -36f), new Vector2(300f, 108f), Green);
        Label("Label", clock, fontSemi, 15, Muted, TextAnchor.UpperLeft, "MISSION TIME", 28f, 0f, 0f, 14f);
        hud.clockText = Label("ClockText", clock, fontMono, 52, Green, TextAnchor.LowerLeft, "T- 00:03", 28f, 10f, 0f, 0f);

        // ---------- Altímetro (arriba al centro) ----------
        RectTransform alt = Card("AltitudeCard", root, new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(560f, 172f), Color.clear);
        Label("Label", alt, fontSemi, 15, Muted, TextAnchor.UpperCenter, "ALTITUDE", 0f, 0f, 0f, 14f);
        hud.altitudeText = Label("AltitudeText", alt, fontMono, 70, White, TextAnchor.UpperCenter, "000,000 m", 0f, 0f, 0f, 30f);
        hud.altitudeText.supportRichText = true;

        Image track = Img("Track", alt, rounded, new Color(1f, 1f, 1f, 0.12f), true);
        Anchor(track.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 46f), new Vector2(-88f, 8f));
        Image fill = Img("Fill", track.transform, rounded, Orange, true);
        Stretch(fill.rectTransform, 0f, 0f, 0f, 0f);
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        hud.altitudeFill = fill.rectTransform;
        hud.altitudeFillImage = fill;

        Image tick = Img("SpaceTick", track.transform, null, Muted, false);
        Anchor(tick.rectTransform, new Vector2(0.2f, 0f), new Vector2(0.2f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2f, 12f));
        hud.spaceTick = tick.rectTransform;
        Text tickLabel = Label("SpaceLabel", tick.transform, fontSemi, 13, Muted, TextAnchor.MiddleCenter, "SPACE 100 km", 0f, 0f, 0f, 0f);
        Anchor(tickLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(160f, 18f));

        hud.orbitLabel = Label("OrbitLabel", alt, fontSemi, 13, Muted, TextAnchor.LowerRight, "ORBIT 500 km", 0f, 16f, 44f, 0f);

        // ---------- Misión (arriba a la derecha) ----------
        RectTransform mission = Card("MissionCard", root, new Vector2(1f, 1f), new Vector2(-36f, -36f), new Vector2(460f, 108f), Blue);
        Label("Label", mission, fontSemi, 15, Muted, TextAnchor.UpperLeft, "MISSION", 28f, 0f, 0f, 14f);
        hud.missionName = Label("MissionName", mission, fontBold, 28, White, TextAnchor.UpperLeft, "Mission", 28f, 0f, 12f, 34f);
        hud.missionDetail = Label("MissionDetail", mission, fontSemi, 17, Muted, TextAnchor.LowerLeft, "Rocket / Orbit", 28f, 12f, 12f, 0f);

        // ---------- Etapas (a la izquierda) ----------
        const int rows = 4;
        RectTransform stages = Card("StageCard", root, new Vector2(0f, 1f), new Vector2(36f, -164f), new Vector2(400f, 74f + rows * 52f), Orange);
        hud.stagePanel = stages;
        Label("Label", stages, fontSemi, 15, Muted, TextAnchor.UpperLeft, "STAGES", 28f, 0f, 0f, 14f);
        hud.stageRows = new Text[rows];
        hud.stageDots = new Image[rows];
        hud.stageRings = new Image[rows];
        hud.stageLinks = new Image[rows];
        for (int i = 0; i < rows; i++)
        {
            float y = -(76f + i * 52f);
            Vector2 topLeft = new Vector2(0f, 1f);

            Image link = Img("Link" + i, stages, null, Muted, false);
            Anchor(link.rectTransform, topLeft, topLeft, new Vector2(0.5f, 1f), new Vector2(40f, y), new Vector2(2f, 52f));
            hud.stageLinks[i] = link;

            Image ringImg = Img("Ring" + i, stages, ring, Color.clear, false);
            Anchor(ringImg.rectTransform, topLeft, topLeft, new Vector2(0.5f, 0.5f), new Vector2(40f, y), new Vector2(18f, 18f));
            hud.stageRings[i] = ringImg;

            Image dot = Img("Dot" + i, stages, circle, Green, false);
            Anchor(dot.rectTransform, topLeft, topLeft, new Vector2(0.5f, 0.5f), new Vector2(40f, y), new Vector2(16f, 16f));
            hud.stageDots[i] = dot;

            Text row = Label("Row" + i, stages, fontSemi, 24, White, TextAnchor.MiddleLeft, "Stage", 0f, 0f, 0f, 0f);
            Anchor(row.rectTransform, topLeft, new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(66f, y), new Vector2(-80f, 44f));
            hud.stageRows[i] = row;
        }

        // ---------- CAPCOM (abajo al centro) ----------
        RectTransform status = Card("StatusBar", root, new Vector2(0.5f, 0f), new Vector2(0f, 56f), new Vector2(1180f, 68f), Color.clear);
        hud.statusGroup = status.gameObject.AddComponent<CanvasGroup>();
        hud.statusGroup.alpha = 0f;
        Image tag = Img("Tag", status, rounded, Orange, true);
        Anchor(tag.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(126f, 40f));
        Label("TagText", tag.transform, fontBold, 18, new Color(0.10f, 0.06f, 0.03f), TextAnchor.MiddleCenter, "CAPCOM", 0f, 0f, 0f, 0f);
        hud.statusText = Label("StatusText", status, fontSemi, 28, White, TextAnchor.MiddleLeft, "", 160f, 0f, 24f, 0f);
        hud.statusText.horizontalOverflow = HorizontalWrapMode.Overflow;

        // ---------- Cartel de evento ----------
        RectTransform card = Card("EventCard", root, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(940f, 236f), Color.clear);
        hud.eventCard = card;
        hud.eventGroup = card.gameObject.AddComponent<CanvasGroup>();
        card.Find("Fill").GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.07f, 0.95f);
        hud.eventBorder = card.Find("Border").GetComponent<Image>();
        hud.eventBorder.sprite = outline;

        Image icon = Img("Icon", card, circle, Orange, false);
        Anchor(icon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -26f), new Vector2(56f, 56f));
        hud.eventIcon = icon;
        Label("IconText", icon.transform, fontBold, 38, new Color(0.06f, 0.05f, 0.05f), TextAnchor.MiddleCenter, "!", 0f, 0f, 0f, 0f);

        hud.eventTitle = Label("EventTitle", card, fontBold, 40, Orange, TextAnchor.UpperLeft, "EVENT", 110f, 0f, 30f, 28f);
        Image divider = Img("Divider", card, null, new Color(1f, 1f, 1f, 0.14f), false);
        Anchor(divider.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(-68f, 1.5f));
        hud.eventBody = Label("EventBody", card, fontSemi, 27, White, TextAnchor.UpperLeft, "", 36f, 20f, 36f, 116f);
        card.gameObject.SetActive(false);

        // ---------- Botón Saltar (abajo a la derecha) ----------
        RectTransform skip = Card("SkipButton", root, new Vector2(1f, 0f), new Vector2(-36f, 56f), new Vector2(170f, 60f), Color.clear);
        Image skipFill = skip.Find("Fill").GetComponent<Image>();
        skipFill.color = Color.white;
        skipFill.raycastTarget = true;
        Button button = skip.gameObject.AddComponent<Button>();
        button.targetGraphic = skipFill;
        ColorBlock colors = button.colors;
        colors.normalColor = PanelFill;
        colors.highlightedColor = new Color(0.16f, 0.12f, 0.08f, 0.95f);
        colors.pressedColor = new Color(0.30f, 0.16f, 0.06f, 1f);
        colors.selectedColor = PanelFill;
        button.colors = colors;
        Label("SkipLabel", skip, fontBold, 22, Orange, TextAnchor.MiddleCenter, "SKIP  >>", 0f, 0f, 0f, 0f);
        hud.skipButton = button;

        // ---------- Fundido a negro, encima de todo ----------
        Image fade = Img("Fade", root, null, new Color(0f, 0f, 0f, 0f), false);
        Stretch(fade.rectTransform, 0f, 0f, 0f, 0f);
        hud.fade = fade;

        return hud;
    }

    // ================= Piezas de UI =================

    // Tarjeta redondeada con sombra, relleno, borde fino y una barra de color a la izquierda.
    // anchor = pivot, así que position es la distancia a esa esquina o borde.
    public static RectTransform Card(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color accent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        Anchor(rt, anchor, anchor, anchor, position, size);

        Image shade = Img("Shadow", rt, shadow, new Color(0f, 0f, 0f, 0.5f), true);
        Stretch(shade.rectTransform, -24f, -24f, -24f, -24f);

        Image fill = Img("Fill", rt, rounded, PanelFill, true);
        Stretch(fill.rectTransform, 0f, 0f, 0f, 0f);

        Image border = Img("Border", rt, outline, PanelLine, true);
        Stretch(border.rectTransform, 0f, 0f, 0f, 0f);

        if (accent.a > 0f)
        {
            Image bar = Img("Accent", rt, rounded, accent, true);
            Anchor(bar.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(5f, -32f));
        }
        return rt;
    }

    public static Image Img(string name, Transform parent, Sprite sprite, Color color, bool sliced)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        if (sliced)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 2f; // los sprites están al doble de resolución
        }
        return img;
    }

    // Texto que ocupa el padre, con márgenes (izquierda, abajo, derecha, arriba).
    public static Text Label(string name, Transform parent, Font font, int size, Color color, TextAnchor anchor, string text,
        float left, float bottom, float right, float top)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>(), left, bottom, right, top);
        Text t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.supportRichText = false;
        return t;
    }

    public static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    public static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    // ================= Fuentes y sprites =================

    public static void LoadAssets()
    {
        Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        fontBold = AssetDatabase.LoadAssetAtPath<Font>(FontDir + "ChakraPetch-Bold.ttf") ?? fallback;
        fontSemi = AssetDatabase.LoadAssetAtPath<Font>(FontDir + "ChakraPetch-SemiBold.ttf") ?? fallback;
        fontMono = AssetDatabase.LoadAssetAtPath<Font>(FontDir + "ShareTechMono-Regular.ttf") ?? fallback;

        if (!AssetDatabase.IsValidFolder(SpriteDir)) AssetDatabase.CreateFolder("Assets/UI", "Launch");

        // Rectángulo redondeado relleno, su contorno, y una sombra difusa. Los tres son 9-slice.
        rounded = MakeSprite("rounded", 128, 128, 48, (x, y) => Mathf.Clamp01(0.5f - RoundedBox(x, y, 128f, 0f, 36f)));
        outline = MakeSprite("outline", 128, 128, 48, (x, y) =>
        {
            float d = RoundedBox(x, y, 128f, 0f, 36f);
            return Mathf.Clamp01(0.5f - (Mathf.Abs(d + 1.5f) - 1.5f));
        });
        shadow = MakeSprite("shadow", 192, 192, 90, (x, y) =>
        {
            float d = RoundedBox(x, y, 192f, 48f, 36f);
            return Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(d / 46f));
        });
        circle = MakeSprite("circle", 64, 64, 0, (x, y) => Mathf.Clamp01(0.5f - (Dist(x, y, 32f) - 30f)));
        ring = MakeSprite("ring", 64, 64, 0, (x, y) => Mathf.Clamp01(0.5f - (Mathf.Abs(Dist(x, y, 32f) - 27f) - 3f)));
        gradient = MakeSprite("gradient", 4, 128, 0, (x, y) => Mathf.Pow(y / 127f, 1.6f));
    }

    static float Dist(float x, float y, float center)
    {
        float dx = x + 0.5f - center, dy = y + 0.5f - center;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    // Distancia con signo a un cuadrado redondeado centrado (negativa adentro).
    static float RoundedBox(float x, float y, float size, float inset, float radius)
    {
        float half = size * 0.5f - inset;
        float px = Mathf.Abs(x + 0.5f - size * 0.5f) - (half - radius);
        float py = Mathf.Abs(y + 0.5f - size * 0.5f) - (half - radius);
        float outside = new Vector2(Mathf.Max(px, 0f), Mathf.Max(py, 0f)).magnitude;
        float inside = Mathf.Min(Mathf.Max(px, py), 0f);
        return outside + inside - radius;
    }

    static Sprite MakeSprite(string name, int width, int height, int border, Func<float, float, float> alpha)
    {
        string path = SpriteDir + "/" + name + ".png";

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                pixels[y * width + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha(x, y) * 255f));
        tex.SetPixels32(pixels);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteBorder = new Vector4(border, border, border, border);
        settings.spritePixelsPerUnit = 100f;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
