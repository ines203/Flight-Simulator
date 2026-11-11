using UnityEngine;

/// <summary>
/// Joue une musique ou une ambiance en boucle dans une scène.
///
/// À poser sur un objet vide de la scène (ex. : « Musique » dans le menu
/// principal), puis glisser le clip dans l'Inspector. La musique s'arrête
/// toute seule au changement de scène, avec l'objet.
/// </summary>
public class MusicPlayer : MonoBehaviour {
    [SerializeField]
    AudioClip music;
    [SerializeField]
    [Range(0f, 1f)]
    float volume = 0.5f;

    void Start() {
        if (music == null) return;

        var source = gameObject.AddComponent<AudioSource>();
        source.clip = music;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = volume;
        // La musique continue même si un script met AudioListener.pause à vrai.
        source.ignoreListenerPause = true;
        source.Play();
    }
}
