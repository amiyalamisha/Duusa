using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_913 : MonoBehaviour
{
    //---- HIDDEN VARIABLES 
    private Transform player;                     // player object
    private DetectionRange detectRange;           // object for detecting


    //---- ENEMY PROPERTIES

    public float patrol_speed = 3.0f;           // speed of movement for the idle behavior
    public float chase_speed = 5.0f;          // speed of movement for the chase behavior

    // AI behavior state
    [System.Serializable]
    public enum AIState{
        Idle,               // doing jack-shit lol
        Patrol,             // walking around, peacefully
        Chase,              // chasing after medusa
        Shoot               // planted, trying to shoot medusa
    }
    public AIState curState;                // current state of the enemy

    // patrol properties
    public List<Transform> patrolPts;       // points the AI moves to in patrol phase (goes in listed order before restarting at the top)
    private int patrolInd = 0;              // current point in the patrol points
    public float waitPatrolTime = 2.0f;           // how long to wait at the patrol point before going to the next


    //=================    GENERAL UNITY FUNCTIONS   ===================//


    // Start is called before the first frame update
    void Start(){

        // assign the range detector if available
        if(transform.Find("DetectionRange"))
            detectRange = transform.Find("DetectionRange").GetComponent<DetectionRange>();

    
    }

    // Update is called once per frame
    void Update(){
        // controls the AI behvaior (see the FSM diagram)
        switch(curState){
            case AIState.Idle:
                break;



            case AIState.Patrol:
                if(patrolPts.Count > 0)                     // if there are patrol points, go to them
                    GoToTarget(patrolPts[patrolInd], patrol_speed, 0.3f);
                else                                        // otherwise be idle
                    curState = AIState.Idle;
                break;



            case AIState.Chase:
                break;




            case AIState.Shoot:
                break;
        }
    }


    //=================    AI FUNCTIONS   ===================//

    // moves the enemy to a target position up until a certain distance with a speed (magnitude); option to ignore verticality
    void GoToTarget(Transform target, float magnitude, float minDist=0.0f, bool ignoreY=false){
        // not at the target yet, keep moving
        if((ignoreY && Mathf.Abs(transform.position.x - target.position.x) > minDist) || Vector2.Distance(transform.position, target.position) > minDist){
            // determine if the target is on the left or right
            int dir = 0;
            dir = transform.position.x > target.position.x ? -1 : 1;

            // move to target
            Move(dir,magnitude);
        }
        // otherwise, wait at the point, and start again
        else if(curState == AIState.Patrol){
            StartCoroutine(NextPatrolPt());
        }
    }

    // transition to a new AI state after a certain amount of time
    IEnumerator TranstitionState(AIState nextState, float transTime){
        yield return new WaitForSeconds(transTime);
        curState = nextState;
    }

    // delay before going to the next patrol point
    IEnumerator NextPatrolPt(){
        curState = AIState.Idle;
        yield return new WaitForSeconds(waitPatrolTime);
        patrolInd+=1;
        patrolInd %= patrolPts.Count;
        curState = AIState.Patrol;
    }

    //=================    CONTROL FUNCTIONS   ===================//

    // moves the character towards a specific direction (left: -1, right: 1, none: 0) with a specific magnitude (0-1)
    void Move(int hor_dir, float magnitude){
        transform.Translate(Vector2.right * hor_dir * magnitude * Time.deltaTime);
    }

    // shoots at a target (trigger event)
    void Shoot(){
        return;
    }


}
