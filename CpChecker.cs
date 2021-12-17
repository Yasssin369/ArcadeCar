using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CpChecker : MonoBehaviour
{
    public CarControllerN car;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Checkpoint")
        {
            car.CheckPointHit(other.GetComponent<CheckPoint>().cpNumber);
        }
    }
}
