using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] Spawner spawner;
    [SerializeField] float delayBeforeGameOver = 3f;
    [SerializeField] GameObject gameOverPanel;

    bool gameOverStarted;

    void Update()
    {
        Plane plane = spawner.ActivePlane;
        if (plane != null && plane.Dead && !gameOverStarted)
        {
            gameOverStarted = true;
            StartCoroutine(ShowGameOver());
        }
    }

    IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(delayBeforeGameOver);
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();

        #if UNITY_EDITOR      
        UnityEditor.EditorApplication.isPlaying=false;
        #endif
    }
}
