using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarHits : MonoBehaviour
{
   // public CarControllerN car;
    public GameObject hiteffect;
    public AudioSource hit;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer !=7)
        {
            GameObject effectClone = Instantiate(hiteffect, collision.GetContact(0).point, Quaternion.identity);//car.transform.rotation);
            Destroy(effectClone, 1f);
            hit.Stop();
            hit.pitch = Random.Range(0.7f, 1.3f);
            hit.Play();


        }
        

    }

    

}
