using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Joue un son quand on clique sur un bouton de l'interface.
///
/// À ajouter sur le même objet qu'un composant Button (menu principal,
/// écran Game Over...), puis glisser le clip dans l'Inspector.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour {
    [SerializeField]
    AudioClip clickSound;
    [SerializeField]
    [Range(0f, 1f)]
    float volume = 1f;

    Button button;

    void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClick);
    }

    void OnDestroy() {
        if (button != null) {
            button.onClick.RemoveListener(PlayClick);
        }
    }

    void PlayClick() {
        Sfx.Play2D(clickSound, volume);
    }
}
