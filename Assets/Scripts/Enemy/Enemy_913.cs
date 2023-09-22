using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_913 : MonoBehaviour
{
    //---- HIDDEN VARIABLES 
    private bool debugView = true;              // whether to show the debug view and outputs to the console
    private Transform player;                     // player object
    private DetectionRange lookRange;           // object for detecting while patroling (triangle)
    private DetectionRange shootRange;           // object for detecting while shooting (circle)
    private EnemyProjectileAttack projAtt;       // projectile attack script
    private PlayerBehavior playerBehavior;
    
    private SpriteRenderer sprRend;               // sprite rendering of the enemy (for use with petrification coloring)


    //---- ENEMY PROPERTIES

    public float patrol_speed = 3.0f;           // speed of movement for the idle behavior
    public float chase_speed = 5.0f;          // speed of movement for the chase behavior
    private bool facingRight = true;           // is facing the right direction (rot=0) or left (rot=180) (default right, change if otherwise)
    private bool allowGrab = false;

    // AI behavior state
    [System.Serializable]
    public enum AIState{
        Idle,               // doing jack-shit lol
        Patrol,             // walking around, peacefully
        Chase,              // chasing after medusa
        Shoot,              // planted, trying to shoot medusa,
        Petrify,            // enemy is turned to stone - do not move
        Grabbed             // being grabbed by the player, stop moving
    }
    public AIState curState;                // current state of the enemy

    // patrol properties
    public List<Transform> patrolPts;       // points the AI moves to in patrol phase (goes in listed order before restarting at the top)
    private int patrolInd = 0;              // current point in the patrol points
    public float waitPatrolTime = 2.0f;     // how long to wait at the patrol point before going to the next
    public float patrolMinDist = 0.5f;      // minimum distance needed to reach a patrol point

    private LineRenderer patrolArea;         // detection area for patroling (for debug visuals)
    private SpriteRenderer shootArea;        // detection area for shooting (for debug visuals)

    [HideInInspector]
    public bool isFrozen = false;           // allows other scripts to check if the enemy is frozen

    // NOTE: Change these to sprites instead of colors
    public Color normalColor;               // normal color of the sprite
    public Color petrifyColor;              // color of the sprite after being petrified 


    //=================    GENERAL UNITY FUNCTIONS   ===================//


    // Start is called before the first frame update
    void Start(){
        // assign the range detectors if available
        Transform pr = transform.Find("PatrolRange");
        if(pr != null){
            lookRange = pr.GetComponent<DetectionRange>();
            patrolArea = pr.GetComponent<LineRenderer>();
        }

        // assign the range detector if available
        Transform sr = transform.Find("ShootRange");
        if(sr != null){
            shootRange = sr.GetComponent<DetectionRange>();
            shootArea = sr.GetComponent<SpriteRenderer>();
        }

        // assign the projectile attack script if available
        if(transform.Find("Projectile"))
            projAtt = transform.Find("Projectile").GetComponent<EnemyProjectileAttack>();

        playerBehavior = GameObject.Find("playerTest").GetComponent<PlayerBehavior>();        // finding player script

        sprRend = transform.GetComponent<SpriteRenderer>();         // assume that the enemy will always have a sprite renderer


        // set starting color to unpetrified
        if (curState != AIState.Petrify)
            sprRend.color = normalColor;
        else
            sprRend.color = petrifyColor;
    }

    // Update is called once per frame
    void Update(){
        // controls the AI behvaior (see the FSM diagram)
        switch(curState){
            case AIState.Idle:
                // doesn't matter, don't do nothing lol
                // maybe animation?

                // if medusa in range, chase her
                allowGrab = false;

                if (lookRange.medusaInSight)
                    curState = AIState.Chase;
                else if (Input.GetMouseButton(1) && allowGrab){
                    curState = AIState.Grabbed;
                }

                break;


            case AIState.Patrol:
                allowGrab = false;

                if (patrolPts.Count > 0)                     // if there are patrol points, go to them
                    GoToTarget(patrolPts[patrolInd], patrol_speed, patrolMinDist);
                else                                        // otherwise be idle
                    curState = AIState.Idle;

                // if medusa in range, chase her
                if(lookRange.medusaInSight)
                    curState = AIState.Chase;
                else if (Input.GetMouseButton(1) && allowGrab){
                    curState = AIState.Grabbed;
                }
                break;



            case AIState.Chase:
                // if not in range of Medusa, the move towards her
                if (shootRange.target != null && !InRangeX(transform, shootRange.target, 3.0f)){
                    GoToTarget(shootRange.target, chase_speed, 3.0f, true);
                }
                // if lost the target, go back to patrol
                else if(shootRange.target == null){
                    curState = AIState.Idle;
                    StartCoroutine(TranstitionState(AIState.Patrol, waitPatrolTime));
                }
                // otherwise try to shoot her
                else if(InRangeX(transform, shootRange.target, 3.0f)){
                    curState = AIState.Shoot;
                }
                else if (Input.GetMouseButton(1) && allowGrab){
                    curState = AIState.Grabbed;
                }

                break;




            case AIState.Shoot:
                // if not in range of Medusa, switch to chasing her
                if (shootRange.target != null && InRangeX(transform, shootRange.target, 3.0f)){
                    if(projAtt != null && projAtt.canFire){
                        //Debug.Log("fire!");
                        projAtt.FireBullet(shootRange.target);
                    }
                }
                // if lost the target, go back to patrol after waiting a bit
                else if(shootRange.target == null){
                    curState = AIState.Idle;
                    StartCoroutine(TranstitionState(AIState.Patrol, waitPatrolTime));
                }
                // otherwise try to shoot her
                else if(!InRangeX(transform, shootRange.target, 3.0f)){
                    curState = AIState.Chase;
                }
                else if (Input.GetMouseButton(1) && allowGrab){
                    curState = AIState.Grabbed;
                }
                break;


            case AIState.Petrify:
                // make the enemy completely still - no chance of being freed

                // freeze and change colors to petrification if not already
                // redundant, but just in case Petrify() failed, or the curState was changed elsewhere
                if (!isFrozen)
                {
                    sprRend.color = petrifyColor;
                    isFrozen = true;
                    allowGrab = false;
                }

                break;


            case AIState.Grabbed:
                if (Input.GetMouseButton(1) && allowGrab)
                {
                    this.gameObject.transform.position = new Vector3(playerBehavior.edges[0].x, playerBehavior.edges[0].y, 0);
                }
                else if (!allowGrab)
                {
                    Debug.Log("let go");

                    StartCoroutine(TranstitionState(AIState.Chase, 2f)) ;
                    allowGrab = true;
                    
                }
                
                break;
        }


        // render areas for debugging
        if(debugView && patrolArea){
            patrolArea.enabled = curState == AIState.Patrol || curState == AIState.Idle;
        }
        if(debugView && shootArea){
            shootArea.enabled = curState == AIState.Chase || curState == AIState.Shoot;
        }


    }


    //=================    HELPER FUNCTIONS   ===================//

    // checks if one point is close enough to another (X and Y axis)
    bool InRange2D(Transform a, Transform b, float d){
        return Vector2.Distance(a.position,b.position) < d;
    }

    // checks if one point is close enough to another (only on the X axis)
    bool InRangeX(Transform a, Transform b, float d){
        return Mathf.Abs(a.position.x - b.position.x) < d;
    }


    //=================    AI FUNCTIONS   ===================//

    // moves the enemy to a target position up until a certain distance with a speed (magnitude); option to ignore verticality
    void GoToTarget(Transform target, float magnitude, float minDist=0.0f, bool ignoreY=false){
        // not at the target yet, keep moving
        if((ignoreY && !InRangeX(transform,target,minDist)) || !InRange2D(transform,target,minDist)){
            // determine if the target is on the left or right (and face the appropriate direction)
            int dir = transform.position.x > target.position.x ? -1 : 1;
            SetDirection(dir);

            // move to target
            Move(magnitude);
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

    // turns the enemy into stone
    public void Petrified()
    {
        isFrozen = true;
        sprRend.color = petrifyColor;
        curState = AIState.Petrify;
    }

    //=================    CONTROL FUNCTIONS   ===================//

    // moves the character towards a specific direction (left: -1, right: 1, none: 0) with a specific magnitude (0-1)
    void Move(float magnitude){
        // transform.Translate((facingRight ? Vector2.right : Vector2.left) * magnitude * Time.deltaTime);
        transform.Translate(Vector2.right * magnitude * Time.deltaTime);
    }

    // sets the direction of the enemy (left = -1, right = everything else)
    void SetDirection(int dir){
        if(dir == -1 && facingRight){
            facingRight = false;
            transform.eulerAngles = new Vector3(0,180,0);
            // Debug.Log("to the left");
        }else if(dir != -1 && !facingRight){
            facingRight = true;
            transform.eulerAngles = new Vector3(0,0,0);
            // Debug.Log("to the right");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "grabSnake" && !isFrozen)
        {
            allowGrab = true;
            curState = AIState.Grabbed;
            Debug.Log(gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "grabSnake")
        {
            allowGrab = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && curState == AIState.Grabbed)
        {
            Debug.Log("enemy died");
            Destroy(gameObject);
        }
    }
    /*
    IEnumerator DyingDelay()
    {

    }*/
}
