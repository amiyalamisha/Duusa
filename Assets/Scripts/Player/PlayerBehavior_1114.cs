using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBehavior_1114 : PlayerBehavior_Abstract
{
    // Components
    private Rigidbody2D rb;                                         // rigidbody physics collider   
    private SpriteRenderer sprRend;                                 // sprite renderer   


    [Header("General Properties")]
    public Camera cam;
    public LineRenderer grabbingSnakes;
    EdgeCollider2D edgeCollider;
    public Animator playerAnim;

    [Header("Health Properties")]
    public int maxHealth = 3;               // maximum possible health of the player
    public int curHealth = 3;               // current health of the player
    public float healthGraceAmt = 0.6f;     // invulnerability time before can take damage again
    public bool canHurt = true;             // flag for whether the player is vulnerable to attacks
    private Color origColor;                // stores the original color to revert back to
    public Color hurtColor;                 // color for when player is hurt and grace period is activated
    public HealthUI healthGUI;              // GUI for the health 

    // petrification properties
    [Header("Petrification Properties")]
    private Transform petRay;
    private Vector3 petPos;
    private Vector3 petRot;
    private SpriteRenderer petrifyRaySpr;           // sprite render for the petrification ray (toggles on and off)
    private DetectionRange_GEN petrifyRayDetect;    // detection script for whether an enemy is in the petrification area
    private bool petrifyRayOn = false;              // boolean for if the ray is activated  
    public float petrifyRayOnTime = 0.3f;           // how long the ray stays activated for
    public float petrifyRayCooldown = 2.0f;         // how long before the ray can be used again
    private bool canPetrify = true;                 // flag for whether or not the petrification ray can be used

    [Header("Snake Grapple Properties")]
    [SerializeField] private float grabSnakeMax = 5;    // min distance of snakes

    public int maxPoints = 3;       // how many movement snakes can you shoot max
       
    private List<Vector2> points = new List<Vector2>();     // all working movement snakes

    // Start is called before the first frame update
    void Start()
    {
        // inherited properties from abstract class
        edges = new List<Vector2>();


        sprRend = GetComponent<SpriteRenderer>();
        origColor = sprRend.color;
        rb = GetComponent<Rigidbody2D>();
        edgeCollider = grabbingSnakes.GetComponent<EdgeCollider2D>();

        grabbingSnakes.enabled = false;

        // search for the petrification ray child object if available
        petRay = transform.Find("PetrifyRay");
        if(petRay != null){
            petPos = petRay.localPosition;
            petRot = petRay.localEulerAngles;
            petrifyRayDetect = petRay.GetComponent<DetectionRange_GEN>();
            petrifyRaySpr = petRay.GetComponent<SpriteRenderer>();
            petrifyRaySpr.enabled = false;   // turn off the sprite renderer at the start
        }

        // start with max health
        curHealth = maxHealth;

        // if the health GUI is added, set the max and current amounts
        if(healthGUI != null){
            healthGUI.SetHealth(maxHealth, curHealth);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // set direction of petrify based on direction
        if(petRay){
            petRay.localPosition = direction == "right" ? petPos : new Vector3(petPos.x*-1.0f,petPos.y,petPos.z);
            petRay.localEulerAngles = direction == "right" ? petRot : new Vector3(petRot.x,petRot.y+180,petRot.z);
        }

        // right click to reach out --> grab --> devour
        if(Input.GetMouseButton(1)){
            Devour();
        }else{
            grabbingSnakes.enabled = false;
        }


        // if the petrify ray is on and an enemy is in range, petrify it
        if (Input.GetKeyDown(KeyCode.Q)){
            Petrify();
        }
        
        if(petrifyRayOn){
            Debug.Log(petrifyRayDetect);
            if(EnemyNotPetrified()){
                petrifyRayDetect.target.GetComponent<Enemy_Abstract>().Petrified();
            }
        }
    }

    // eating enemies
    void Devour()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);     // target pos
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;        // direction to aim snake

        grabbingSnakes.enabled = true;

        Vector2 dir = rb.position - mousePos;
        float dist = Mathf.Clamp(Vector3.Distance(rb.position, mousePos), 0, grabSnakeMax);
        Vector2 maxReach = rb.position - (dir.normalized * dist);

        grabbingSnakes.SetPosition(0, rb.position);
        grabbingSnakes.SetPosition(1, maxReach);

        // hard coding only one snake for now
        edges.Add(new Vector2(0, 0));

        GrabbingSnakeCollisions(grabbingSnakes);
        //Debug.Log(edgeCollider.points[0].x);
    }

    void GrabbingSnakeCollisions(LineRenderer snake)
    {
        /*List<Vector2> edges = new List<Vector2>();

        // hard coding only one snake for now
        Vector2 snakePoint = snake.GetPosition(1);      // for the end point position of the line
        edges.Add(new Vector2(snakePoint.x, snakePoint.y));

        edgeCollider.SetPoints(edges);*/

        Vector2 snakePoint = snake.GetPosition(1);
        edges[0] = new Vector2(snakePoint.x, snakePoint.y);
        edgeCollider.SetPoints(edges);
    }

    // check if the enemy target in the petrify range is already petrified or not
    bool EnemyNotPetrified(){
        return petrifyRayDetect.targetInSight 
        && petrifyRayDetect.target.tag == "enemy" 
        && !petrifyRayDetect.target.GetComponent<Enemy_Abstract>().isFrozen;
    }

    // turning enemies to stone
    void Petrify()
    {
        if(canPetrify)
            StartCoroutine(FlashPetrifyRay());
    }

    // timer for showing and activating the petrification ray
    IEnumerator FlashPetrifyRay(){
        // ray on + detection
        petrifyRayOn = true;
        petrifyRaySpr.enabled = true;
        canPetrify = false;
        
        yield return new WaitForSeconds(petrifyRayOnTime);

        // ray off
        petrifyRayOn = false;
        petrifyRaySpr.enabled = false;

        // ray cooldown
        yield return new WaitForSeconds(petrifyRayCooldown);
        canPetrify = true;
    }

    

    // called when medusa is hit by a projectile
    override public void Hurt(int dmgAmt){
        // still in grace period, so ignore
        if(!canHurt)
            return;

        // otherwise lose health and start grace invulnerability
        curHealth -= dmgAmt;

        // update GUI
        if(healthGUI !=null)
            healthGUI.UpdateHealth(curHealth);

        // lost all health
        if(curHealth <= 0)
            OnDeath();
        // still some health left
        else
            StartCoroutine(HurtGrace());
    }

    // activates the grace period for the player to be invulnerable immediately after being hit
    IEnumerator HurtGrace(){
        // grace on
        canHurt = false;
        sprRend.color = hurtColor;

        yield return new WaitForSeconds(healthGraceAmt);  

        // grace off
        sprRend.color = origColor;
        canHurt = true;
    }

    // action to do when the player reaches 0 health
    void OnDeath(){
        Application.LoadLevel(Application.loadedLevel);
    }

}
