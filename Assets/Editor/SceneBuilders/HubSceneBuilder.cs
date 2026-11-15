using System.Linq;
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

    static void BuildUI(HubController hub) => HubUIBuilder.Build(hub);
}
