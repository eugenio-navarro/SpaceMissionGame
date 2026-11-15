using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Secuencia automática del lanzamiento: cuenta regresiva, despegue, separación de etapa,
// llegada a órbita, cartel de evento y paso a Resultados. El jugador solo mira (o salta).
public class LaunchSequence : MonoBehaviour
{
    [Header("Escena")]
    public GameObject world;
    public Transform rocket;
    public Transform stage1;
    public ParticleSystem stage1Flame;
    public ParticleSystem stage1Smoke;
    public ParticleSystem stage2Flame;
    public ParticleSystem padSmoke;
    public LaunchCamera launchCamera;
    public Camera cam;
    public LaunchHud hud;
    public Renderer starsRenderer;

    [Header("Tiempos (segundos)")]
    public float countdownSeconds = 3f;
    public float stage1Seconds = 8f;
    public float stage2Seconds = 6f;
    public float orbitHoldSeconds = 3f;
    public float eventSeconds = 5f;
    public float fadeSeconds = 0.8f;

    [Header("Movimiento")]
    public float acceleration = 4.4f;

    [Header("Cielo")]
    public Color skyGround = new Color(0.45f, 0.70f, 0.95f);
    public Color skyHigh = new Color(0.10f, 0.20f, 0.45f);
    public Color skySpace = new Color(0.043f, 0.059f, 0.094f);

    [Header("Cámara (offset respecto del cohete)")]
    public Vector3 camPad = new Vector3(7f, 1.5f, -12f);
    public Vector3 camAscent = new Vector3(5f, -5f, -10f);
    public Vector3 camUpperStage = new Vector3(9f, -1f, -10f);
    public Vector3 camOrbit = new Vector3(11f, 2f, -7f);

    public string nextScene = "Resultados";

    LaunchInfo info;
    Material starsMaterial;
    bool leaving;
    Vector3 rocketStart;

    void Start()
    {
        info = LaunchContext.GetOrCreateTestData();
        LaunchContext.LaunchFinished = false;

        rocketStart = rocket.position;
        hud.Setup(info);
        hud.SetClock(-countdownSeconds);
        if (hud.skipButton != null) hud.skipButton.onClick.AddListener(Skip);

        if (starsRenderer != null) starsMaterial = starsRenderer.material;
        SetSky(0f);

        launchCamera.offset = camPad;
        StartCoroutine(Run());
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)) Skip();
    }

    public void Skip()
    {
        if (leaving) return;
        StopAllCoroutines();
        StartCoroutine(Leave());
    }

    IEnumerator Run()
    {
        // 1. Cuenta regresiva
        bool ignited = false;
        for (float t = countdownSeconds; t > 0f; t -= Time.deltaTime)
        {
            hud.SetClock(-t);
            hud.SetStatus("Launch in " + Mathf.CeilToInt(t));
            if (!ignited && t <= 1.2f)
            {
                ignited = true;
                stage1Flame.Play();
                stage1Smoke.Play();
                padSmoke.Play();
                launchCamera.shake = 0.05f;
                hud.SetStatus("Ignition!");
            }
            yield return null;
        }

        // 2. Ascenso (etapa 1 + etapa 2)
        float total = stage1Seconds + stage2Seconds;
        bool separated = false;
        float stage1Velocity = 0f;
        float stage1Timer = 0f;
        launchCamera.offset = camAscent;
        launchCamera.shake = 0.12f;

        for (float t = 0f; t < total; t += Time.deltaTime)
        {
            float p = t / total;
            float height = 0.5f * acceleration * t * t;
            rocket.position = rocketStart + Vector3.up * height;

            hud.SetClock(t);
            hud.SetAltitude(info.orbitAltitudeKm * 1000f * p * p);
            SetSky(p);
            if (world != null && world.activeSelf && p > 0.6f) world.SetActive(false); // ya no se ve el suelo
            launchCamera.shake = Mathf.Lerp(0.12f, 0f, p * 2f);

            if (t < 2.5f) hud.SetStatus("Liftoff!");
            else if (t < stage1Seconds - 2f) hud.SetStatus("Max Q: the air pushes hardest on the rocket");
            else if (t < stage1Seconds) hud.SetStatus("Stage 1 is almost out of fuel");
            else if (t < stage1Seconds + 2.5f) hud.SetStatus("Stage separation: the empty booster drops away");
            else hud.SetStatus("Leaving the atmosphere");

            if (!separated && t >= stage1Seconds)
            {
                separated = true;
                stage1Flame.Stop();
                stage1Smoke.Stop();
                stage1.SetParent(null, true);
                stage1Velocity = acceleration * t;
                stage2Flame.Play();
                hud.SetActiveStage(1);
                launchCamera.offset = camUpperStage;
            }

            if (separated && stage1 != null && stage1.gameObject.activeSelf)
            {
                // La etapa vacía sigue subiendo un poco por inercia y después cae.
                stage1Timer += Time.deltaTime;
                stage1Velocity -= 25f * Time.deltaTime;
                stage1.position += Vector3.up * stage1Velocity * Time.deltaTime;
                stage1.Rotate(12f * Time.deltaTime, 0f, 20f * Time.deltaTime, Space.World);
                if (stage1Timer > 5f) stage1.gameObject.SetActive(false);
            }

            yield return null;
        }

        // 3. Llegada a órbita
        stage2Flame.Stop();
        if (stage1 != null) stage1.gameObject.SetActive(false);
        SetSky(1f);
        if (world != null) world.SetActive(false);
        hud.SetAltitude(info.orbitAltitudeKm * 1000f);
        hud.SetActiveStage(2);
        hud.SetStatus("Orbit reached: " + info.orbitName + ", " + info.orbitAltitudeKm.ToString("0") + " km");
        launchCamera.offset = camOrbit;
        launchCamera.shake = 0f;

        float clock = total;
        for (float t = 0f; t < orbitHoldSeconds; t += Time.deltaTime)
        {
            clock += Time.deltaTime;
            hud.SetClock(clock);
            yield return null;
        }

        // 4. Evento
        hud.ShowEvent(info.eventTitle, info.eventText, info.eventKind == LaunchEventKind.Calm);
        for (float t = 0f; t < eventSeconds; t += Time.deltaTime)
        {
            clock += Time.deltaTime;
            hud.SetClock(clock);
            yield return null;
        }

        yield return Leave();
    }

    IEnumerator Leave()
    {
        leaving = true;
        LaunchContext.LaunchFinished = true;
        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
        {
            hud.SetFade(t / fadeSeconds);
            yield return null;
        }
        hud.SetFade(1f);
        SceneManager.LoadScene(nextScene);
    }

    // p = 0 en la plataforma, 1 en órbita.
    void SetSky(float p)
    {
        Color c = p < 0.5f
            ? Color.Lerp(skyGround, skyHigh, p / 0.5f)
            : Color.Lerp(skyHigh, skySpace, (p - 0.5f) / 0.3f);
        cam.backgroundColor = c;

        if (starsMaterial != null)
        {
            float a = Mathf.InverseLerp(0.45f, 0.85f, p);
            starsMaterial.SetColor("_BaseColor", new Color(1f, 1f, 1f, a));
        }
    }
}
