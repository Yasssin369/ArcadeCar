using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerN : MonoBehaviour
{
    public Rigidbody theRB;
    public float maxSpeed = 30f;

    public float forwardAccel = 8f;
    public float reverseAccel=4f;
    
    private float speedInput;

    public float turnStrength = 180f;
    public float turnInput;

    public bool grounded;
    public Transform groundRayPoint;
    public LayerMask ground;
    public float groundRayLength = 0.75f;

    private float dragOnGround;
    public float gravity = 10f;
    public Transform groundRayPointBalance;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    public ParticleSystem[] dustTrail;
    public float maxEmission = 25;
    public float emissionFadeSpeed = 20f;
    public float emissionRate;
    /// <summary>
    /// /drifting testing
    /// </summary>
    public bool drifting;
    public float driftDir;
    // float driftAngle;
    public float driftSlide = 500f;
    public float driftForce= 700f;
    public TrailRenderer[] skids;
    /// <summary>
    /// /CheckPoints
    /// </summary>
    public int nextCheckPoint;
    public int currentLap = 0;


    /// <summary>
    /// /AI
    /// </summary>
    public bool isAI;
    public int currentTarget;
    private Vector3 targetPoint;
    public float aiAccelspeed=1f;
    public float aiTurnspeed = 0.8f;
    public float aiReachPointRange = 5f;
    public float aiPointVariance = 3f;
    private float aiSpeedInput, aiSpeedMod;
    public float aiMaxTurn = 15f;
    public float resetCD = 2f;
    public float resetCounter;
    /// <summary>
    /// UI
    /// </summary>
    public float lapTime, bestLapTime;

    /// <summary>
    /// audio
    /// </summary>
    public AudioSource engineSound,skidSound;
    public float skidFade;
    
    void Start()
    {
        theRB.transform.parent = null;
        dragOnGround = theRB.drag;
        UIManager.instance.lapCounter.text = currentLap + "/" + RaceManager.instance.totalLaps;

        if (isAI)
        {
            targetPoint = RaceManager.instance.allCheckPoints[currentTarget].transform.position;
            RandomiseAITarget();
            aiSpeedMod = Random.Range(.8f, 1.1f);


        }
        resetCounter = resetCD + 2;
    }

    void Update()
    {
        if (RaceManager.instance.isStarting == false)
       { 
        lapTime += Time.deltaTime;
       

        if (!isAI)
        {
            var ts = System.TimeSpan.FromSeconds(lapTime);
            UIManager.instance.currentLaptTime.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);
            speedInput = 0;
            drifting = false;
            driftDir = 0;
            if (Input.GetAxis("Vertical") > 0)
            {
                speedInput = Input.GetAxis("Vertical") * forwardAccel;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                speedInput = Input.GetAxis("Vertical") * reverseAccel;

            }
            if (Input.GetButton("Jump") && grounded && turnInput != 0 && theRB.velocity.magnitude != 0)
            {
                drifting = true;
                driftDir = Input.GetAxis("Horizontal");
            }
            turnInput = Input.GetAxis("Horizontal");
            if(resetCounter > 0)
            {
                    resetCounter -= Time.deltaTime;
            }
            if(Input.GetKeyDown(KeyCode.R) && resetCounter <=0)
            {
                    ResetToTrack();
            }

        }
        else
        {
            targetPoint.y = transform.position.y;
            if(Vector3.Distance (transform.position,targetPoint) < aiReachPointRange)
            {
                SetNextAITarget();
            }

            Vector3 targetDirection = targetPoint - transform.position;

            float angle = Vector3.Angle(targetDirection, transform.forward);
            Vector3 localPos = transform.InverseTransformPoint(targetPoint);
          
            if(localPos.x < 0f)
            {
                angle = -angle;
            }

            turnInput = Mathf.Clamp(angle / aiMaxTurn, -1, 1);

            if(Mathf.Abs(angle) < aiMaxTurn)
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, 1f, aiAccelspeed);
                    drifting = false;
                   
            }
               if (Mathf.Abs(angle) > aiMaxTurn/2)
                {
                    AIDrift(angle);
                }
            else
                {
                 aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, aiTurnspeed, aiAccelspeed);
                    // AIDrift(angle);
                    // drifting = true;
                    drifting = false;


                }

                speedInput = aiSpeedInput * forwardAccel * aiSpeedMod;
        }
      /*  if(Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                new Vector3(0f, turnInput * turnStrength * Time.deltaTime* Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed) , 0f));
        }
        */
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn - 180), leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn ), rightFrontWheel.localRotation.eulerAngles.z);
        

       // transform.position = theRB.position;
        //particles

        emissionRate = Mathf.MoveTowards(emissionRate, 0, emissionFadeSpeed * Time.deltaTime);
        if(grounded && (Mathf.Abs(turnInput) > .5f || (theRB.velocity.magnitude < maxSpeed * .5f && theRB.velocity.magnitude != 0)))
        {
            emissionRate = maxEmission;
        }
        if(theRB.velocity.magnitude <= 0.25f)
        {
            emissionRate = 0;
        }
        for (int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;
            emissionModule.rateOverTime = emissionRate;
        }
        if (engineSound != null)
        {
            engineSound.pitch = 1f + ((theRB.velocity.magnitude / maxSpeed) * 2f);
        }
        if (skidSound != null)
        {
            if (Mathf.Abs(turnInput) > .5f)
            {
                skidSound.volume = 1f;
            }
            else
            {
                skidSound.volume = Mathf.MoveTowards(skidSound.volume, 0f, skidFade * Time.deltaTime);
            }
        }
        if (drifting)
        {
            foreach (TrailRenderer  T in skids)
            {
                T.emitting = true;
                skidSound.volume = 1f;
               skidSound.pitch = 1.5f;
            }
        }
        else
        {
            foreach (TrailRenderer T in skids)
            {
                T.emitting = false;
               // skidSound.volume = 0f;
                skidSound.pitch = 1f;
            }
        }
       }

    }
    private void FixedUpdate()
    {
        grounded = false;
        RaycastHit hit;

        Vector3 normalTarget = Vector3.zero;
        if(transform.rotation.eulerAngles.z !=0 )
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        }
        if (Physics.Raycast(groundRayPoint.position , -transform.up , out hit , groundRayLength , ground))
        {
            grounded = true;
            normalTarget = hit.normal;
        }
        if (Physics.Raycast(groundRayPointBalance.position, -transform.up, out hit, groundRayLength, ground))
        {
            grounded = true;
            normalTarget = (normalTarget + hit.normal ) /2f;
        }
        if (grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }

        if(grounded)
        {
            theRB.drag = dragOnGround;

            theRB.AddForce(transform.forward *speedInput * 1000);
        }
       
        else
        {
            theRB.drag = .1f;
            theRB.AddForce(-Vector3.up * gravity * 100f);
        }

        if (theRB.velocity.magnitude > maxSpeed)
        {
            theRB.velocity = theRB.velocity.normalized * maxSpeed;
        }

        transform.position = theRB.position;

        if (speedInput != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f));
            
        }
       
        if (grounded && drifting )
        {
            //theRB.drag = dragOnGround;
            // theRB.angularDrag = 1;
            
            if (driftDir > 0)
            {
                theRB.AddForce((transform.forward * speedInput * driftForce ) + (-transform.right * speedInput * driftSlide));

                //theRB.AddForce(new Vector3(speedInput * 700, 0f, speedInput * 500));
                // theRB.AddForce(-transform.right* speedInput * 700);
                //theRB.AddForce(Vector3 new())
                // theRB.AddForce(-transform.right * speedInput * 500);
                //theRB.AddForce(new Vector3(-transform.rotation.x * speedInput * 500, 0f, speedInput * 500));




            }

            else if (driftDir < 0)
            {
                theRB.AddForce((transform.forward * speedInput * driftForce) + (transform.right * speedInput * driftSlide));

                // theRB.AddForce(new Vector3(-speedInput * 700, 0f, speedInput * 500));
                // theRB.AddForce(transform.right * speedInput * 700);
                //theRB.AddForce(transform.right * speedInput * 500);
                //theRB.AddForce( new Vector3 (transform.rotation.x * speedInput * 500,0f, speedInput * 500));


            }
        }
    }
    public void CheckPointHit( int cpNumber)
    {
        if(cpNumber == nextCheckPoint)
        {
            nextCheckPoint++;

            if(nextCheckPoint == RaceManager.instance.allCheckPoints.Length)
            {
                nextCheckPoint = 0;
                LapCompleted();
            }
        }
        if(isAI)
        {
            if (cpNumber == currentTarget)
            {
                SetNextAITarget();
            }
        }
    }
    public void LapCompleted()
    {
        currentLap++;

        if(lapTime < bestLapTime || bestLapTime ==0)
        {
            bestLapTime = lapTime;
        }

        if (currentLap <= RaceManager.instance.totalLaps)
        {


            lapTime = 0f;

            if (!isAI)
            {
                UIManager.instance.lapCounter.text = currentLap + "/" + RaceManager.instance.totalLaps;
                var ts = System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);
            }
        }
        else
        {
            if(!isAI)
            {
                isAI = true;
                aiSpeedMod = 1f;

                targetPoint = RaceManager.instance.allCheckPoints[currentTarget].transform.position;
                RandomiseAITarget();

                var ts = System.TimeSpan.FromSeconds(bestLapTime);
                UIManager.instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);
               
                RaceManager.instance.Finishrace();
            }
        }
    }
    public void SetNextAITarget()
    {
        currentTarget++;
        if (currentTarget >= RaceManager.instance.allCheckPoints.Length)
        {
            currentTarget = 0;
        }
        targetPoint = RaceManager.instance.allCheckPoints[currentTarget].transform.position;
        RandomiseAITarget();
    }
    public void RandomiseAITarget()
    {
        targetPoint += new Vector3(Random.Range(-aiPointVariance, aiPointVariance),0, Random.Range(-aiPointVariance, aiPointVariance));
    }
    public void AIDrift(float angle)
    {
        drifting = true;
        if (angle > 0)
        {
            theRB.AddForce((transform.forward * aiSpeedInput * driftForce) + (-transform.right * aiSpeedInput * driftSlide / 2));
        }
        if (angle < 0)
        {
            theRB.AddForce((transform.forward * aiSpeedInput * driftForce) + (transform.right * aiSpeedInput * driftSlide / 2));

        }
    }
    public void ResetToTrack()
    {
        int pointToGo = nextCheckPoint - 1;
        if (pointToGo < 0)
        {
            pointToGo = RaceManager.instance.allCheckPoints.Length - 1;
        }
        transform.position = RaceManager.instance.allCheckPoints[pointToGo].transform.position;
        // transform.rotation =RaceManager.instance.allCheckPoints[pointToGo+1].transform.rotation;
        theRB.transform.position = transform.position;
        theRB.velocity = Vector3.zero;
        speedInput = 0f;
        turnInput = 0f;
        resetCounter = resetCD;
    }

}
