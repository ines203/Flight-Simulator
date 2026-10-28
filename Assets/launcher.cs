using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class launcher : MonoBehaviour
{
    [SerializeField] private GameObject _rocket;
    [SerializeField] private GameObject _rocketPosition;
    [SerializeField] private GameObject _missileProp;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Fire Missile");
            Instantiate(_rocket, _rocketPosition.transform.position + new Vector3(0, -1f, 0), _rocketPosition.transform.rotation * Quaternion.Euler(0, 180, 0));
            // _missileProp.SetActive(false);
        }
    }
}
