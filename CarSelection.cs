using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSelection : MonoBehaviour
{
    public CarControllerN[] cars;
    public static int selectedCar = 0;

    //public CarControllerN car;
    // Start is called before the first frame update
    public void NextCar()
    {
        cars[selectedCar].gameObject.SetActive(false);
        selectedCar = (selectedCar + 1) % cars.Length;
        cars[selectedCar].gameObject.SetActive(true);
    }
    public void PreviousCar()
    {
        cars[selectedCar].gameObject.SetActive(false);
        selectedCar--;
        if(selectedCar < 0)
        {
            selectedCar += cars.Length;
        }
        cars[selectedCar].gameObject.SetActive(true);

    }
  /* public void SelectCar()
    {
        //car = cars[selectedCar].GetComponent<CarControllerN>();
        RaceInfoHandler.instance.racerToUse = cars[selectedCar];
       // SceneManager.LoadScene(RaceInfoHandler.instance.trackToLoad);

    }*/
    public void Go()
    {
        //RaceInfoHandler.instance.racerToUse = cars[selectedCar].gameObject.GetComponent<CarControllerN>();
      //  SceneManager.LoadScene(RaceInfoHandler.instance.trackToLoad);

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D ) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextCar();
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousCar();
        }
    }
}
