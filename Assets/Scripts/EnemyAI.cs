using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    private Plane plane;

    void Awake()
    {
        plane = GetComponent<Plane>();
    }

    void FixedUpdate()
    {
        if (plane == null || plane.Dead)
            return;

        plane.SetThrottleInput(1f);
        plane.SetControlInput(Vector3.zero);
    }
}