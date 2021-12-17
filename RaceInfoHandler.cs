using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceInfoHandler : MonoBehaviour
{
    public static RaceInfoHandler instance;

    public string trackToLoad;
    public CarControllerN racerToUse;
    public int noOfAI;
    public int noOfLaps;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
