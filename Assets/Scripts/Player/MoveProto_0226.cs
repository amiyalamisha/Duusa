using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProto_0226 : MonoBehaviour
{
    // Components
    private PlayerBehavior_Abstract playerBehavior;
    private Rigidbody2D rb;                                         // rigidbody physics collider   
    private SpriteRenderer sprRend;                                 // sprite renderer
    private SpringJoint2D spring;                                   // spring joint 2d for moving to the target position
    private BoxCollider2D boxCollider;                              // character box collider
    private Vector2 targPt;                                         // target position the player will move to
    [SerializeField] private Vector2 orgBoxColl;                                     // original size for player box collieder
    
    // global script
    private MenuControl mainMenu;                                   // menu object of the game

    [Header("Controls")]
    [SerializeField] private KeyCode snakeToggleBtn = KeyCode.Space;       // toggle using snakes
    [SerializeField] private KeyCode jumpBtn = KeyCode.W;                      // jump button
    

    [Header("Platform Settings")]
    [SerializeField] private readonly float groundSpeed = 3.0f;     // speed at which medusa moves while on the ground
    [SerializeField] private readonly float jumpForce = 3.0f;       // force to apply for jumping

    [Header("Snake Settings")]
    public LayerMask grappleMask;       // layer for all grapplable surfaces
    public bool togSnakes = true;                                   // toggle the snakes mid-air movement
    [SerializeField] private float maxSnakeRange = 10.0f;           // the maximum distance allowed from the player for the snakes to grapple from (should be larger than target range)
    [SerializeField] private float maxTargetRange = 5.0f;           // the maximum distance allowed from the player for the target position
    public Transform mainSnakePos;
    [SerializeField] private List<GameObject> snakeLines;           // snakes to spring from medusa (aesthetic purposes only)
    public bool canExtend = false;                                  // able to project snakes
    [Range(0,90)]
    [SerializeField] private float snakeSpread = 30.0f;             // angle difference
    private Vector2[] snakeAnchors;


    [Header("Target Settings")]
    [SerializeField] private bool showCrosshair = true;             // whether to show the aiming crosshair
    public GameObject targetSpr;                                    // the crosshair targeting value
    private SpriteRenderer targetSprRend;                           // sprite rendering of the crosshair
    [SerializeField] private LineRenderer targetLine;               // line to show where the player is able to move to
    [SerializeField] private Color validColor;                      // color to indicate that it is valid to move to a position         (green?)
    [SerializeField] private Color invalidColor;                    // color to indicate that it is not valid to move to a position  (red?)
    private RaycastHit2D contactPt;
    private Vector2 permaContactPt;

    [Header("Sprite Settings")]
    public Color snakeMedusa;
    public Color normalMedusa;

    [Header("Spring Settings")]
    [SerializeField] private float targDamp = 0.5f;                 // target dampening value for the spring joint
    [SerializeField] private float targDist = 0.005f;               // proximity to reach target with the minimum spring length
    [SerializeField] private float targFreq = 0.5f;                 // how many "coils" of the spring
    [SerializeField] private float fallGrav = 0.1f;                 // how much to fall by while in mid-air

    // setup
    void Start(){
        rb = transform.GetComponent<Rigidbody2D>();
        playerBehavior = transform.GetComponent<PlayerBehavior_Abstract>();
        sprRend = transform.GetComponent<SpriteRenderer>();
        spring = transform.GetComponent<SpringJoint2D>();
        boxCollider = transform.GetComponent<BoxCollider2D>();
        orgBoxColl = new Vector2(0.65f, 1.79f);
        targPt = transform.position;

        if(targetSpr)
            targetSprRend = targetSpr.GetComponent<SpriteRenderer>();

        if(togSnakes){
            //sprRend.color = snakeMedusa;
            rb.gravityScale = fallGrav;
            spring.enabled = true;
        }else{
            //sprRend.color = normalMedusa;
            rb.gravityScale = 1;
            spring.enabled = false;
        }

        snakeAnchors = new Vector2[snakeLines.Count];

        // if the master control script is in the scene, set the menu
        if(GameObject.Find("MasterControl")){
            mainMenu = GameObject.Find("MasterControl").GetComponent<MenuControl>();
        }

        HideSnakes();
    }


    // Update is called once per frame
    void Update(){
        if(togSnakes){
            // left mouse button click change position to move to
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);                     // get the world mouse position
            Vector2 playerPos = new Vector2(transform.position.x, transform.position.y);                // get the player position
            Vector2 maxPt = playerPos + Vector2.ClampMagnitude(mousePos - playerPos,maxTargetRange);             // limit the target to the range

            // check if the target is a valid place to move to and within snake range
            contactPt = CanExtendSnake(maxPt);

            // set the target position to move to if valid and extend snakes to nearby contact point
            if(Input.GetMouseButtonDown(0) && togSnakes && canExtend){
                PlayerBehavior_1114.instance.isSwinging = true;
                boxCollider.size = new Vector2(0.65f, 1.25f);
                targPt = maxPt;
                permaContactPt = new Vector2(contactPt.point.x,contactPt.point.y);
                SetSnakes(permaContactPt);
            }

            // show the crosshair and the line renderer
            if(showCrosshair && (mainMenu == null || !mainMenu.gamePaused)){
                // change color of crosshair and line based on validity
                Color color2use = canExtend ? validColor : invalidColor;
                targetLine.material.SetColor("_Color", color2use);
                targetSprRend.color = color2use;

                // set the positions of the target line and the crosshair
                targetSpr.transform.position = maxPt;       // set the crosshair position to the target (maxed by range distance)
                targetLine.SetPositions(MakeLinePoints(new Vector2[]{playerPos,maxPt}));
            }

            if(canExtend){
                ShowSnakes();
            }
        }
        else
        {
            PlayerBehavior_1114.instance.isSwinging = false;
            boxCollider.size = orgBoxColl;
        }

        // toggle whether medusa is using her grapple snakes or not
        if(Input.GetKeyDown(snakeToggleBtn)){
            // set the values to alternates
            togSnakes = !togSnakes;
            spring.enabled = togSnakes;
            targetSpr.SetActive(togSnakes);
            targetLine.enabled = togSnakes;
            //sprRend.color = togSnakes ? snakeMedusa : normalMedusa;

            // reset values
            targPt = transform.position;
            rb.gravityScale = togSnakes ? fallGrav : 1;

            if(!togSnakes)
                HideSnakes();
            else
                SetSnakes(new Vector2(transform.position.x, transform.position.y));
        }

        // move to a point with snakes or do the platforming
        if(togSnakes){
            Snakes();
        }
        if(Input.GetKeyDown(KeyCode.Space)){
            GroundMove();
        }

        // debug
        // if(Input.GetKeyDown(KeyCode.Z)){
        //     snakeLines[0].GetComponent<SnakeRope>().HideSnakes();
        // }

    }

    // use the spring to move to a point (snakes)
    private void Snakes(){
        // setup the spring
        spring.dampingRatio = targDamp;
        spring.distance = targDist;
        spring.frequency = targFreq;
        spring.connectedAnchor = targPt;

        // set direction
        if(playerBehavior && Mathf.Round(rb.velocity.x) != 0){
            playerBehavior.direction = rb.velocity.x > 0 ? "right" : "left";
        }
    }

    // move with gravity on the platform (normal)
    private void GroundMove(){
        // horizontal movement
        float hor = Input.GetAxisRaw("Horizontal") * groundSpeed * Time.deltaTime; 
        transform.Translate(hor * Vector2.right);

        // set the direction
        if(playerBehavior && hor != 0)
            playerBehavior.direction = hor > 0 ? "right" : "left";

        // allow jumps
        if(Input.GetKeyDown(jumpBtn)){
            rb.AddForce(Vector3.up*jumpForce*rb.mass*100);
        }

        
    }

    // makes 2d vector points to 3d for use with the line renderer
    private Vector3[] MakeLinePoints(Vector2[] p){
        Vector3[] p3 = new Vector3[p.Length];
        for(int i=0;i<p.Length;i++){
            p3[i] = new Vector3(p[i].x,p[i].y,transform.position.z);
        }
        return p3;
    }


    // determine if a point is within range of platform to extend to
    private RaycastHit2D CanExtendSnake(Vector2 pt){
        // RaycastHit2D hit;
        Vector2 targDir = (pt - (Vector2)transform.position).normalized;
        RaycastHit2D contact = Physics2D.Raycast(transform.position,targDir,maxSnakeRange,grappleMask);
        if(contact)
            canExtend = true;
        else
            canExtend = false;
        return contact;
    }


    // makes snake line renderers (like grappling hooks) for aesthetic purposes only
    private void SetSnakes(Vector2 contactPt){
        Vector2 startPos = mainSnakePos.localPosition;
        Vector2 ctPt2 = mainSnakePos.InverseTransformPoint(contactPt);
        Vector2 dirVec = (ctPt2 - startPos).normalized;

        // straight to the contact point
        // snakeLines[0].SetPosition(0, startPos);
        // snakeLines[0].SetPosition(1, ctPt2);

        // extends the rest of the snakes in random directions
        int modAngle = (int)Math.Floor(snakeSpread*2 / (snakeLines.Count-1));

        for(int i=0;i<snakeLines.Count;i++){
            // change the direction of the angle
            float newAng = -snakeSpread + (i)*modAngle;
            Quaternion rot = Quaternion.Euler(0,0,newAng);
            Vector2 altVec = rot * dirVec;

            // extend to new contact point (if able to attach)
            RaycastHit2D newHit = Physics2D.Raycast(transform.position,altVec,maxSnakeRange,grappleMask);
            if(newHit){
                snakeLines[i].GetComponent<SnakeRope>().GenerateSnakeGrapple(newHit.point);
                snakeAnchors[i] = newHit.point;
            }
        }

    }

    // shows the snakes in the position
    private void ShowSnakes(){
        // Vector2 startPos = mainSnakePos.localPosition;
        for(int i=0;i<snakeLines.Count;i++){
            snakeLines[i].SetActive(true);
            // SetSnakes(permaContactPt);
            snakeLines[i].GetComponent<SnakeRope>().UpdateSnakeRope();
        }
    }

    private void HideSnakes(){
        for(int i=0;i<snakeLines.Count;i++){
            snakeLines[i].GetComponent<SnakeRope>().HideSnakes();
            snakeLines[i].SetActive(false);
        }

        // hide the objects
    }


    // checks if one point is close enough to another (X and Y axis)
    private bool InRangeT2D(Transform a, Transform b, float d=0.2f){
        return Vector2.Distance(a.position,b.position) < d;
    }
    private bool InRange2D(Vector2 a, Vector2 b, float d=0.2f){
        return Vector2.Distance(a,b) < d;
    }

    void OnDrawGizmos(){
        // draw the contact point
        if(canExtend){
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(contactPt.point,0.2f);
        }

        // draw the range of the snakes
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxSnakeRange);
    }
}


