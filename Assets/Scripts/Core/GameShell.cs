using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Lo que comparten todas las escenas y sobrevive al cambio de escena: fundidos, música y efectos,
// menú de pausa (Esc), créditos, pantalla de título, tarjetas de tutorial y cursor de mano.
// Se crea solo al arrancar el juego; las escenas no necesitan tenerlo.
public class GameShell : MonoBehaviour
{
    public static GameShell Instance { get; private set; }

    // Una escena puede usar Esc para cerrar sus propios paneles: devuelve true si lo usó.
    public static Func<bool> SceneEscape;

    public const string HubScene = "CentroEspacial";
    public const string MenuScene = "MenuPrincipal";

    RectTransform tutorialLayer, titleLayer, menuLayer, creditsLayer;
    Image fade;
    AudioSource music, effects, loop;
    bool loading;
    float lastHover;

    public bool MenuOpen => menuLayer.childCount > 0;
    bool CreditsOpen => creditsLayer.childCount > 0;
    bool TitleOpen => titleLayer.childCount > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Instance != null) return;
        var go = new GameObject("GameShell");
        DontDestroyOnLoad(go);
        go.AddComponent<GameShell>();
    }

    void Awake()
    {
        Instance = this;
        BuildCanvas();
        BuildAudio();
        StartCoroutine(ApplyLayout());
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(FadeTo(0f, 0.5f, 1f));
    }

    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        Clear(tutorialLayer);
        Clear(titleLayer);
        Clear(menuLayer);
        Clear(creditsLayer);
        StopLoop();
        SetPointer(false);
        loading = false;
        StopAllCoroutines();
        StartCoroutine(FadeTo(0f, 0.45f, 1f));
        StartCoroutine(ApplyLayout());
    }

    IEnumerator ApplyLayout()
    {
        yield return null;
        UILayout.ApplyScene();
    }

    // ---------- Cambio de escena con fundido ----------

    public static void Load(string scene)
    {
        if (Instance == null) { SceneManager.LoadScene(scene); return; }
        Instance.StartCoroutine(Instance.LoadRoutine(scene));
    }

    IEnumerator LoadRoutine(string scene)
    {
        if (loading) yield break;
        loading = true;
        Play(Sfx.Whoosh);
        yield return FadeTo(1f, 0.35f);
        SceneManager.LoadScene(scene);
    }

    IEnumerator FadeTo(float target, float seconds, float? from = null)
    {
        if (from.HasValue) SetFade(from.Value);
        float start = fade.color.a;
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            SetFade(Mathf.Lerp(start, target, t / seconds));
            yield return null;
        }
        SetFade(target);
    }

    void SetFade(float a)
    {
        fade.color = new Color(0f, 0f, 0f, a);
        fade.raycastTarget = a > 0.01f;
    }

    // Destello de color sobre toda la pantalla (por ejemplo, cuando la misión falla).
    public void Flash(Color color, float seconds = 0.6f) => StartCoroutine(FlashRoutine(color, seconds));

    IEnumerator FlashRoutine(Color color, float seconds)
    {
        var img = UIFactory.Image("Flash", creditsLayer.parent, null, color);
        UIFactory.Stretch(img.rectTransform);
        img.transform.SetSiblingIndex(fade.transform.GetSiblingIndex());
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            img.color = new Color(color.r, color.g, color.b, color.a * (1f - t / seconds));
            yield return null;
        }
        Destroy(img.gameObject);
    }

    // Papelitos de colores sobre la escena (debajo de los menús).
    public static void Confetti()
    {
        if (Instance != null) global::Confetti.Burst(Instance.tutorialLayer);
    }

    // ---------- Sonido ----------

    void BuildAudio()
    {
        music = gameObject.AddComponent<AudioSource>();
        music.clip = SoundBank.Music();
        music.loop = true;
        music.ignoreListenerPause = true;
        effects = gameObject.AddComponent<AudioSource>();
        loop = gameObject.AddComponent<AudioSource>();
        loop.loop = true;
        ApplyVolumes();
        music.Play();
    }

    public void ApplyVolumes()
    {
        music.volume = SaveData.MusicVolume * 0.5f;
        effects.volume = SaveData.EffectsVolume;
        loop.volume = SaveData.EffectsVolume * 0.7f;
    }

    public static void Play(Sfx s, float volume = 1f)
    {
        if (Instance == null) return;
        if (s == Sfx.Hover)
        {
            // Evita el "ametrallado" al pasar rápido por una lista.
            if (Time.unscaledTime - Instance.lastHover < 0.05f) return;
            Instance.lastHover = Time.unscaledTime;
        }
        Instance.effects.PlayOneShot(SoundBank.Get(s), volume);
    }

    // Sonido continuo (el rugido del cohete) hasta que se llame a StopLoop.
    public static void PlayLoop(Sfx s)
    {
        if (Instance == null) return;
        Instance.loop.clip = SoundBank.Get(s);
        Instance.loop.Play();
    }

    public static void StopLoop()
    {
        if (Instance != null) Instance.loop.Stop();
    }

    // ---------- Esc y pausa ----------

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame || loading) return;

        if (CreditsOpen) CloseCredits();
        else if (MenuOpen) CloseMenu();
        else if (TitleOpen) return;
        else if (SceneEscape != null && SceneEscape()) return;
        else OpenMenu();
    }

    public void OpenMenu() => OpenMenu(false);
    public void OpenOptions() => OpenMenu(true); // desde el menú principal: solo opciones

    void OpenMenu(bool fromTitle)
    {
        if (MenuOpen) return;
        Time.timeScale = 0f;
        loop.Pause();
        Play(Sfx.Click);
        ShellScreens.BuildMenu(this, menuLayer, fromTitle);
    }

    public void CloseMenu()
    {
        Clear(menuLayer);
        Time.timeScale = 1f;
        loop.UnPause();
    }

    // Ventana de aviso con OK (por ejemplo, un contrato nuevo desbloqueado).
    public static void Announce(string caption, string title, string body, float delay = 0f)
    {
        if (Instance != null) Instance.StartCoroutine(Instance.AnnounceRoutine(caption, title, body, delay));
    }

    IEnumerator AnnounceRoutine(string caption, string title, string body, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Play(Sfx.Accept);
        ShellScreens.BuildNotice(this, creditsLayer, caption, title, body);
    }

    public void OpenGuide() { Clear(creditsLayer); ShellScreens.BuildGuide(this, creditsLayer); }
    public void OpenCredits() => ShellScreens.BuildCredits(this, creditsLayer);
    public void CloseCredits() => Clear(creditsLayer);

    public void ToMainMenu()
    {
        CloseMenu();
        ResultsContext.Clear();
        Load(MenuScene);
    }

    public void ToSpaceCenter()
    {
        CloseMenu();
        ResultsContext.Clear();
        Load(HubScene);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Tutorial ----------

    // Tarjeta de pasos abajo al centro. Se marca como vista al terminarla o saltearla.
    public static void Tutorial(string key, TutorialStep[] steps, float bottom, float width, Func<bool> hideWhen = null, Action onFinish = null)
    {
        if (Instance == null || SaveData.Seen(key)) return;
        Clear(Instance.tutorialLayer);
        var coach = UIFactory.Rect("Tutorial", Instance.tutorialLayer).gameObject.AddComponent<TutorialCoach>();
        coach.Setup(key, steps, bottom, width, hideWhen, onFinish);
    }

    // ---------- Cursor ----------

    Texture2D hand;

    public static void SetPointer(bool on)
    {
        if (Instance == null) return;
        if (on && Instance.hand == null) Instance.hand = CursorArt.Hand();
        Cursor.SetCursor(on ? Instance.hand : null, on ? new Vector2(9f, 1f) : Vector2.zero, CursorMode.Auto);
    }

    // ---------- Armado ----------

    void BuildCanvas()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        UILayout.Configure(scaler);
        gameObject.AddComponent<GraphicRaycaster>();

        var root = (RectTransform)transform;
        tutorialLayer = Layer("Tutorial", root);
        titleLayer = Layer("Title", root);
        menuLayer = Layer("Menu", root);
        creditsLayer = Layer("Credits", root);
        fade = UIFactory.Image("Fade", root, null, Color.black);
        UIFactory.Stretch(fade.rectTransform);
    }

    static RectTransform Layer(string name, RectTransform parent)
    {
        var rt = UIFactory.Rect(name, parent);
        UIFactory.Stretch(rt);
        return rt;
    }

    static void Clear(Transform layer)
    {
        foreach (Transform child in layer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        if (layer.childCount > 0) layer.DetachChildren();
    }
}
