using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProto_1017 : MonoBehaviour
{

    private Rigidbody2D rb;
    public Vector2 endPt;
    private float timeLerping = 0.0f;
    private bool inAir = true;

    public float airSpeed = 12.0f;
    public float groundSpeed = 3.0f;
    public float lerpSmooth = 0.01f;
    public float bufferAmt = 1.5f;
    public float jumpForce = 3.0f;


    public LayerMask groundLayer;


    public bool debug_inrange = false;

    private Vector2 velocity = Vector2.zero;


    private SpriteRenderer sprRend;
    public Color groundColor;
    public Color airColor;

    // Start is called before the first frame update
    void Start(){
        rb = transform.GetComponent<Rigidbody2D>();
        endPt = transform.position;
        groundLayer = 1 << LayerMask.NameToLayer("Block");
        sprRend = transform.GetComponent<SpriteRenderer>();
    }

    void Update(){
         // left mouse button change position to move to
        if(Input.GetMouseButton(0)){
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            endPt = new Vector2(mousePos.x,mousePos.y);
        }

        // toggle being in the air or on the ground
        if(Input.GetMouseButtonDown(1)){
            inAir = !inAir;

            // turn gravity on
            if(!inAir){
                rb.gravityScale = 2.0f;
                sprRend.color = groundColor;
            }
            // turn gravity off
            else{
                endPt = rb.position;
                rb.gravityScale = 0.0f;
                sprRend.color = airColor;
                rb.velocity = Vector2.zero;
            }
        }


        // move towards the currently selected point on lmb
        if(inAir){
            AirMove(endPt);
        }else{
            GroundMove();
        }
    }

    // moves the character towards a specific direction (left: -1, right: 1, none: 0) with a specific magnitude (0-1)
    void AirMove(Vector2 toPos){
        // make sure the gravity is off
        rb.gravityScale = 0.0f;
        sprRend.color = airColor;

        Vector2 curPos = new Vector2(rb.position.x, rb.position.y);

        // don't move if already there
        if(toPos == null || InRange2D(curPos, endPt, 0.3f)){
            timeLerping = 0.0f;
            return;
        }
        
        // send out distance ray in the direction to detect if near a wall
        // forward
        bool closeToWall = false;
        Vector2 forward = (toPos-curPos).normalized * bufferAmt; 
        RaycastHit2D f_ray = Physics2D.Raycast(curPos, forward, bufferAmt, groundLayer);
        if (f_ray){
            closeToWall = true;
            Debug.DrawRay(curPos, forward * f_ray.distance, Color.red);
        }else{
            Debug.DrawRay(curPos, forward * bufferAmt, Color.green);
        }

        // if within a certain distance, lerp to it
        Vector2 movement;
        velocity = rb.velocity;
        if(InRange2D(curPos,endPt,0.3f) || closeToWall){
            movement = Vector2.Lerp(curPos,toPos,Time.deltaTime/(lerpSmooth*airSpeed));
            // movement = Vector2.MoveTowards(curPos,toPos,airSpeed*Time.deltaTime) * decel; 
            debug_inrange = true;
        }
        // otherwise move like normal
        else{
            movement = Vector2.MoveTowards(curPos,toPos,airSpeed*Time.deltaTime);
            debug_inrange = false;
            timeLerping = 0.0f;
        }

        rb.MovePosition(movement);
        timeLerping += Time.deltaTime;
    }

    void GroundMove(){
        // horizontal movement
        float hor = Input.GetAxisRaw("Horizontal") * groundSpeed * Time.deltaTime; 
        transform.Translate(hor * Vector2.right);

        // allow jumps
        if(Input.GetKeyDown(KeyCode.W)){
            rb.AddForce(Vector3.up*jumpForce*rb.mass*100);
        }
    }

    // checks if one point is close enough to another (X and Y axis)
    bool InRangeT2D(Transform a, Transform b, float d=0.2f){
        return Vector2.Distance(a.position,b.position) < d;
    }
    bool InRange2D(Vector2 a, Vector2 b, float d=0.2f){
        return Vector2.Distance(a,b) < d;
    }

}
