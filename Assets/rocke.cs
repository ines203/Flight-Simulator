using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private ParticleSystem _smokeTrail;
    private BoxCollider _boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(UnityEngine.Vector3.forward * Time.deltaTime * _speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision");
        _speed = 0;
        _boxCollider.enabled = false;
        _smokeTrail.Stop();
    }
}
