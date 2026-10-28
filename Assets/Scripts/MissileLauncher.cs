using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject missilePrefab; // The Missile prefab
    public Transform spawnPoint;     // The empty GameObject placed under the aircraft's wing

    [Header("Input")]
    public KeyCode fireKey = KeyCode.Space;

    void Update()
    {
        if (Input.GetKeyDown(fireKey))
        {
            FireMissile();
        }
    }

    void FireMissile()
    {
        if (missilePrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Please assign the Missile Prefab and the Spawn Point in the Inspector!");
            return;
        }

        // Spawn the missile at the spawn point's position with the aircraft's current rotation
        Instantiate(missilePrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
