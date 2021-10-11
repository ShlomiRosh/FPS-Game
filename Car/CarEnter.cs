using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class CarEnter : MonoBehaviour
{
    public GameObject CarCam;
    public GameObject ThePlayer;
    public GameObject TheCar;
    public GameObject ExitTrigger;
    public bool TriggerCheck;

    void OnTriggerEnter(Collider collider)
    {
        TriggerCheck = true;
    }

    void OnTriggerExit(Collider collider)
    {
        TriggerCheck = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (TriggerCheck)
        {
            if (Input.GetButtonDown("Action"))
            {
                CarCam.SetActive(true);
                ThePlayer.SetActive(false);
                TheCar.GetComponent<CarController>().enabled = true;
                TheCar.GetComponent<CarUserControl>().enabled = true;
                //TheCar.GetComponent<CarAudio>().enabled = true;
                ExitTrigger.SetActive(true);
            }
        }
    }
}
