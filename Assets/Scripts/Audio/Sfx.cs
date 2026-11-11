using UnityEngine;

/// <summary>
/// Petit utilitaire pour jouer un effet sonore ponctuel depuis n'importe
/// quel script, en une ligne :
///
///     Sfx.Play2D(clip);                       // interface, tir du joueur
///     Sfx.PlayAt(clip, transform.position);   // explosion dans le monde
///
/// Les deux méthodes ne font rien si le clip est vide : on peut donc
/// ajouter un champ AudioClip à un script et le laisser vide sans erreur.
///
/// Pourquoi ne pas utiliser AudioSource.PlayClipAtPoint ? Sa distance
/// d'écoute par défaut est pensée pour un jeu à pied. Ici les distances
/// se comptent en centaines de mètres : PlayAt règle donc lui-même les
/// distances minimale et maximale.
/// </summary>
public static class Sfx {
    /// <summary>Volume général des effets sonores, entre 0 et 1.</summary>
    public static float MasterVolume = 1f;

    static AudioSource source2D;

    /// <summary>
    /// Joue un son en 2D (même volume partout). Plusieurs sons peuvent se
    /// superposer : utile pour un canon à cadence élevée.
    /// </summary>
    public static void Play2D(AudioClip clip, float volume = 1f) {
        if (clip == null) return;

        if (source2D == null) {
            var go = new GameObject("Sfx 2D");
            Object.DontDestroyOnLoad(go);

            source2D = go.AddComponent<AudioSource>();
            source2D.playOnAwake = false;
            source2D.spatialBlend = 0f;
        }

        source2D.PlayOneShot(clip, volume * MasterVolume);
    }

    /// <summary>
    /// Joue un son en 3D à une position du monde. Un objet temporaire est
    /// créé puis détruit à la fin du clip : le son continue même si
    /// l'objet qui l'a déclenché (missile, montgolfière) est détruit.
    /// </summary>
    /// <param name="minDistance">En dessous de cette distance, le son est à plein volume.</param>
    /// <param name="maxDistance">Au-delà de cette distance, le son n'est plus audible.</param>
    public static void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float minDistance = 50f, float maxDistance = 3000f) {
        if (clip == null) return;

        var go = new GameObject("Sfx " + clip.name);
        go.transform.position = position;

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume * MasterVolume;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = Mathf.Max(minDistance + 1f, maxDistance);
        source.dopplerLevel = 0f;
        source.Play();

        Object.Destroy(go, clip.length + 0.1f);
    }
}
