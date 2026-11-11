using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Missile : MonoBehaviour
{
    [Header("Missile Settings")]
    public float speed = 60f;
    public float lifetime = 5f;

    [Header("Audio")]
    [Tooltip("Son joué au point d'impact. Optionnel.")]
    public AudioClip impactSound;
    [Range(0f, 1f)]
    public float impactVolume = 1f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Disable gravity for a straight rocket-like flight
        rb.useGravity = false; 

        // Automatically destroy the missile after X seconds if it hasn't hit anything
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        // Continuously propel the missile forward along its Z-axis
        // Note: For Unity 2023+ or Unity 6, you can use rb.linearVelocity instead of rb.velocity
        rb.velocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Log the impact in the Unity console
        Debug.Log("Missile impacted: " + collision.gameObject.name);
        
        // Explosion or damage logic will go here
        
        // Sfx.PlayAt keeps playing after the missile is destroyed
        Sfx.PlayAt(impactSound, transform.position, impactVolume);

        // Destroy the missile immediately upon impact
        Destroy(gameObject);
    }
}
