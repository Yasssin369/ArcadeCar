using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public static TopDownCamera instance;
    public CarControllerN Target;
    private Vector3 offsetDir;

    public float minDist, maxDist;
    private float activeDist;
    public float damper = 1f;

    public Transform startTargetOffset;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        offsetDir = transform.position - startTargetOffset.position;
        activeDist = minDist;
        offsetDir.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        activeDist = minDist + ((maxDist - minDist) * (Target.theRB.velocity.magnitude / Target.maxSpeed));
        transform.position = Target.transform.position + (offsetDir * activeDist );
       // transform.position = Vector3.Lerp(transform.position, Target.transform.position, damper * Time.fixedDeltaTime);
    }
}
