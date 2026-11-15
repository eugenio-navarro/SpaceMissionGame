using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Arma la escena MenuPrincipal desde cero: la Tierra desde el espacio, un satélite y el menú.
// Reutiliza el cielo, el planeta y el satélite de Resultados y del Hangar.
public static class MenuSceneBuilder
{
    const string ScenePath = "Assets/Scenes/MenuPrincipal.unity";

    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cielo y luz (los mismos materiales que Resultados)
        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Results/Results_Sky.mat");
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.10f, 0.12f, 0.18f);

        var sun = new GameObject("Directional Light").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.transform.rotation = Quaternion.Euler(24f, 62f, 0f);
        sun.intensity = 1.7f;
        sun.color = new Color(1f, 0.97f, 0.92f);

        // Planeta: abajo a la derecha, con el horizonte curvo a la vista
        var planet = new GameObject("Planet", typeof(MeshFilter), typeof(MeshRenderer));
        planet.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Results/PlanetSphere.asset");
        planet.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Results/Results_Planet.mat");
        planet.transform.position = new Vector3(26f, -60f, 62f);
        planet.transform.localScale = Vector3.one * 56f;
        planet.transform.rotation = Quaternion.Euler(18f, 40f, 12f);

        // Satélite (el mismo del Hangar)
        var sat = new GameObject("Satellite");
        sat.transform.position = new Vector3(4.3f, 1.1f, 14f);
        sat.transform.rotation = Quaternion.Euler(12f, -35f, -8f);
        var visual = sat.AddComponent<SatelliteVisual>();
        var parts = new GameObject("Parts").transform;
        parts.SetParent(sat.transform, false);
        parts.localPosition = new Vector3(0f, -0.85f, 0f);
        parts.localScale = Vector3.one * 1.3f;
        visual.satellite = parts;
        HangarSceneBuilder.BuildSatellite(visual);

        // Cámara
        var rig = new GameObject("CameraRig");
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        camGo.transform.SetParent(rig.transform, false);
        var cam = camGo.AddComponent<Camera>();
        camGo.AddComponent<AudioListener>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 45f;
        cam.farClipPlane = 2000f;

        // Menú (la interfaz se arma al iniciar, con UIFactory)
        var canvas = BuilderUtil.CreateCanvas();
        var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
        menu.planet = planet.transform;
        menu.satellite = sat.transform;
        menu.satelliteParts = visual;
        menu.cameraRig = rig.transform;
        menu.canvasRoot = (RectTransform)canvas.transform;

        EditorSceneManager.SaveScene(scene, ScenePath);

        // Primera escena del juego
        var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("[MenuSceneBuilder] Escena MenuPrincipal armada.");
    }
}
