using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    [SerializeField]
    string gameplaySceneName = "Main";
    [SerializeField]
    GameObject comingSoonPanel;

    public void StartVolLibre() {
        PlayerPrefs.SetString("GameMode", "VolLibre");
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void StartVoyage() {
        PlayerPrefs.SetString("GameMode", "Voyage");
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void StartCombatAerien() {
        // if (comingSoonPanel != null) {
        //     comingSoonPanel.SetActive(true);
        // }

        PlayerPrefs.SetString("GameMode", "CombatLibre");
        SceneManager.LoadScene(gameplaySceneName);
    }
}
