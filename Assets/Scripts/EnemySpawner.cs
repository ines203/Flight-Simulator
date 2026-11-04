using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    GameObject planePrefab;

    [SerializeField]
    Transform enemySpawnPoint;

    void Start()
    {
        string gameMode = PlayerPrefs.GetString("GameMode", "VolLibre");

        if (gameMode == "CombatLibre")
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        GameObject enemy = Instantiate(
            planePrefab,
            enemySpawnPoint.position,
            enemySpawnPoint.rotation
        );
        enemy.AddComponent<EnemyAIController>();

        enemy.name = "EnemyPlane";
    }
}