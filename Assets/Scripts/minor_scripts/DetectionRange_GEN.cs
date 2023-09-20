using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// like DetectionRange.cs but works for a specified tag object or list of tags

public class DetectionRange_GEN : MonoBehaviour
{
    public List<string> allowedTags;
    public bool targetInSight = false;      // check if a target is currently in sight
    public Transform target;                // current target object of the range detector


    // object comes in range
    void OnTriggerEnter2D(Collider2D c){
        if(allowedTags.Contains(c.gameObject.tag)){   // if target is in range, assign it as the current target
            targetInSight = true;
            target = c.transform;
        }
    }

    // object leaves range
    void OnTriggerExit2D(Collider2D c){
        if(c.transform == target){   // if target is out of range, remove it as the target
            targetInSight = false;
            target = null;
        }
    }
}
