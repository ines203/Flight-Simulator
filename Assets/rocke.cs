using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private float _speed = 10000f;
    // [SerializeField] private ParticleSystem _smokeTrail;
    [SerializeField] private ParticleSystem _explosionPrefab;
    [SerializeField] private ParticleSystem _smokeTrail;
    private Rigidbody _rb;

    private BoxCollider _boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _rb = GetComponent<Rigidbody>();
        _speed = 300f;
        Debug.Log("SmokeTrail : " + _smokeTrail);

        _smokeTrail.Play();

        Debug.Log("Fumée active : " + _smokeTrail.isPlaying);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(UnityEngine.Vector3.forward * Time.deltaTime * _speed);
        Debug.Log("Vitesse missile : " + _speed);

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision avec : " + collision.gameObject.name);
        _rb.useGravity = true;
        _speed = 0;
        // _boxCollider.enabled = false;
        _smokeTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        ParticleSystem explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        explosion.Play();
    }


}
