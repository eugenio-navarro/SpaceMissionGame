using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static BuilderUtil;

// Arma la escena Hangar desde cero. Ojo: borra lo que haya en la escena.
// Usarlo para el armado inicial; si después se edita a mano, no volver a correrlo.
public static class HangarSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Hangar.unity";
    const float RocketScale = 1.4f;
    const float PadTop = 0.25f;

    [MenuItem("Tools/Escenas/Construir Hangar")]
    public static string Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.32f, 0.34f, 0.38f);

        var controller = new GameObject("HangarController").AddComponent<HangarController>();
        BuildInterior();
        var visual = BuildShip(controller);
        BuildCamera(visual.satellite);
        BuildUI(controller);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Hangar built";
    }

    // ---------- Interior del hangar ----------

    static void BuildInterior()
    {
        var env = new GameObject("Interior").transform;
        var floor = Mat("HangarFloor", Palette.Hex("#4A4F57"), 0f, 0.45f);
        var wall = Mat("HangarWall", Palette.Hex("#5A626C"), 0.3f, 0.3f);
        var ceiling = Mat("HangarCeiling", Palette.Hex("#2E333B"), 0.2f, 0.2f);
        var door = Mat("HangarDoor", Palette.Hex("#C9A227"), 0.4f, 0.4f);
        var stripe = Mat("FloorStripe", Palette.Yellow, 0f, 0.3f);
        var lamp = Mat("Lamp", Color.white, 0f, 0.5f);

        Prim(PrimitiveType.Plane, "Floor", env, Vector3.zero, new Vector3(5f, 1f, 5f), floor);
        Prim(PrimitiveType.Cube, "Wall_Back", env, new Vector3(0f, 10f, 22f), new Vector3(44f, 20f, 0.6f), wall);
        Prim(PrimitiveType.Cube, "Wall_Front", env, new Vector3(0f, 10f, -22f), new Vector3(44f, 20f, 0.6f), wall);
        Prim(PrimitiveType.Cube, "Wall_Left", env, new Vector3(-22f, 10f, 0f), new Vector3(0.6f, 20f, 44f), wall);
        Prim(PrimitiveType.Cube, "Wall_Right", env, new Vector3(22f, 10f, 0f), new Vector3(0.6f, 20f, 44f), wall);
        Prim(PrimitiveType.Cube, "Ceiling", env, new Vector3(0f, 20f, 0f), new Vector3(44f, 0.6f, 44f), ceiling);

        // Portón amarillo al fondo, como en el moodboard
        Prim(PrimitiveType.Cube, "Door", env, new Vector3(0f, 7f, 21.6f), new Vector3(12f, 14f, 0.3f), door);
        for (int i = 0; i < 6; i++)
            Prim(PrimitiveType.Cube, $"DoorRib_{i}", env, new Vector3(0f, 1.2f + i * 2.3f, 21.4f), new Vector3(12.2f, 0.15f, 0.2f), wall);

        // Franjas del piso alrededor de la plataforma
        Prim(PrimitiveType.Cube, "Stripe_N", env, new Vector3(0f, 0.01f, 5f), new Vector3(10.3f, 0.02f, 0.3f), stripe);
        Prim(PrimitiveType.Cube, "Stripe_S", env, new Vector3(0f, 0.01f, -5f), new Vector3(10.3f, 0.02f, 0.3f), stripe);
        Prim(PrimitiveType.Cube, "Stripe_E", env, new Vector3(5f, 0.01f, 0f), new Vector3(0.3f, 0.02f, 10f), stripe);
        Prim(PrimitiveType.Cube, "Stripe_W", env, new Vector3(-5f, 0.01f, 0f), new Vector3(0.3f, 0.02f, 10f), stripe);

        Model("Platform", SpaceKit + "platform_large", env, Vector3.zero, 2.5f);

        // Andamios a los costados del cohete
        foreach (float x in new[] { -3.4f, 3.4f })
            for (int k = 0; k < 8; k++)
                Model($"Scaffold_{x}_{k}", SpaceKit + "structure", env, new Vector3(x, k * 1.2f, 1.2f), 1.2f);

        // Decorado
        Model("Barrels", SpaceKit + "barrels", env, new Vector3(-10f, 0f, 9f), 3f, 20f);
        Model("Generator", SpaceKit + "machine_generatorLarge", env, new Vector3(-11f, 0f, -6f), 3f, 90f);
        Model("Container", CityKit + "shipping-container-b", env, new Vector3(11f, 0f, 9f), 4f, 20f);
        Model("Crates", CityKit + "shipping-container-a", env, new Vector3(12f, 0f, -7f), 3.5f, -10f);

        // Luces: lámparas en el techo y focos sobre la nave
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                Prim(PrimitiveType.Cube, $"Lamp_{i}_{j}", env, new Vector3(i * 12f, 19.6f, j * 12f), new Vector3(4f, 0.2f, 1f), lamp);

        AddLight("Fill", LightType.Directional, new Vector3(0f, 18f, 0f), new Vector3(70f, 30f, 0f), 0.45f, new Color(0.9f, 0.93f, 1f));
        var target = new Vector3(0f, 4f, 0f);
        foreach (var pos in new[] { new Vector3(0f, 17f, -10f), new Vector3(-10f, 17f, 6f), new Vector3(10f, 17f, 6f) })
        {
            var l = AddLight("Spot", LightType.Spot, pos, Vector3.zero, 80f, new Color(1f, 0.95f, 0.85f), 40f, 55f);
            l.transform.LookAt(target);
        }
        AddLight("RoomLight", LightType.Point, new Vector3(0f, 14f, 0f), Vector3.zero, 25f, new Color(1f, 0.96f, 0.9f), 35f, 30f, LightShadows.None);
    }

    // ---------- Cohetes y satélite ----------

    static SatelliteVisual BuildShip(HangarController controller)
    {
        var ship = new GameObject("Ship").transform;
        var visual = ship.gameObject.AddComponent<SatelliteVisual>();
        visual.controller = controller;

        visual.noRocketTop = Group("NoRocketTop", ship, new Vector3(0f, PadTop, 0f));
        visual.rockets.Add(BuildRocket(ship, "chico", 2, false));
        visual.rockets.Add(BuildRocket(ship, "mediano", 4, false));
        visual.rockets.Add(BuildRocket(ship, "grande", 5, true));

        visual.satellite = Group("Satellite", ship, new Vector3(0f, PadTop, 0f));
        BuildSatellite(visual);
        return visual;
    }

    static SatelliteVisual.Entry BuildRocket(Transform ship, string id, int fuelSegments, bool boosters)
    {
        var root = Group("Rocket_" + id, ship, new Vector3(0f, PadTop, 0f));
        float y = Stack(root, Vector3.zero, RocketScale, fuelSegments, null);
        if (id != "chico") Model("Fins", SpaceKit + "rocket_finsA", root, Vector3.zero, RocketScale);
        if (boosters)
            foreach (float x in new[] { -1.55f, 1.55f })
                Stack(root, new Vector3(x, 0f, 0f), 0.75f, 3, "rocket_topB");

        var top = Group("Top", root, new Vector3(0f, y, 0f));
        return new SatelliteVisual.Entry { id = id, root = root.gameObject, top = top };
    }

    // Apila base + tramos de combustible (+ punta opcional). Devuelve la altura final.
    static float Stack(Transform parent, Vector3 origin, float scale, int fuelSegments, string tip)
    {
        var parts = new List<string> { "rocket_baseA" };
        for (int i = 0; i < fuelSegments; i++) parts.Add("rocket_fuelA");
        if (tip != null) parts.Add(tip);

        float y = 0f;
        foreach (var part in parts)
        {
            var piece = Model(part, SpaceKit + part, parent, origin + new Vector3(0f, y, 0f), scale);
            y += Height(piece);
        }
        return y;
    }

    static void BuildSatellite(SatelliteVisual visual)
    {
        var metal = Mat("SatMetal", Palette.Metal, 0.8f, 0.6f);
        var gold = Mat("SatGold", Palette.Hex("#C9A227"), 0.9f, 0.7f);
        var dark = Mat("SatDark", Palette.Hex("#23272E"), 0.3f, 0.5f);
        var white = Mat("SatWhite", Palette.Hex("#D9DDE2"), 0.1f, 0.4f);
        var glass = Mat("SatLens", Palette.Hex("#4FA3E0"), 0.2f, 0.95f);
        var cells = Mat("SolarCells", Palette.Hex("#1F3F7A"), 0.6f, 0.85f);
        var orange = Mat("SatOrange", Palette.Orange, 0f, 0.4f);

        var body = Group("Body", visual.satellite, new Vector3(0f, 0.85f, 0f));
        Prim(PrimitiveType.Cube, "Core", body, Vector3.zero, new Vector3(1.1f, 1.3f, 1.1f), gold);
        Prim(PrimitiveType.Cube, "TopPlate", body, new Vector3(0f, 0.68f, 0f), new Vector3(1.2f, 0.06f, 1.2f), metal);
        Prim(PrimitiveType.Cube, "BottomPlate", body, new Vector3(0f, -0.68f, 0f), new Vector3(1.2f, 0.06f, 1.2f), metal);

        // Instrumentos (cada uno en su punto fijo, mirando hacia -Z)
        var camB = Part(visual, "cam_b", body, new Vector3(-0.3f, 0.25f, -0.56f));
        Prim(PrimitiveType.Cylinder, "Tube", camB, new Vector3(0f, 0f, -0.1f), new Vector3(0.28f, 0.1f, 0.28f), dark, new Vector3(90f, 0f, 0f));
        Prim(PrimitiveType.Cylinder, "Lens", camB, new Vector3(0f, 0f, -0.21f), new Vector3(0.18f, 0.01f, 0.18f), glass, new Vector3(90f, 0f, 0f));

        var camA = Part(visual, "cam_a", body, new Vector3(0.22f, 0.25f, -0.56f));
        Prim(PrimitiveType.Cylinder, "Tube", camA, new Vector3(0f, 0f, -0.22f), new Vector3(0.45f, 0.22f, 0.45f), dark, new Vector3(90f, 0f, 0f));
        Prim(PrimitiveType.Cylinder, "Lens", camA, new Vector3(0f, 0f, -0.45f), new Vector3(0.32f, 0.01f, 0.32f), glass, new Vector3(90f, 0f, 0f));

        var spec = Part(visual, "espec", body, new Vector3(0.28f, -0.3f, -0.56f));
        Prim(PrimitiveType.Cube, "Box", spec, new Vector3(0f, 0f, -0.12f), new Vector3(0.42f, 0.34f, 0.24f), white);
        Prim(PrimitiveType.Cube, "Slit", spec, new Vector3(0f, 0f, -0.245f), new Vector3(0.3f, 0.05f, 0.01f), dark);

        var radar = Part(visual, "radar", body, new Vector3(-0.25f, -0.3f, -0.56f));
        Prim(PrimitiveType.Cube, "Arm", radar, new Vector3(0f, 0f, -0.12f), new Vector3(0.08f, 0.08f, 0.24f), metal);
        Prim(PrimitiveType.Cube, "Plate", radar, new Vector3(0f, 0f, -0.26f), new Vector3(0.5f, 0.32f, 0.04f), white);
        Prim(PrimitiveType.Cube, "Grid", radar, new Vector3(0f, 0f, -0.285f), new Vector3(0.44f, 0.04f, 0.01f), dark);

        // Paneles solares: alas a los costados
        BuildWings(visual, "panel_s", body, 1.2f, 0.7f, metal, cells);
        BuildWings(visual, "panel_l", body, 2.3f, 0.95f, metal, cells);

        var battery = Part(visual, "bat", body, new Vector3(0f, 0.1f, 0.56f));
        Prim(PrimitiveType.Cube, "Pack", battery, new Vector3(0f, 0f, 0.09f), new Vector3(0.55f, 0.42f, 0.18f), dark);
        Prim(PrimitiveType.Cube, "Stripe", battery, new Vector3(0f, 0.1f, 0.185f), new Vector3(0.55f, 0.06f, 0.01f), orange);

        var antS = Part(visual, "ant_s", body, new Vector3(0f, 0.71f, 0f));
        Model("Dish", SpaceKit + "satelliteDish", antS, Vector3.zero, 0.9f);
        var antL = Part(visual, "ant_l", body, new Vector3(0f, 0.71f, 0f));
        Model("Dish", SpaceKit + "satelliteDish_large", antL, Vector3.zero, 1.35f);
    }

    static void BuildWings(SatelliteVisual visual, string id, Transform body, float length, float width, Material metal, Material cells)
    {
        var wings = Part(visual, id, body, Vector3.zero);
        foreach (float side in new[] { -1f, 1f })
        {
            Prim(PrimitiveType.Cube, "Arm", wings, new Vector3(side * 0.8f, 0f, 0f), new Vector3(0.5f, 0.05f, 0.05f), metal);
            Prim(PrimitiveType.Cube, "Cells", wings, new Vector3(side * (1.05f + length / 2f), 0f, 0f), new Vector3(length, width, 0.04f), cells);
            Prim(PrimitiveType.Cube, "Frame", wings, new Vector3(side * (1.05f + length / 2f), 0f, 0.025f), new Vector3(length + 0.06f, width + 0.06f, 0.02f), metal);
        }
    }

    static Transform Part(SatelliteVisual visual, string id, Transform parent, Vector3 position)
    {
        var t = Group("Part_" + id, parent, position);
        visual.parts.Add(new SatelliteVisual.Entry { id = id, root = t.gameObject });
        return t;
    }

    static void BuildCamera(Transform focus)
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 45f;
        cam.farClipPlane = 200f;
        camGo.AddComponent<AudioListener>();
        var orbit = camGo.AddComponent<OrbitCamera>();
        orbit.focus = focus;
        camGo.transform.position = new Vector3(5f, 6f, -12f);
        camGo.transform.LookAt(new Vector3(0f, 3.5f, 0f));
    }

    // ---------- UI ----------

    static void BuildUI(HangarController controller)
    {
        var root = CreateCanvas().transform;
        var ui = root.gameObject.AddComponent<HangarUI>();
        ui.controller = controller;
        ui.uiSprite = UISprite;

        // Barra superior
        var top = UIFactory.Panel("TopBar", root, Palette.PanelDark);
        UIFactory.Place(top.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -64f), Vector2.zero);
        var title = UIFactory.Text("Title", top.transform, "ASSEMBLY HANGAR", 30f, Palette.Phosphor, TextAlignmentOptions.MidlineLeft, true);
        UIFactory.Place(title.rectTransform, Vector2.zero, new Vector2(0.5f, 1f), new Vector2(24f, 0f), Vector2.zero);
        var hint = UIFactory.Text("Hint", top.transform, "Drag to rotate  |  Scroll to zoom", 18f, Palette.Metal, TextAlignmentOptions.Center);
        UIFactory.Place(hint.rectTransform, new Vector2(0.3f, 0f), new Vector2(0.7f, 1f), Vector2.zero, Vector2.zero);
        ui.backButton = UIFactory.Button("Back", top.transform, "<  SPACE CENTER", Palette.Panel, Palette.Phosphor, 20f, true);
        var backRt = (RectTransform)ui.backButton.transform;
        UIFactory.Place(backRt, new Vector2(1f, 0f), Vector2.one, new Vector2(-270f, 8f), new Vector2(-16f, -8f));

        BuildCatalog(root, ui);
        BuildFunds(root, ui);
        BuildHud(root, ui);
    }

    static void BuildCatalog(Transform root, HangarUI ui)
    {
        var panel = UIFactory.Panel("Catalog", root, Palette.Panel);
        UIFactory.Place(panel.rectTransform, Vector2.zero, new Vector2(0f, 1f), new Vector2(20f, 150f), new Vector2(500f, -84f));

        var tabs = UIFactory.Rect("Tabs", panel.transform);
        UIFactory.Place(tabs, new Vector2(0f, 1f), Vector2.one, new Vector2(10f, -62f), new Vector2(-10f, -10f));
        var h = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 6f;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = h.childForceExpandHeight = true;
        string[] names = { "Instruments", "Power", "Rocket/Orbit", "Comms" };
        ui.tabButtons = new Button[names.Length];
        for (int i = 0; i < names.Length; i++)
            ui.tabButtons[i] = UIFactory.Button("Tab_" + names[i], tabs, names[i], Palette.PanelDark, Palette.Phosphor, 15f, true);

        ui.listContent = UIFactory.Rect("List", panel.transform);
        UIFactory.Place(ui.listContent, Vector2.zero, Vector2.one, new Vector2(10f, 170f), new Vector2(-10f, -72f));
        var v = ui.listContent.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 6f;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        var info = UIFactory.Panel("Info", panel.transform, Palette.PanelDark);
        UIFactory.Place(info.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(10f, 10f), new Vector2(-10f, 160f));
        ui.infoText = UIFactory.Text("Text", info.transform, "", 18f, Palette.TextLight, TextAlignmentOptions.TopLeft);
        UIFactory.Stretch(ui.infoText.rectTransform, 12f);
    }

    static void BuildFunds(Transform root, HangarUI ui)
    {
        var box = UIFactory.Panel("Funds", root, Palette.PanelDark);
        UIFactory.Place(box.rectTransform, Vector2.zero, Vector2.zero, new Vector2(20f, 20f), new Vector2(500f, 136f));
        ui.fundsText = UIFactory.Text("Text", box.transform, "", 44f, Palette.Phosphor, TextAlignmentOptions.MidlineLeft, true);
        UIFactory.Stretch(ui.fundsText.rectTransform, 18f);
    }

    static void BuildHud(Transform root, HangarUI ui)
    {
        var panel = UIFactory.Panel("Status", root, Palette.Panel);
        UIFactory.Place(panel.rectTransform, new Vector2(1f, 0f), Vector2.one, new Vector2(-500f, 20f), new Vector2(-20f, -84f));
        var v = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(20, 20, 18, 18);
        v.spacing = 10f;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        UIFactory.Layout(UIFactory.Text("Title", panel.transform, "MISSION STATUS", 26f, Palette.Phosphor, TextAlignmentOptions.MidlineLeft, true).gameObject, 34f);
        ui.contractText = UIFactory.Text("Contract", panel.transform, "", 20f, Color.white, TextAlignmentOptions.TopLeft);
        UIFactory.Layout(ui.contractText.gameObject, 56f);

        ui.massBar = Bar(panel.transform, "MASS  (satellite vs. what the rocket lifts)");
        ui.powerBar = Bar(panel.transform, "POWER  (used vs. made)");
        ui.dataBar = Bar(panel.transform, "DATA  (made vs. sent to Earth)");
        ui.costBar = Bar(panel.transform, "COST  (spent vs. budget)");

        ui.scienceText = UIFactory.Text("Science", panel.transform, "", 22f, Palette.Phosphor);
        UIFactory.Layout(ui.scienceText.gameObject, 30f);
        ui.riskText = UIFactory.Text("Risk", panel.transform, "", 22f, Palette.Phosphor);
        UIFactory.Layout(ui.riskText.gameObject, 30f);

        var alertsBox = UIFactory.Panel("Alerts", panel.transform, Palette.PanelDark);
        UIFactory.Layout(alertsBox.gameObject, -1f, -1f, -1f, 1f);
        ui.alertsText = UIFactory.Text("Text", alertsBox.transform, "", 17f, Palette.TextLight, TextAlignmentOptions.TopLeft);
        UIFactory.Stretch(ui.alertsText.rectTransform, 10f);

        ui.launchReasonText = UIFactory.Text("LaunchReason", panel.transform, "", 18f, Palette.Red, TextAlignmentOptions.Center);
        UIFactory.Layout(ui.launchReasonText.gameObject, 48f);
        ui.launchButton = UIFactory.Button("Launch", panel.transform, "LAUNCH", Palette.Orange, Palette.Space, 38f, true);
        UIFactory.Layout(ui.launchButton.gameObject, 84f);
    }

    static ResourceBar Bar(Transform parent, string label)
    {
        var group = UIFactory.Rect("Bar_" + label.Split(' ')[0], parent);
        UIFactory.Layout(group.gameObject, 62f);
        var bar = group.gameObject.AddComponent<ResourceBar>();

        var title = UIFactory.Text("Label", group, label, 16f, Palette.Metal, TextAlignmentOptions.MidlineLeft);
        UIFactory.Place(title.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -24f), Vector2.zero);

        var background = UIFactory.Panel("Background", group, Palette.PanelDark);
        UIFactory.Place(background.rectTransform, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 34f));

        bar.fillImage = UIFactory.Panel("Fill", background.transform, Palette.Phosphor, false);
        bar.fill = bar.fillImage.rectTransform;
        UIFactory.Place(bar.fill, Vector2.zero, new Vector2(0.5f, 1f), new Vector2(2f, 2f), new Vector2(-2f, -2f));

        bar.valueText = UIFactory.Text("Value", background.transform, "", 17f, Color.white, TextAlignmentOptions.Center, true);
        UIFactory.Stretch(bar.valueText.rectTransform);
        return bar;
    }
}
