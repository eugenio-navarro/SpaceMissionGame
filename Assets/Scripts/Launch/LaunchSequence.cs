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
    public LaunchRocket[] rockets;
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

    [Tooltip("Solo para probar sin pasar por el Hangar: chico, mediano o grande. Vacio = lo que diga LaunchInfo.")]
    public string testRocketId = "";

    Transform rocket;
    Transform stage1;
    ParticleSystem stage1Flame;
    ParticleSystem stage1Smoke;
    ParticleSystem stage2Flame;

    LaunchInfo info;
    Material starsMaterial;
    bool leaving;
    Vector3 rocketStart;
    SeparationChallenge challenge;
    bool separateRequested; // el jugador apretó Espacio o hizo clic en la tarjeta del desafío

    void Start()
    {
        // La misión llega desde el Hangar; si se abre la escena sola, se usan datos de prueba.
        ResultsInfo run = ResultsContext.GetOrCreate();
        if (ResultsContext.IsTestData && !string.IsNullOrEmpty(testRocketId))
        {
            run.rocketId = testRocketId;
            run.rocketName = testRocketId == "chico" ? "Small rocket" : testRocketId == "grande" ? "Large rocket" : "Medium rocket";
        }
        // El evento y el dado de riesgo se sortean acá, una sola vez por lanzamiento.
        if (!run.resolved)
            MissionOutcome.Resolve(run, (LaunchEventKind)Random.Range(0, 3), Random.Range(0f, 100f));

        info = LaunchInfo.FromRun(run);
        LaunchContext.LaunchFinished = false;

        PickRocket(info.rocketId);
        rocketStart = rocket.position;
        hud.Setup(info);
        if (info.orbitId == "lunar") LabelLunarTrip();
        hud.SetClock(-countdownSeconds);
        if (hud.skipButton != null) hud.skipButton.onClick.AddListener(Skip);
        challenge = SeparationChallenge.Create(hud.fade.canvas.rootCanvas.transform, RequestSeparation);
        challenge.transform.SetSiblingIndex(hud.fade.transform.GetSiblingIndex()); // debajo del fundido

        if (starsRenderer != null) starsMaterial = starsRenderer.material;
        SetSky(0f);

        launchCamera.offset = camPad;
        StartCoroutine(Run());
    }

    // Hacia la Luna el número que sube no es la altura de la órbita (unos 50 km sobre la Luna)
    // sino la distancia recorrida desde la Tierra: se rotula así para no enseñar un dato falso.
    void LabelLunarTrip()
    {
        hud.orbitLabel.text = "MOON 384,000 km";
        foreach (var t in hud.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            if (t.text == "ALTITUDE") t.text = "DISTANCE FROM EARTH";
    }

    // Deja activa solo la variante pedida y ajusta la cámara a su tamaño.
    void PickRocket(string id)
    {
        LaunchRocket picked = rockets[0];
        foreach (LaunchRocket r in rockets)
            if (r.id == id) picked = r;
        foreach (LaunchRocket r in rockets)
            r.gameObject.SetActive(r == picked);

        rocket = picked.transform;
        stage1 = picked.stage1;
        stage1Flame = picked.stage1Flame;
        stage1Smoke = picked.stage1Smoke;
        stage2Flame = picked.stage2Flame;

        float k = Mathf.Max(0.75f, picked.height / 8f);
        camPad *= k;
        camAscent *= k;
        camUpperStage *= k;
        camOrbit *= k;
        launchCamera.target = rocket;
        launchCamera.lookHeight = picked.height * 0.5f;
    }

    void Update()
    {
        // Esc abre la pausa (GameShell); Espacio suelta la etapa 1 (mini desafío); Enter saltea.
        if (GameShell.Instance != null && GameShell.Instance.MenuOpen) return;
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.spaceKey.wasPressedThisFrame) RequestSeparation();
        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) Skip();
    }

    void RequestSeparation()
    {
        if (challenge != null && challenge.Active) separateRequested = true;
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
                GameShell.PlayLoop(Sfx.Rumble);
                launchCamera.shake = 0.05f;
                hud.SetStatus("Ignition!");
            }
            yield return null;
        }

        // 2. Ascenso (etapa 1 + etapa 2)
        float total = stage1Seconds + stage2Seconds;
        bool separated = false;
        bool challengeStarted = false;
        float windowStart = stage1Seconds - 1.6f, windowEnd = stage1Seconds + 0.7f;
        string separationMessage = "Stage separation: the empty booster drops away";
        float separatedAt = 0f;
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

            if (t < 4f) hud.SetStatus("Liftoff! Time is sped up: the real climb to orbit takes ~10 min");
            else if (separated) hud.SetStatus(t < separatedAt + 2.5f ? separationMessage : "Leaving the atmosphere");
            else if (t < stage1Seconds - 2f) hud.SetStatus("Max Q: the air pushes hardest on the rocket");
            else if (t < stage1Seconds) hud.SetStatus("Stage 1 is almost out of fuel");
            else hud.SetStatus("Stage 1 is out of fuel: separate it now!");

            // Mini desafío: el jugador suelta la etapa 1; si no lo hace, la computadora la suelta sola.
            if (!separated && !challengeStarted && t >= windowStart)
            {
                challengeStarted = true;
                challenge.Begin(windowStart, windowEnd, stage1Seconds);
            }
            if (challengeStarted) challenge.Tick(t);

            if (!separated && challengeStarted && (separateRequested || t >= windowEnd))
            {
                var rating = separateRequested ? SeparationChallenge.Rate(t, stage1Seconds) : SeparationRating.Automatic;
                LaunchContext.Separation = rating;
                challenge.Finish(rating);
                GameShell.Play(rating == SeparationRating.Perfect ? Sfx.Accept : Sfx.Click);
                separationMessage = rating == SeparationRating.Perfect ? "Perfect separation! The empty booster drops away"
                    : rating == SeparationRating.Automatic ? "Automatic separation: the flight computer dropped the booster"
                    : "Stage separation: the empty booster drops away";
                separatedAt = t;
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
        GameShell.StopLoop();
        GameShell.Play(Sfx.Accept);
        if (stage1 != null) stage1.gameObject.SetActive(false);
        SetSky(1f);
        if (world != null) world.SetActive(false);
        hud.SetAltitude(info.orbitAltitudeKm * 1000f);
        hud.SetActiveStage(2);
        hud.SetStatus(info.orbitId == "lunar"
            ? "Arrived at the Moon, 384,000 km away. Orbiting ~50 km above it"
            : "Orbit reached: " + info.orbitName + ", " + info.orbitAltitudeKm.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture) + " km up");
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
        GameShell.Play(ResultsContext.GetOrCreate().eventHurt ? Sfx.Deny : Sfx.Click);
        hud.ShowEvent(info.eventTitle, info.eventText, !ResultsContext.GetOrCreate().eventHurt);
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
        GameShell.StopLoop();
        GameShell.Play(Sfx.Whoosh);
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
