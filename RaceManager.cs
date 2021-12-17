using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;
    public CheckPoint[] allCheckPoints;

    public CarControllerN PlayerCar;
    public int totalLaps;
    public List<CarControllerN> allAICars = new List<CarControllerN>();
    public int playerPosition;
    public float checkTime=.2f;
    private float posChecker;
    //RubberBand
    public float aiDefaultSpeed = 30f;
    public float playerDefaultSpeed = 30f;
    public float rubberBandSpeedMod = 3.5f;
    public float rubberBandAccel = 0.5f;
    //Race start count down
    public bool isStarting;
    public float timeBetweenCount = 1f;
    private float startCounter;
    public int countdownCurrent = 3;
    //spawning
    public int playerStartPosition,aiToSpawn;
    public Transform[] startPoints;
    public List<CarControllerN> carsToSpawn = new List<CarControllerN>();
    // Race 
    public bool raceComplete;
    public string raceCompleteScene;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {

        totalLaps = RaceInfoHandler.instance.noOfLaps;
        aiToSpawn = RaceInfoHandler.instance.noOfAI;
        for(int i=0; i<allCheckPoints.Length; i++)
        {
            allCheckPoints[i].cpNumber = i;
        }
        isStarting = true;
        startCounter = timeBetweenCount;

        UIManager.instance.countDown.text = countdownCurrent + "!";
        playerStartPosition = Random.Range(0, aiToSpawn + 1);
        PlayerCar = Instantiate(RaceInfoHandler.instance.racerToUse, startPoints[playerStartPosition].position, startPoints[playerStartPosition].rotation);
        PlayerCar.isAI = false;
        TopDownCamera.instance.Target = PlayerCar;
        TopDownCamera.instance.startTargetOffset = PlayerCar.transform;

        PlayerCar.GetComponent<AudioListener>().enabled = true;

        // PlayerCar.transform.position = startPoints[playerStartPosition].position;
        // PlayerCar.theRB.transform.position = startPoints[playerStartPosition].position;

        for (int i=0; i<aiToSpawn+1; i++)
        {
            if(i !=playerStartPosition)
            {
                int selectedCar = Random.Range(0, carsToSpawn.Count);
              allAICars.Add(Instantiate(carsToSpawn[selectedCar], startPoints[i].position, startPoints[i].rotation));
                if(carsToSpawn.Count > aiToSpawn - i )
                {
                    carsToSpawn.RemoveAt(selectedCar);

                }
            }
        }
        UIManager.instance.position.text = (playerStartPosition+1) + "/" + (allAICars.Count + 1);
        AudioHandler.instance.Gameplay();
    }

    // Update is called once per frame
    void Update()
    {
        if(isStarting)
        {
            startCounter -= Time.deltaTime;
            if(startCounter <= 0)
            {
                countdownCurrent--;
                startCounter = timeBetweenCount;

                UIManager.instance.countDown.text = countdownCurrent + "!";

                if (countdownCurrent == 0)
                {
                    isStarting = false;
                    UIManager.instance.countDown.gameObject.SetActive(false);
                    UIManager.instance.Go.gameObject.SetActive(true);

                }
            }
        }
        else 
        { 
        posChecker -= Time.deltaTime;
        if (posChecker <= 0 && !raceComplete)
        {
            playerPosition = 1;
            foreach (CarControllerN aiCar in allAICars)
            {
                if (aiCar.currentLap > PlayerCar.currentLap)
                {
                    playerPosition++;
                }
                else if (aiCar.currentLap == PlayerCar.currentLap)
                {
                    if (aiCar.nextCheckPoint > PlayerCar.nextCheckPoint)
                    {
                        playerPosition++;
                    }
                    else if (aiCar.nextCheckPoint == PlayerCar.nextCheckPoint)
                    {
                        if (Vector3.Distance(aiCar.transform.position, allCheckPoints[aiCar.nextCheckPoint].transform.position) <
                            Vector3.Distance(PlayerCar.transform.position, allCheckPoints[aiCar.nextCheckPoint].transform.position))
                        {
                            playerPosition++;
                        }
                    }
                }
            }

            posChecker = checkTime;
            UIManager.instance.position.text = playerPosition +"/"+ (allAICars.Count + 1) ;
        }
        //Rubber Banding
        if(playerPosition <=4)
        {
            foreach (CarControllerN aiCar in allAICars)
            {
                aiCar.maxSpeed = Mathf.MoveTowards(aiCar.maxSpeed , aiDefaultSpeed + rubberBandSpeedMod  , rubberBandAccel * Time.deltaTime);
            }
            PlayerCar.maxSpeed = Mathf.MoveTowards(PlayerCar.maxSpeed, playerDefaultSpeed - rubberBandSpeedMod, rubberBandAccel * Time.deltaTime);

        }
        if(playerPosition>=6)
        {
            foreach (CarControllerN aiCar in allAICars)
            {
                aiCar.maxSpeed = Mathf.MoveTowards(aiCar.maxSpeed, aiDefaultSpeed - 
                (rubberBandSpeedMod * ((float)playerPosition / ((float)allAICars.Count + 1))), rubberBandAccel * Time.deltaTime);
            }
            PlayerCar.maxSpeed = Mathf.MoveTowards(PlayerCar.maxSpeed, playerDefaultSpeed +
            ( rubberBandSpeedMod * ((float)playerPosition / ((float)allAICars.Count+1 ))), rubberBandAccel * Time.deltaTime);

        }
      }
    }
    public void Finishrace()
    {
        raceComplete = true;
        switch(playerPosition)
        {
            case 1:
                UIManager.instance.raceResult.text = "OoOooH Weeeh You finished 1st!";
                break;

            case 2:
                UIManager.instance.raceResult.text = "OoOooH Weeeh You finished 2nd!";
                break;
            case 3:
                UIManager.instance.raceResult.text = "OoOooH Weeeh You finished 3rd!";
                break;

            default:
                UIManager.instance.raceResult.text = " You barely finished the " + playerPosition + "th";

                break;
        }
       // UIManager.instance.raceResult.text = "OoOooH Weeeh You finished " + playerPosition + "th";

        UIManager.instance.resultScreen.SetActive(true);
    }
    public void ExitRace()
    {
        SceneManager.LoadScene(raceCompleteScene);
        AudioHandler.instance.MenuMusic();

    }
}
