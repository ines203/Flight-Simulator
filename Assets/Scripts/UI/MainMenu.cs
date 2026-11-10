using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    [SerializeField]
    string gameplaySceneName = "Main";
    [SerializeField]
    GameObject comingSoonPanel;
    [SerializeField]
    string userSceneName = "UserSelect";

    void OnGUI() {
        string user = UserSession.HasUser ? UserSession.CurrentUser : "(aucun)";
        GUI.Label(new Rect(10, 10, 400, 25), "Utilisateur : " + user);

        if (GUI.Button(new Rect(10, 40, 180, 30), "Changer d'utilisateur")) {
            SceneManager.LoadScene(userSceneName);
        }
    }

    public void StartVolLibre() {
        PlayerPrefs.SetString("GameMode", "VolLibre");
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void StartVoyage() {
        PlayerPrefs.SetString("GameMode", "Voyage");
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void StartCombatAerien() {
        if (comingSoonPanel != null) {
            comingSoonPanel.SetActive(true);
        }
    }
}
