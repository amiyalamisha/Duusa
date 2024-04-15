using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerBehavior : MonoBehaviour
{
    public static PlayerBehavior instance;

    [Header("General Properties")]
    public Camera cam;
    public LineRenderer movementSnakes;
    public LineRenderer grabbingSnakes;
    EdgeCollider2D edgeCollider;
    private Animator playerAnim;
    private SpriteRenderer sprRend;         // sprite renderer of the player

    private void Awake()
    {
        instance = this;
    }

    public enum DuusaStates
    {
        Idle,                   // not moving
        Crawl,                  // moving around with snakes
        //Walking,              // normally walking
        Petrify,                // turning enemies to stone
        Grabbing,               // grabbing with snake
        Devour                  // devouring an enemy
    }
    public DuusaStates currentDuusaState;

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
    private SpriteRenderer petrifyRaySprR;           // sprite render for the petrification ray (toggles on and off)
    private Transform petrifyRayTransR;              // reference to transform for the petrification ray (to flip when not facing right)
    private DetectionRange_GEN petrifyRayDetectR;    // detection script for whether an enemy is in the petrification area
    private SpriteRenderer petrifyRaySprL;           // all petrify ray info for left side
    private Transform petrifyRayTransL;              
    private DetectionRange_GEN petrifyRayDetectL;
    private bool petrifyRayOn = false;              // boolean for if the ray is activated  
    public float petrifyRayOnTime = 0.3f;           // how long the ray stays activated for
    public float petrifyRayCooldown = 2.0f;         // how long before the ray can be used again
    public float reposRay = 0f; 
    private bool canPetrify = true;                 // flag for whether or not the petrification ray can be used
    private bool facingRight = false;               // checking for which direction player is facing

    [Header("Snake Grapple Properties")]
    public LayerMask grappleMask;       // layer for all grapplable surfaces
    public float maxSpeed = 1.5f;
    [SerializeField] private float grav;
    [SerializeField] private float gravMultiplier = 0;
    [SerializeField] private float moveSpeed = 2;         // speed when it pulls you
    [SerializeField] private float moveSnakeLength = 5;    // how far can it shoot
    [SerializeField] private float moveSnakeMin = 0.8f;    // min distance of snakes
    [SerializeField] private float grabSnakeMax = 5;    // min distance of snakes
    [SerializeField] private float velocity = 0;

    public int maxPoints = 3;       // how many movement snakes can you shoot max

    private Rigidbody2D rb;
    private Vector2 direction;
    private List<Vector2> points = new List<Vector2>();     // all working movement snakes
    public List<Vector2> edges = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();
        sprRend = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        origColor = sprRend.color;
        edgeCollider = grabbingSnakes.GetComponent<EdgeCollider2D>();
        grav = rb.gravityScale;

        movementSnakes.positionCount = 0;
        movementSnakes.enabled = false;
        grabbingSnakes.enabled = false;

        // search for the petrification ray child object if available
        Transform petRayRight = transform.Find("PetrifyRayRight");
        Transform petRayLeft = transform.Find("PetrifyRayLeft");
        if (petRayRight != null)
        {
            petrifyRayDetectR = petRayRight.GetComponent<DetectionRange_GEN>();
            petrifyRaySprR = petRayRight.GetComponent<SpriteRenderer>();
            petrifyRayTransR = petRayRight.GetComponent<Transform>();
            petrifyRaySprR.enabled = false;   // turn off the sprite renderer at the start
        }
        if (petRayLeft != null)
        {
            petrifyRayDetectL = petRayLeft.GetComponent<DetectionRange_GEN>();
            petrifyRaySprL = petRayLeft.GetComponent<SpriteRenderer>();
            petrifyRayTransL = petRayLeft.GetComponent<Transform>();
            petrifyRaySprR.enabled = false;   // turn off the sprite renderer at the start
        }

        // start with max health
        curHealth = maxHealth;

        // if the health GUI is added, set the max and current amounts
        if (healthGUI != null)
        {
            healthGUI.SetHealth(maxHealth, curHealth);
        }
    }

    // Update is called once per frame
    void Update()
    {/*
        switch (currentDuusaState)
        {

        }*/

        if (!Input.anyKey && !movementSnakes.enabled)
        {
            rb.gravityScale = grav;         // enabling gravity when lines are gone
        }

        // checking left click button every frame
        if (Input.GetMouseButton(0))
        {
            PlayerMovement();
            if (!facingRight)
            {
                sprRend.flipX = true;
            }
            else
            {
                sprRend.flipX = false;
            }
        }
        else if(movementSnakes.enabled)
        {
            // when left click is let go
            velocity = 0;
            rb.gravityScale *= gravMultiplier;
            //rb.drag = 3;
        }
        else
        {
            velocity = 0;
            //rb.drag = 1;
        }
        
        // right click to reach out --> grab --> devour
        if(Input.GetMouseButton(1))
        {
            Devour();
            
        }
        else
        {
            grabbingSnakes.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Petrify();
        }

        // allowing the starting point for all line rendenders to follow the player, even when not moving
        if (movementSnakes.enabled)
        {
            //lr.SetPosition(0, rb.position);
            
            for (int n = 0, j = 0; n < points.Count * 2; n += 2, j++)
            {
                movementSnakes.SetPosition(n, rb.position);
            }
        }

        // only detach manually
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Detatch();
        }

        // if the petrify ray is on and an enemy is in range, petrify it
        if (petrifyRayOn)
        {
            if (EnemyNotPetrified())
            {
                if(facingRight)
                    petrifyRayDetectR.target.GetComponent<Enemy_920>().Petrified();
                else
                    petrifyRayDetectL.target.GetComponent<Enemy_920>().Petrified();
            }
        }
    }

    // moving around with snakes (for now)
    void PlayerMovement()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);                 // target pos
        direction = (mousePos - (Vector2)transform.position).normalized;        // where the player should be moving
       
        // for which direction the player is currently facing (used for petrifry)
        if(direction.x < 0)
        {
            facingRight = false;
        }
        else
        {
            facingRight = true;
        }
        
        //Debug.Log(facingRight);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, moveSnakeLength, grappleMask);
        // trying to randomize in the general range of where the first hit point is aiming

        if (hit.distance > moveSnakeMin && hit.distance < moveSnakeLength)
        {
            if (hit.collider != null)
            {
                rb.gravityScale = 0.5f;
                movementSnakes.enabled = true;      // enable line

                Vector2 hitpoint = hit.point;
                points.Add(hitpoint);

                // line renderers get removed once max amount is deployed
                if (points.Count > maxPoints)
                {
                    points.RemoveAt(0);     // deactivates?
                    //Debug.Log(points.Count);
                }
            }

            if (points.Count > 0)
            {
                //Vector2 moveTo = centroid(points.ToArray());
                // current pos and target
                //Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);     // target pos
                //float distToTarget = Vector2.Distance(transform.position, mousePos);
                if (velocity < maxSpeed)
                    velocity += Time.deltaTime * moveSpeed;
                rb.MovePosition(Vector2.MoveTowards(transform.position, mousePos, velocity));
                //rb.position = Vector2.Add(transform.position, points[0]);

                //Transform targetX;
                //targetX.position = mousePos;


                movementSnakes.positionCount = 0;
                movementSnakes.positionCount = points.Count * 2;

                for (int n = 0, j = 0; n < points.Count * 2; n += 2, j++)
                {
                    movementSnakes.SetPosition(n, transform.position);
                    movementSnakes.SetPosition(n + 1, points[j]);
                }
            }
        }
    }

    // DETACHING SNAKES --> only detaching will get rid of lrs and make player fall
    void Detatch()
    {
        // no active lr points
        movementSnakes.positionCount = 0;
        points.Clear();
        movementSnakes.enabled = false;
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
        Vector2 snakePoint = snake.GetPosition(1);
        edges[0] = new Vector2(snakePoint.x, snakePoint.y);
        edgeCollider.SetPoints(edges);
    }

    bool EnemyNotPetrified()
    {
        if (facingRight)
        {
            return petrifyRayDetectR.targetInSight
            && petrifyRayDetectR.target.tag == "enemy"
            && !petrifyRayDetectR.target.GetComponent<Enemy_920>().isFrozen;
        }
        else
        {
            return petrifyRayDetectL.targetInSight
            && petrifyRayDetectL.target.tag == "enemy"
            && !petrifyRayDetectL.target.GetComponent<Enemy_920>().isFrozen;
        }

    }

    // turning enemies to stone
    void Petrify()
    {
        if (canPetrify)
            StartCoroutine(FlashPetrifyRay());
    }

    // timer for showing and activating the petrification ray
    IEnumerator FlashPetrifyRay()
    {
        //Vector2 rayPos = petrifyRayTrans.position;

        // ray on + detection
        petrifyRayOn = true;

        if(facingRight)
            petrifyRaySprR.enabled = true;
        else
            petrifyRaySprL.enabled = true;

        canPetrify = false;

        yield return new WaitForSeconds(petrifyRayOnTime);

        // ray off
        petrifyRayOn = false;

        if (facingRight)
            petrifyRaySprR.enabled = false;
        else
            petrifyRaySprL.enabled = false;

        // ray cooldown
        yield return new WaitForSeconds(petrifyRayCooldown);
        canPetrify = true;
    }

    // i dont remember what this does YET
    Vector2 centroid(Vector2[] points)
    {
        Vector2 center = Vector2.zero;

        foreach(Vector2 point in points)
        {
            center += point;
        }

        center /= points.Length;
        return center;
    }

    public void Hurt(int dmgAmt)
    {
        // still in grace period, so ignore
        if (!canHurt)
            return;

        // otherwise lose health and start grace invulnerability
        curHealth -= dmgAmt;

        // update GUI
        if (healthGUI != null)
            healthGUI.UpdateHealth(curHealth);

        // lost all health
        if (curHealth <= 0)
            OnDeath();
        // still some health left
        else
            StartCoroutine(HurtGrace());
    }

    // activates the grace period for the player to be invulnerable immediately after being hit
    IEnumerator HurtGrace()
    {
        // grace on
        canHurt = false;
        sprRend.color = hurtColor;

        yield return new WaitForSeconds(healthGraceAmt);

        // grace off
        sprRend.color = origColor;
        canHurt = true;
    }

    // action to do when the player reaches 0 health
    void OnDeath()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
}
