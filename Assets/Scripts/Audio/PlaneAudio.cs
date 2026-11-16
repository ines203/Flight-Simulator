using UnityEngine;

/// <summary>
/// Sons continus de l'avion : moteur, postcombustion et vent.
///
/// À ajouter sur le prefab de l'avion (le même objet que Plane). Il suffit
/// de glisser les clips dans l'Inspector : les AudioSource sont créés par
/// le script au démarrage. Un clip laissé vide est simplement ignoré.
///
///  - Moteur : volume et hauteur (pitch) suivent la puissance réelle du
///    moteur, qui met plusieurs secondes à monter : on entend le réacteur
///    prendre son régime. La vitesse de l'avion ajoute un peu d'aigu.
///  - Postcombustion : s'ajoute au-dessus de 50 % de puissance (plein
///    régime sec), comme dans le modèle du moteur (Engine.cs).
///  - Vent : volume selon la vitesse de l'avion.
///  - Crash : joué une fois quand l'avion meurt ; les boucles s'arrêtent.
///
/// Les clips de moteur, postcombustion et vent doivent être des boucles
/// (un son qui se répète sans coupure audible).
/// </summary>
[RequireComponent(typeof(Plane))]
public class PlaneAudio : MonoBehaviour {
    [Header("Moteur")]
    [SerializeField]
    [Tooltip("Boucle du moteur.")]
    AudioClip engineLoop;
    [SerializeField]
    [Range(0f, 1f)]
    float engineVolumeIdle = 0.15f;
    [SerializeField]
    [Range(0f, 1f)]
    float engineVolumeMax = 0.7f;
    [SerializeField]
    [Tooltip("Hauteur du son au ralenti (1 = son d'origine).")]
    float enginePitchIdle = 0.6f;
    [SerializeField]
    [Tooltip("Hauteur du son à plein régime sec (sans postcombustion).")]
    float enginePitchMax = 1.5f;
    [SerializeField]
    [Tooltip("Hauteur ajoutée quand l'avion atteint Speed For Max Effect : le son continue d'évoluer après le décollage.")]
    float speedPitchBoost = 0.3f;
    [SerializeField]
    [Tooltip("Vitesse, en m/s, à laquelle l'effet de la vitesse sur le moteur est maximal.")]
    float speedForMaxEffect = 250f;

    [Header("Postcombustion")]
    [SerializeField]
    [Tooltip("Boucle de la postcombustion. Optionnel.")]
    AudioClip afterburnerLoop;
    [SerializeField]
    [Range(0f, 1f)]
    float afterburnerVolume = 1f;

    [Header("Vent")]
    [SerializeField]
    [Tooltip("Boucle du vent. Optionnel.")]
    AudioClip windLoop;
    [SerializeField]
    [Range(0f, 1f)]
    float windVolumeMax = 0.5f;
    [SerializeField]
    [Tooltip("Vitesse, en m/s, à laquelle le vent atteint son volume maximal.")]
    float windMaxSpeed = 300f;

    [Header("Crash")]
    [SerializeField]
    [Tooltip("Son joué une fois quand l'avion est détruit. Optionnel.")]
    AudioClip crashSound;
    [SerializeField]
    [Range(0f, 1f)]
    float crashVolume = 1f;

    [Header("Réglages")]
    [SerializeField]
    [Tooltip("Vitesse à laquelle les volumes suivent les changements (plus grand = plus rapide).")]
    float smoothing = 4f;

    Plane plane;
    AudioSource engineSource;
    AudioSource afterburnerSource;
    AudioSource windSource;
    bool deathHandled;

    void Awake() {
        plane = GetComponent<Plane>();

        engineSource = CreateLoop("Audio Moteur", engineLoop);
        afterburnerSource = CreateLoop("Audio Postcombustion", afterburnerLoop);
        windSource = CreateLoop("Audio Vent", windLoop);
    }

    // Crée un enfant avec un AudioSource en boucle, volume à 0 au départ.
    // Les sons sont en 2D : c'est l'avion du joueur, on l'entend toujours
    // de la même façon, peu importe où regarde la caméra.
    AudioSource CreateLoop(string objectName, AudioClip clip) {
        if (clip == null) return null;

        var go = new GameObject(objectName);
        go.transform.SetParent(transform, false);

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0f;
        source.Play();

        return source;
    }

    void Update() {
        if (plane.Dead) {
            HandleDeath();
            return;
        }

        // Time.unscaledDeltaTime : le fondu garde la même vitesse même si
        // le jeu est ralenti.
        float t = Mathf.Clamp01(smoothing * Time.unscaledDeltaTime);
        float speed = plane.Velocity.magnitude;

        // EnginePowerOutput va de 0 à 100 : 0-50 = régime sec, 50-100 =
        // postcombustion. Contrairement à la manette des gaz, qui atteint
        // le maximum en une seconde, la puissance monte progressivement.
        float power = plane.EnginePowerOutput;
        float dry = Mathf.InverseLerp(0f, 50f, power);
        float afterburner = Mathf.InverseLerp(50f, 100f, power);

        if (engineSource != null) {
            float speedAmount = speedForMaxEffect > 0f ? Mathf.Clamp01(speed / speedForMaxEffect) : 0f;
            float volume = Mathf.Lerp(engineVolumeIdle, engineVolumeMax, dry) * Sfx.MasterVolume;
            float pitch = Mathf.Lerp(enginePitchIdle, enginePitchMax, dry) + speedPitchBoost * speedAmount;

            engineSource.volume = Mathf.Lerp(engineSource.volume, volume, t);
            engineSource.pitch = Mathf.Lerp(engineSource.pitch, pitch, t);
        }

        if (afterburnerSource != null) {
            float volume = afterburner * afterburnerVolume * Sfx.MasterVolume;

            afterburnerSource.volume = Mathf.Lerp(afterburnerSource.volume, volume, t);
            afterburnerSource.pitch = Mathf.Lerp(0.85f, 1.15f, afterburner);
        }

        if (windSource != null) {
            float amount = windMaxSpeed > 0f ? Mathf.Clamp01(speed / windMaxSpeed) : 0f;
            float volume = amount * windVolumeMax * Sfx.MasterVolume;

            windSource.volume = Mathf.Lerp(windSource.volume, volume, t);
            windSource.pitch = Mathf.Lerp(0.8f, 1.3f, amount);
        }
    }

    void HandleDeath() {
        if (deathHandled) return;
        deathHandled = true;

        if (engineSource != null) engineSource.Stop();
        if (afterburnerSource != null) afterburnerSource.Stop();
        if (windSource != null) windSource.Stop();

        Sfx.Play2D(crashSound, crashVolume);
    }
}
