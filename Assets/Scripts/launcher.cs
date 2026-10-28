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

    public void FireMissile()
    {
        if (_rocket == null || _missilePropR == null || _missilePropL == null)
        {
            Debug.LogWarning("Missiles non configurés !");
            return;
        }

        Debug.Log("Fire Missiles");

        Instantiate(
            _rocket,
            _missilePropR.transform.position,
            _missilePropR.transform.rotation
        );

        Instantiate(
            _rocket,
            _missilePropL.transform.position,
            _missilePropL.transform.rotation
        );
    }
}
