using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProto_1025 : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;                                         // rigidbody physics collider   
    private SpriteRenderer sprRend;                                 // sprite renderer
    private SpringJoint2D spring;                                   // spring joint 2d for moving to the target position
    private Vector2 targPt;                                         // target position the player will move to

    [Header("Platform Settings")]
    [SerializeField] private readonly float groundSpeed = 3.0f;     // speed at which medusa moves while on the ground
    [SerializeField] private readonly float jumpForce = 3.0f;       // force to apply for jumping

    [Header("Snake Settings")]
    public LayerMask grappleMask;       // layer for all grapplable surfaces
    public bool togSnakes = true;                                   // toggle the snakes mid-air movement
    [SerializeField] private float maxSnakeRange = 10.0f;           // the maximum distance allowed from the player for the snakes to grapple from (should be larger than target range)
    [SerializeField] private float maxTargetRange = 5.0f;           // the maximum distance allowed from the player for the target position
    [SerializeField] private List<LineRenderer> snakeLines;         // snakes to spring from medusa (aesthetic purposes only)
    public bool canExtend = false;


    [Header("Target Settings")]
    [SerializeField] private bool showCrosshair = true;             // whether to show the aiming crosshair
    public GameObject targetSpr;                                    // the crosshair targeting value
    private SpriteRenderer targetSprRend;                           // sprite rendering of the crosshair
    [SerializeField] private LineRenderer targetLine;               // line to show where the player is able to move to
    [SerializeField] private Color validColor;                      // color to indicate that it is valid to move to a position         (green?)
    [SerializeField] private Color invalidColor;                    // color to indicate that it is not valid to move to a position  (red?)

    [Header("Sprite Settings")]
    public Color snakeMedusa;
    public Color normalMedusa;

    [Header("Spring Settings")]
    [SerializeField] private float targDamp = 0.5f;                 // target dampening value for the spring joint
    [SerializeField] private float targDist = 0.005f;               // proximity to reach target with the minimum spring length
    [SerializeField] private float targFreq = 0.5f;                 // how many "coils" of the spring

    // setup
    void Start(){
        rb = transform.GetComponent<Rigidbody2D>();
        sprRend = transform.GetComponent<SpriteRenderer>();
        spring = transform.GetComponent<SpringJoint2D>();

        if(targetSpr)
            targetSprRend = targetSpr.GetComponent<SpriteRenderer>();

        if(togSnakes){
            sprRend.color = snakeMedusa;
            rb.gravityScale = 0;
            spring.enabled = true;
        }else{
            sprRend.color = normalMedusa;
            rb.gravityScale = 1;
            spring.enabled = false;
        }
    }


    // Update is called once per frame
    void Update(){
        if(togSnakes){
            // left mouse button click change position to move to
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);                     // get the world mouse position
            Vector2 playerPos = new Vector2(transform.position.x, transform.position.y);                // get the player position
            Vector2 maxPt = playerPos + Vector2.ClampMagnitude(mousePos - playerPos,maxTargetRange);             // limit the target to the range

            // check if the target is a valid place to move to and within snake range
            RaycastHit2D validPos = CanExtendSnake(maxPt);

            // set the target position to move to if valid and extend snakes to nearby contact point
            if(Input.GetMouseButton(0) && togSnakes && canExtend){
                targPt = maxPt;
                AddSnakes(validPos.point);
            }

            // show the crosshair and the line renderer
            if(showCrosshair){
                // change color of crosshair and line based on validity
                Color color2use = canExtend ? validColor : invalidColor;
                targetLine.material.SetColor("_Color", color2use);
                targetSprRend.color = color2use;

                // set the positions of the target line and the crosshair
                targetSpr.transform.position = maxPt;       // set the crosshair position to the target (maxed by range distance)
                targetLine.SetPositions(MakeLinePoints(new Vector2[]{playerPos,maxPt}));
            }
        }

        // toggle whether medusa is using her grapple snakes or not
        if(Input.GetMouseButtonDown(1)){
            // set the values to alternates
            togSnakes = !togSnakes;
            spring.enabled = togSnakes;
            targetSpr.SetActive(togSnakes);
            targetLine.enabled = togSnakes;
            sprRend.color = togSnakes ? snakeMedusa : normalMedusa;

            // reset values
            targPt = transform.position;
            rb.gravityScale = togSnakes ? 0 : 1;
        }

        // move to a point with snakes or do the platforming
        if(togSnakes){
            Snakes();
        }else{
            GroundMove();
        }

    }

    // use the spring to move to a point (snakes)
    private void Snakes(){
        // setup the spring
        spring.dampingRatio = targDamp;
        spring.distance = targDist;
        spring.frequency = targFreq;
        spring.connectedAnchor = targPt;
    }

    // move with gravity on the platform (normal)
    private void GroundMove(){
        // horizontal movement
        float hor = Input.GetAxisRaw("Horizontal") * groundSpeed * Time.deltaTime; 
        transform.Translate(hor * Vector2.right);

        // allow jumps
        if(Input.GetKeyDown(KeyCode.W)){
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
    private void AddSnakes(Vector2 contactPt){

    }


    // checks if one point is close enough to another (X and Y axis)
    private bool InRangeT2D(Transform a, Transform b, float d=0.2f){
        return Vector2.Distance(a.position,b.position) < d;
    }
    private bool InRange2D(Vector2 a, Vector2 b, float d=0.2f){
        return Vector2.Distance(a,b) < d;
    }
}


