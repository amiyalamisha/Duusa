using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProto_1025 : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;
    private SpriteRenderer sprRend;
    private SpringJoint2D spring;
    private Vector2 targPt;

    [Header("Platform Settings")]
    [SerializeField] private float groundSpeed = 3.0f;
    [SerializeField] private float jumpForce = 3.0f;

    [Header("Snake Settings")]
    public bool togSnakes = true;
    [SerializeField] private float maxSnakeRange = 10.0f;           // the maximum distance allowed from the player for the target position

    [Header("Sprite Settings")]
    public Color snakeMedusa;
    public Color normalMedusa;

    [Header("Spring Settings")]
    [SerializeField] private float targDamp = 0.5f;
    [SerializeField] private float targDist = 0.005f;
    [SerializeField] private float targFreq = 0.5f;


    [Header("Extra Debug")]
    public GameObject targetSpr;


    void Start(){
        rb = transform.GetComponent<Rigidbody2D>();
        sprRend = transform.GetComponent<SpriteRenderer>();
        spring = transform.GetComponent<SpringJoint2D>();

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
         // left mouse button click change position to move to
        if(togSnakes){
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);     // get the world mouse position
            Vector2 playerPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 maxPt = playerPos + Vector2.ClampMagnitude(mousePos - playerPos,maxSnakeRange);                        // limit the target to the range

            // set the target position to move to
            if(Input.GetMouseButton(0))
                targPt = maxPt;

            // show the crosshair if available
            if(targetSpr)
                targetSpr.transform.position = maxPt;
        }

        // toggle whether medusa is using her grapple snakes or not
        if(Input.GetMouseButtonDown(1)){
            // activate the spring
            togSnakes = !togSnakes;
            spring.enabled = togSnakes;

            // reset values
            targPt = transform.position;
            rb.gravityScale = togSnakes ? 0 : 1;

            // change the sprite renderer color and reset target point back to medusa
            if(togSnakes){
                sprRend.color = snakeMedusa;
                targetSpr.SetActive(true);
            }else{
                sprRend.color = normalMedusa;
                targetSpr.SetActive(false);
            }
        }

        // move to a point if able to
        if(togSnakes && !InRange2D(transform.position,targPt,targDist)){
            Snakes();
        }else{
            GroundMove();
        }

    }

    // use the spring to move to a point (snakes)
    void Snakes(){
        // setup the spring
        spring.dampingRatio = targDamp;
        spring.distance = targDist;
        spring.frequency = targFreq;
        spring.connectedAnchor = targPt;
    }

    // move with gravity on the platform (normal)
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
