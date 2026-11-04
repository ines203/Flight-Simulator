using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class launcher : MonoBehaviour
{
    [SerializeField] private GameObject _rocket;
    [SerializeField] private GameObject _missilePropR;
    [SerializeField] private GameObject _missilePropL;

    // [SerializeField] private GameObject _missileProp;
    // Start is called before the first frame update
    // void Start()
    // {

    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         Debug.Log("Fire Missile");
    //         Instantiate(_rocket, _missilePropR.transform.position ,_missilePropR.transform.rotation);
    //         Instantiate(_rocket, _missilePropL.transform.position ,_missilePropL.transform.rotation);

    //         // _missileProp.SetActive(false);
    //     }
    // }

    // public void FireMissile()
    // {
    //     if (_rocket == null || _missilePropR == null || _missilePropL == null)
    //     {
    //         Debug.LogWarning("Missiles non configurés !");
    //         return;
    //     }

    //     Debug.Log("Fire Missiles");

    //     Instantiate(
    //         _rocket,
    //         _missilePropR.transform.position,
    //         _missilePropR.transform.rotation
    //     );

    //     Instantiate(
    //         _rocket,
    //         _missilePropL.transform.position,
    //         _missilePropL.transform.rotation
    //     );
    // }



    public void FireMissile()
    {
        if (_rocket == null || _missilePropR == null || _missilePropL == null)
        {
            Debug.LogWarning("Missiles non configurés !");
            return;
        }

        Rigidbody planeRb = GetComponentInParent<Rigidbody>();

        if (planeRb == null)
        {
            Debug.LogError("Rigidbody de l'avion introuvable !");
            return;
        }

        Vector3 planeVelocity = planeRb.velocity;

        Quaternion rotation = Quaternion.LookRotation(
            planeRb.transform.forward,
            planeRb.transform.up
        );

        // Missile droit
        GameObject missileR = Instantiate(
            _rocket,
            _missilePropR.transform.position,
            rotation
        );

        missileR.GetComponent<Rocket>().Initialize(planeVelocity);


        // Missile gauche
        GameObject missileL = Instantiate(
            _rocket,
            _missilePropL.transform.position,
            rotation
        );

        missileL.GetComponent<Rocket>().Initialize(planeVelocity);
    }


}
