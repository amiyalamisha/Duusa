using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private CheckpointSystem mainCheck;

    // Start is called before the first frame update
    void Start(){
        mainCheck = GameObject.Find("MasterControl").GetComponent<CheckpointSystem>();
    }

    // when player touches checkpoint
    public void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag == "Player")
            mainCheck.SetCheckpoint(this.gameObject);
    }
}
