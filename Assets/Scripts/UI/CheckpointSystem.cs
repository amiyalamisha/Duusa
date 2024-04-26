using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    // recorded data
    private GameObject player;               // player object in the scene
    public List<string> recorded_tags;       // list of GameObject tags to keep track of in the scene
    private List<GameObject> all_objs;       // all objects (enemies, items) in the scene
    private bool[] active_objs;              // record which objects (enemies, items) are active in the scene when loading from checkpoint
   
    // checkpoints
    public List<GameObject> checkpoints;            // list of checkpoints in the scene (including the spawn point)
    private int activeCheckpoint = 0;               // which of the checkpoints to reload the scene from
    private bool[] saved_active;                    // activation at save/check point
    public Color activePtColor = Color.yellow;      // color for active checkpoint
    public Color inactivePtColor = Color.white;     // color for inactive checkpoint

    // Start is called before the first frame update
    void Start() {
        // find and save the (first) player object
        player = GameObject.FindGameObjectsWithTag("Player")[0];

        // find all savable objects
        all_objs = new List<GameObject>();
        foreach(string tag in recorded_tags){
            foreach(GameObject g in GameObject.FindGameObjectsWithTag(tag)){
                all_objs.Add(g);
            }
        }
        // set activation status based on whether it's in the scene at start
        active_objs = new bool[all_objs.Count];
        for(int i=0;i<all_objs.Count;i++){
            active_objs[i] = all_objs[i].activeSelf;
        }

        // set the first checkpoint if they're available
        if(checkpoints.Count > 0)
            SetCheckpoint(checkpoints[0]);
    }

    // saves the game state and object states
    public void SaveState(){
        // set activation status based on whether it's in the scene at start
        saved_active = new bool[all_objs.Count];
        for(int i=0;i<active_objs.Length;i++){
            saved_active[i] = active_objs[i];
        }
    }

    // sets the current checkpoint
    public void SetCheckpoint(GameObject c){
        for(int i=0;i<checkpoints.Count;i++){
            // set checkpoint to active
            if(c == checkpoints[i]){
                activeCheckpoint = i;
                checkpoints[i].transform.GetComponent<SpriteRenderer>().color = activePtColor;
                Debug.Log(i);
            }else{
                checkpoints[i].transform.GetComponent<SpriteRenderer>().color = inactivePtColor;
            }
        }
        SaveState();
        // Debug.Log("Checkpoint #" + (activeCheckpoint+1) + " reached!");
    }

    // loads from the last checkpoint and re/de activates objects
    public void LoadCheckpoint(){
        //reactivate objects
        for(int i=0;i<all_objs.Count;i++){
            all_objs[i].SetActive(saved_active[i]);
            active_objs[i] = saved_active[i];
        }

        //put player position back at checkpoint
        Vector2 newPos = checkpoints[activeCheckpoint].transform.position;
        player.transform.position = newPos;
        player.transform.GetComponent<MoveProto_1025>().ResetSnakes(newPos);
    }
}
