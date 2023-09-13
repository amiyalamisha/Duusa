using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionRange : MonoBehaviour
{
    public bool medusaInSight = false;      // check if medusa is currently in sight
    public Transform target;                // current target object of the range detector


    // object comes in range
    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag == "Player"){   // if medusa is in range, assign her as the current target
            medusaInSight = true;
            target = c.transform;
        }
    }

    // object leaves range
    void OnTriggerExit2D(Collider2D c){
        if(c.gameObject.tag == "Player"){   // if medusa is out of range, remove her as the target
            medusaInSight = false;
            target = null;
        }
    }
}
