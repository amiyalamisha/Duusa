using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// like DetectionRange.cs but works for a specified tag object or list of tags

public class DetectionRange_Layer : MonoBehaviour
{
    public LayerMask checkLayer;
    private int checkLayerInt = -1;
    public bool nearLayer = false;      // check if a target is currently in sight

    void Start(){
        checkLayerInt = (int) Mathf.Log(checkLayer.value, 2);   // convert to integer
    }

    // object comes in range
    void OnTriggerEnter2D(Collider2D c){
        // Debug.Log(LayerMask.LayerToName(c.gameObject.layer));
        if(checkLayerInt == c.gameObject.layer){   // if target is in range, assign it as the current target
            nearLayer = true;
        }
    }

    void OnTriggerStay2D(Collider2D c){
        // Debug.Log(LayerMask.LayerToName(c.gameObject.layer));
        if(checkLayerInt == c.gameObject.layer){   // if target is in range, assign it as the current target
            nearLayer = true;
        }
    }

    // object leaves range
    void OnTriggerExit2D(Collider2D c){
        if(checkLayerInt == c.gameObject.layer){   // if target is out of range, remove it as the target
            nearLayer = false;
        }
    }
}
