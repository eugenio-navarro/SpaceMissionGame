using UnityEngine;

public enum Sfx { Hover, Click, Accept, Deny, Whoosh, Success, Partial, Fail, Rumble }

// Sonidos y música generados por código (no hay archivos de audio en el proyecto).
// Cada clip se arma una sola vez, la primera vez que se pide.
public static class SoundBank
{
    const int Rate = 22050;
    static readonly AudioClip[] clips = new AudioClip[System.Enum.GetValues(typeof(Sfx)).Length];
    static AudioClip music;

    public static AudioClip Get(Sfx s)
    {
        int i = (int)s;
        if (clips[i] == null) clips[i] = Build(s);
        return clips[i];
    }

    static AudioClip Build(Sfx s)
    {
        switch (s)
        {
            case Sfx.Hover: return Clip("hover", 0.04f, t => Sine(2200f, t) * Decay(t, 60f) * 0.12f);
            case Sfx.Click: return Clip("click", 0.09f, t => Tri(t < 0.03f ? 900f : 1350f, t) * Decay(t, 35f) * 0.35f);
            case Sfx.Accept: return Notes("accept", new[] { 523f, 659f, 784f }, 0.07f, 0.25f, 0.3f);
            case Sfx.Deny: return Clip("deny", 0.18f, t => Square(220f - 60f * t, t) * Decay(t, 14f) * 0.16f);
            case Sfx.Whoosh: return Clip("whoosh", 0.55f, t => Noise() * Mathf.Sin(Mathf.PI * t / 0.55f) * 0.12f, 0.08f);
            case Sfx.Success: return Notes("success", new[] { 523f, 659f, 784f, 1047f }, 0.11f, 1.1f, 0.3f);
            case Sfx.Partial: return Notes("partial", new[] { 523f, 659f, 587f }, 0.14f, 0.7f, 0.28f);
            case Sfx.Fail: return Notes("fail", new[] { 392f, 330f, 262f, 233f }, 0.17f, 0.9f, 0.3f);
            case Sfx.Rumble: return Clip("rumble", 2f, t => Noise() * 0.5f, 0.015f, true);
        }
        return null;
    }

    // ---------- Música: pad espacial lento (4 acordes de 6 s, en loop) ----------

    public static AudioClip Music()
    {
        if (music != null) return music;

        float[][] chords =
        {
            new[] { 110f, 220f, 261.6f, 329.6f, 392f },  // Am7
            new[] { 87.3f, 174.6f, 220f, 261.6f, 329.6f }, // Fmaj7
            new[] { 130.8f, 196f, 261.6f, 329.6f, 392f }, // C
            new[] { 98f, 196f, 246.9f, 293.7f, 370f },    // G (con F# de color)
        };
        float[] bell = { 659.3f, 784f, 880f, 987.8f, 1174.7f };
        const float chordLen = 6f;
        float length = chordLen * chords.Length;
        int n = (int)(length * Rate);
        var data = new float[n];
        var rng = new System.Random(7);

        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Rate;
            float v = 0f;
            // Cada acorde entra y sale con un fundido que se superpone con el siguiente (el loop no corta).
            for (int c = 0; c < chords.Length; c++)
            {
                float local = Mathf.Repeat(t - c * chordLen + 1.5f, length);
                if (local > chordLen + 3f) continue;
                float env = Mathf.Sin(Mathf.PI * local / (chordLen + 3f));
                env *= env;
                foreach (float f in chords[c])
                    v += (Mathf.Sin(2f * Mathf.PI * f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * f * 1.003f * t)) * env;
            }
            v *= 0.022f;
            data[i] = v;
        }

        // Campanitas suaves cada 1,5 s, con eco.
        for (float start = 0.75f; start < length; start += 1.5f)
        {
            float f = bell[rng.Next(bell.Length)];
            for (int echo = 0; echo < 3; echo++)
            {
                float amp = 0.05f * Mathf.Pow(0.45f, echo);
                int from = (int)((start + echo * 0.375f) * Rate);
                for (int k = 0; k < Rate * 2; k++)
                {
                    int idx = (from + k) % n;
                    float tt = k / (float)Rate;
                    data[idx] += Mathf.Sin(2f * Mathf.PI * f * tt) * Mathf.Exp(-tt * 2.5f) * Mathf.Min(1f, tt * 200f) * amp;
                }
            }
        }

        music = AudioClip.Create("music", n, 1, Rate, false);
        music.SetData(data, 0);
        return music;
    }

    // ---------- Ayudas de síntesis ----------

    static float noiseState;
    static readonly System.Random random = new System.Random(1);

    static float Sine(float f, float t) => Mathf.Sin(2f * Mathf.PI * f * t);
    static float Tri(float f, float t) => 1f - 4f * Mathf.Abs(Mathf.Repeat(f * t + 0.25f, 1f) - 0.5f);
    static float Square(float f, float t) => Mathf.Repeat(f * t, 1f) < 0.5f ? 1f : -1f;
    static float Decay(float t, float speed) => Mathf.Exp(-t * speed) * Mathf.Min(1f, t * 400f);
    static float Noise() => (float)(random.NextDouble() * 2.0 - 1.0);

    // smoothing < 1 pasa la señal por un filtro pasabajos (ruido más grave).
    // loop: suaviza el principio y el final para que se pueda repetir sin clic.
    static AudioClip Clip(string name, float seconds, System.Func<float, float> wave, float smoothing = 1f, bool loop = false)
    {
        int n = (int)(seconds * Rate);
        var data = new float[n];
        noiseState = 0f;
        for (int i = 0; i < n; i++)
        {
            float v = wave(i / (float)Rate);
            noiseState += (v - noiseState) * smoothing;
            data[i] = smoothing < 1f ? noiseState * 4f : v;
        }
        if (loop)
        {
            int fade = Rate / 4;
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                data[i] = data[i] * k + data[n - fade + i] * (1f - k);
            }
            System.Array.Resize(ref data, n - fade);
            n -= fade;
        }
        var clip = AudioClip.Create(name, n, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Arpegio: cada nota entra 'step' segundos después de la anterior y suena 'tail' segundos.
    static AudioClip Notes(string name, float[] freqs, float step, float tail, float volume)
    {
        float seconds = step * (freqs.Length - 1) + tail;
        return Clip(name, seconds, t =>
        {
            float v = 0f;
            for (int i = 0; i < freqs.Length; i++)
            {
                float local = t - i * step;
                if (local < 0f) continue;
                float sustain = i == freqs.Length - 1 ? 3f : 9f; // la última nota queda sonando
                v += (Tri(freqs[i], local) * 0.7f + Sine(freqs[i] * 2f, local) * 0.3f) * Decay(local, sustain);
            }
            return v * volume;
        });
    }
}
