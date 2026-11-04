using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private float _speed = 300f;
    [SerializeField] private float missileDamage = 25f;
    // [SerializeField] private ParticleSystem _smokeTrail;
    [SerializeField] private ParticleSystem _explosionPrefab;
    [SerializeField] private ParticleSystem _smokeTrail;
    private Rigidbody _rb;
    private Vector3 _velocity;

    private BoxCollider _boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _rb = GetComponent<Rigidbody>();
        Debug.Log("SmokeTrail : " + _smokeTrail);

        _smokeTrail.Play();

        Debug.Log("Fumée active : " + _smokeTrail.isPlaying);
    }

    public void Initialize(Vector3 planeVelocity)
    {
        _rb = GetComponent<Rigidbody>();

        _velocity = planeVelocity + transform.forward * _speed;

        _rb.useGravity = false;
        _rb.velocity = _velocity;

        Debug.Log("Vitesse avion : " + planeVelocity.magnitude);
        Debug.Log("Propulsion missile : " + _speed);
        Debug.Log("Vitesse initiale missile : " + _rb.velocity.magnitude);
    }

    // Update is called once per frame
    // void Update()
    // {
    //     transform.position += _velocity * Time.deltaTime;
    //     Debug.Log("Vitesse missile : " + _speed);

    // }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision avec : " + collision.gameObject.name);
        // _boxCollider.enabled = false;
        _smokeTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        ParticleSystem explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        explosion.Play();

        Destroy(explosion.gameObject, explosion.main.duration
           + explosion.main.startLifetime.constantMax);

        IDamageable target = collision.collider.GetComponentInParent<IDamageable>();

        if (target != null)
        {
            target.ApplyDamage(missileDamage);
            Debug.Log("Missile : " + missileDamage + " dégâts appliqués !");
        }

        Destroy(gameObject);
    }


}
