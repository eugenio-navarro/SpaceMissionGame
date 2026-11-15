using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static BuilderUtil;

// Arma la escena CentroEspacial desde cero. Ojo: borra lo que haya en la escena.
// Usarlo para el armado inicial; si después se edita a mano, no volver a correrlo.
public static class HubSceneBuilder
{
    const string ScenePath = "Assets/Scenes/CentroEspacial.unity";

    [MenuItem("Tools/Escenas/Construir CentroEspacial")]
    public static string Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);

        AddLight("Sun", LightType.Directional, new Vector3(0f, 30f, 0f), new Vector3(45f, -35f, 0f), 1.3f, new Color(1f, 0.96f, 0.88f));
        BuildWorld();
        var hub = new GameObject("HubController").AddComponent<HubController>();
        BuildCamera();
        BuildUI(hub);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "CentroEspacial built";
    }

    // ---------- Mundo 3D ----------

    static void BuildWorld()
    {
        var world = new GameObject("World").transform;
        var grass = Mat("Grass", Palette.Hex("#5E8C3F"), 0f, 0.1f);
        var road = Mat("Road", Palette.Hex("#7B8087"), 0f, 0.2f);
        var roadLine = Mat("RoadLine", Palette.Hex("#E8E2C8"), 0f, 0.2f);

        Prim(PrimitiveType.Plane, "Ground", world, Vector3.zero, new Vector3(60f, 1f, 60f), grass);
        Prim(PrimitiveType.Cube, "Road_NS", world, new Vector3(0f, 0.02f, 0f), new Vector3(4f, 0.04f, 70f), road);
        Prim(PrimitiveType.Cube, "Road_EW", world, new Vector3(0f, 0.02f, 0f), new Vector3(60f, 0.04f, 4f), road);
        Prim(PrimitiveType.Cylinder, "Plaza", world, new Vector3(0f, 0.03f, 0f), new Vector3(12f, 0.02f, 12f), road);
        for (int i = -6; i <= 6; i++)
        {
            if (Mathf.Abs(i) < 2) continue;
            Prim(PrimitiveType.Cube, $"Line_NS_{i}", world, new Vector3(0f, 0.05f, i * 5f), new Vector3(0.25f, 0.02f, 2f), roadLine);
            Prim(PrimitiveType.Cube, $"Line_EW_{i}", world, new Vector3(i * 4.5f, 0.05f, 0f), new Vector3(2f, 0.02f, 0.25f), roadLine);
        }

        var buildings = Group("Buildings", world, Vector3.zero);

        var missionControl = Model("MissionControl", CityKit + "building-f", buildings, new Vector3(-12f, 0f, 8f), 4.5f, 180f);
        Interactable(missionControl, BuildingKind.MissionControl, "Mission Control");

        var hangar = Model("AssemblyHangar", SpaceKit + "hangar_largeA", buildings, new Vector3(13f, 0f, 10f), 5f);
        Interactable(hangar, BuildingKind.Hangar, "Assembly Hangar");

        var stations = Group("GroundStations", buildings, new Vector3(14f, 0f, -12f)).gameObject;
        Model("Dish_A", SpaceKit + "satelliteDish_large", stations.transform, new Vector3(-4.5f, 0f, 0f), 5f, 30f);
        Model("Dish_B", SpaceKit + "satelliteDish_large", stations.transform, new Vector3(0f, 0f, 3.5f), 5f, -20f);
        Model("Dish_C", SpaceKit + "satelliteDish_large", stations.transform, new Vector3(4.5f, 0f, -1f), 5f, 60f);
        Interactable(stations, BuildingKind.Antennas, "Ground Stations");

        var dome = Model("MissionArchive", SpaceKit + "hangar_roundGlass", buildings, new Vector3(-13f, 0f, -12f), 3.5f, 180f);
        Interactable(dome, BuildingKind.Dome, "Mission Archive");

        BuildDecor(world);
    }

    static void BuildDecor(Transform world)
    {
        var decor = Group("Decor", world, Vector3.zero);

        // Plataforma de lanzamiento con un cohete parado
        Model("LaunchPad", SpaceKit + "platform_large", decor, new Vector3(0f, 0f, 27f), 4f);
        var rocket = Group("PadRocket", decor, new Vector3(0f, 0.4f, 27f));
        float y = 0f;
        foreach (var part in new[] { "rocket_baseA", "rocket_fuelA", "rocket_fuelA", "rocket_fuelA", "rocket_topA" })
        {
            var piece = Model(part, SpaceKit + part, rocket, new Vector3(0f, y, 0f), 2.2f);
            y += Height(piece);
        }

        Model("Tank_A", CityKit + "detail-tank-large", decor, new Vector3(-25f, 0f, 18f), 3f);
        Model("Tank_B", CityKit + "detail-tank-large", decor, new Vector3(-20f, 0f, 25f), 3f, 90f);
        Model("WaterTower", CityKit + "water-tower", decor, new Vector3(25f, 0f, 22f), 4f);
        Model("Office", CityKit + "building-a", decor, new Vector3(-27f, 0f, -2f), 3.5f, 90f);
        Model("Workshop", CityKit + "building-k", decor, new Vector3(27f, 0f, 3f), 3.5f, -90f);
        Model("Generator", SpaceKit + "machine_generatorLarge", decor, new Vector3(5f, 0f, 17f), 3f);
        string[] containers = { "shipping-container-a", "shipping-container-b", "shipping-container-c" };
        for (int i = 0; i < 5; i++)
            Model($"Container_{i}", CityKit + containers[i % 3], decor, new Vector3(-26f + i * 2.4f, 0f, -22f), 4f, 90f);
    }

    static void Interactable(GameObject go, BuildingKind kind, string displayName)
    {
        FitCollider(go);
        var b = go.AddComponent<BuildingInteractable>();
        b.kind = kind;
        b.displayName = displayName;
    }

    static void BuildCamera()
    {
        var pivot = new GameObject("CameraPivot");
        pivot.AddComponent<HubCameraSway>();
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.SetParent(pivot.transform, false);
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 45f;
        cam.farClipPlane = 600f;
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<PhysicsRaycaster>();
        camGo.transform.localPosition = new Vector3(0f, 34f, -44f);
        camGo.transform.LookAt(new Vector3(0f, 0f, 2f));
    }

    // ---------- UI ----------

    static void BuildUI(HubController hub)
    {
        var root = CreateCanvas().transform;

        // Barra superior
        var top = UIFactory.Panel("TopBar", root, Palette.PanelDark);
        UIFactory.Place(top.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -64f), Vector2.zero);
        var title = UIFactory.Text("Title", top.transform, "SPACE MISSION DESIGN", 30f, Palette.Phosphor, TextAlignmentOptions.MidlineLeft, true);
        UIFactory.Place(title.rectTransform, Vector2.zero, new Vector2(0.45f, 1f), new Vector2(24f, 0f), Vector2.zero);
        hub.statusText = UIFactory.Text("Status", top.transform, "", 22f, Palette.Phosphor, TextAlignmentOptions.MidlineRight);
        UIFactory.Place(hub.statusText.rectTransform, new Vector2(0.4f, 0f), Vector2.one, Vector2.zero, new Vector2(-24f, 0f));

        var hint = UIFactory.Text("Hint", root, "Click a building to begin", 22f, new Color(1f, 1f, 1f, 0.85f), TextAlignmentOptions.Center);
        UIFactory.Place(hint.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 16f), new Vector2(0f, 56f));

        // Tooltip que sigue al mouse
        var tip = UIFactory.Panel("Tooltip", root, Palette.PanelDark);
        tip.raycastTarget = false;
        hub.tooltip = tip.rectTransform;
        hub.tooltip.anchorMin = hub.tooltip.anchorMax = Vector2.zero;
        hub.tooltip.pivot = new Vector2(0f, 1f);
        hub.tooltip.sizeDelta = new Vector2(300f, 46f);
        tip.gameObject.SetActive(false);
        hub.tooltipText = UIFactory.Text("Text", tip.transform, "Building", 22f, Palette.Phosphor, TextAlignmentOptions.Center, true);
        UIFactory.Stretch(hub.tooltipText.rectTransform, 6f);

        // Aviso temporal (toast)
        var toast = UIFactory.Panel("Toast", root, Palette.PanelDark);
        toast.raycastTarget = false;
        toast.rectTransform.anchorMin = toast.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        toast.rectTransform.anchoredPosition = new Vector2(0f, 130f);
        toast.rectTransform.sizeDelta = new Vector2(1000f, 64f);
        hub.toastGroup = toast.gameObject.AddComponent<CanvasGroup>();
        hub.toastGroup.blocksRaycasts = false;
        hub.toastGroup.alpha = 0f;
        hub.toastText = UIFactory.Text("Text", toast.transform, "", 24f, Palette.Phosphor, TextAlignmentOptions.Center);
        UIFactory.Stretch(hub.toastText.rectTransform, 8f);

        hub.contractsPanel = BuildContractsPanel(root, hub);
        BuildInfoPanel(root, hub);
    }

    static RectTransform Modal(string name, Transform root, Vector2 size, out Image window)
    {
        var modal = UIFactory.Rect(name, root);
        UIFactory.Stretch(modal);
        var dim = modal.gameObject.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.55f); // bloquea los clics a los edificios
        window = UIFactory.Panel("Window", modal, Palette.Panel);
        window.rectTransform.anchorMin = window.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        window.rectTransform.sizeDelta = size;
        return modal;
    }

    static Button CloseButton(Transform window, HubController hub)
    {
        var close = UIFactory.Button("Close", window, "X", Palette.PanelDark, Palette.Phosphor, 26f, true);
        var rt = (RectTransform)close.transform;
        rt.anchorMin = rt.anchorMax = Vector2.one;
        rt.pivot = Vector2.one;
        rt.anchoredPosition = new Vector2(-14f, -14f);
        rt.sizeDelta = new Vector2(52f, 52f);
        UnityEventTools.AddPersistentListener(close.onClick, hub.CloseAll);
        return close;
    }

    static GameObject BuildContractsPanel(Transform root, HubController hub)
    {
        var modal = Modal("ContractsPanel", root, new Vector2(1440f, 760f), out var window);
        var header = UIFactory.Text("Header", window.transform, "MISSION CONTROL  -  Choose your contract", 34f, Palette.Phosphor, TextAlignmentOptions.MidlineLeft, true);
        UIFactory.Place(header.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(36f, -84f), new Vector2(-90f, -10f));
        CloseButton(window.transform, hub);

        var cards = UIFactory.Rect("Cards", window.transform);
        UIFactory.Place(cards, Vector2.zero, Vector2.one, new Vector2(30f, 30f), new Vector2(-30f, -96f));
        var row = cards.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 28f;
        row.childControlWidth = row.childControlHeight = true;
        row.childForceExpandWidth = row.childForceExpandHeight = true;

        Color[] colors = { Palette.Planet, Palette.Hex("#6B4FB0"), Palette.Hex("#9AA3AD") };
        for (int i = 0; i < TestCatalog.Contracts.Count; i++)
            BuildCard(cards, TestCatalog.Contracts[i], colors[i], i, hub);

        modal.gameObject.SetActive(false);
        return modal.gameObject;
    }

    static void BuildCard(Transform parent, ContractSpec c, Color color, int index, HubController hub)
    {
        var card = UIFactory.Panel("Card_" + c.id, parent, Palette.PanelDark);
        var v = card.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(20, 20, 20, 20);
        v.spacing = 12f;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        var destination = UIFactory.Panel("Destination", card.transform, color, false);
        UIFactory.Layout(destination.gameObject, 190f);
        var destText = UIFactory.Text("Text", destination.transform, c.destination, 44f, Color.white, TextAlignmentOptions.Center, true);
        UIFactory.Stretch(destText.rectTransform);

        UIFactory.Layout(UIFactory.Text("Title", card.transform, c.title, 30f, Color.white, TextAlignmentOptions.TopLeft, true).gameObject);
        UIFactory.Layout(UIFactory.Text("Objective", card.transform, c.objective, 22f, Palette.TextLight, TextAlignmentOptions.TopLeft).gameObject);

        string orbits = string.Join(", ", c.allowedOrbits.Select(id => TestCatalog.Get(id).displayName));
        string details = $"Science goal: {c.scienceGoal}\nBudget: ${c.budget}M\nOrbits: {orbits}\nReal mission like it: {c.similarMission}";
        UIFactory.Layout(UIFactory.Text("Details", card.transform, details, 20f, Palette.Phosphor, TextAlignmentOptions.TopLeft).gameObject);

        var spacer = UIFactory.Rect("Spacer", card.transform);
        UIFactory.Layout(spacer.gameObject, -1f, -1f, -1f, 1f);

        var button = c.unlocked
            ? UIFactory.Button("Accept", card.transform, "ACCEPT", Palette.Orange, Palette.Space, 28f, true)
            : UIFactory.Button("Locked", card.transform, "LOCKED", Palette.Panel, Palette.TextDisabled, 28f, true);
        UIFactory.Layout(button.gameObject, 64f);
        button.interactable = c.unlocked;
        if (c.unlocked) UnityEventTools.AddIntPersistentListener(button.onClick, hub.AcceptContract, index);
    }

    static void BuildInfoPanel(Transform root, HubController hub)
    {
        var modal = Modal("InfoPanel", root, new Vector2(820f, 420f), out var window);
        hub.infoTitle = UIFactory.Text("Title", window.transform, "TITLE", 32f, Palette.Phosphor, TextAlignmentOptions.MidlineLeft, true);
        UIFactory.Place(hub.infoTitle.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(32f, -84f), new Vector2(-90f, -12f));
        hub.infoBody = UIFactory.Text("Body", window.transform, "", 24f, Palette.TextLight, TextAlignmentOptions.TopLeft);
        UIFactory.Place(hub.infoBody.rectTransform, Vector2.zero, Vector2.one, new Vector2(32f, 110f), new Vector2(-32f, -100f));
        CloseButton(window.transform, hub);

        var ok = UIFactory.Button("OK", window.transform, "OK", Palette.Orange, Palette.Space, 26f, true);
        var rt = (RectTransform)ok.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 60f);
        rt.sizeDelta = new Vector2(220f, 60f);
        UnityEventTools.AddPersistentListener(ok.onClick, hub.CloseAll);

        hub.infoPanel = modal.gameObject;
        modal.gameObject.SetActive(false);
    }
}
