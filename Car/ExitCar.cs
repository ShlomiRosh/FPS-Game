using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;


public class ExitCar : MonoBehaviour
{

    public GameObject CarCam;
    public GameObject ThePlayer;
    public GameObject TheCar;
    public GameObject ExitTrigger;
    public GameObject ExitPlace;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Action"))
        {
            ThePlayer.SetActive(true);
            ThePlayer.transform.position = ExitPlace.transform.position;
            CarCam.SetActive(false);
            TheCar.GetComponent<CarController>().enabled = false;
            TheCar.GetComponent<CarUserControl>().enabled = false;
            //TheCar.GetComponent<CarAudio>().enabled = false;
            ExitTrigger.SetActive(false);
        }
    }
}
