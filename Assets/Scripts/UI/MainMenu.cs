using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    [SerializeField]
    string gameplaySceneName = "Main";

    public void StartVolLibre() {
        PlayerPrefs.SetString("GameMode", "VolLibre");
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void StartChasse() {
        PlayerPrefs.SetString("GameMode", "Chasse");
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void StartVoyage() {
        PlayerPrefs.SetString("GameMode", "Voyage");
        SceneManager.LoadScene(gameplaySceneName);
    }
}
