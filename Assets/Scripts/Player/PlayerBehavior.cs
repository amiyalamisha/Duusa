using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerBehavior : MonoBehaviour
{
    [Header("General Properties")]
    public Camera cam;
    public LineRenderer movementSnakes;
    public LineRenderer grabbingSnakes;
    EdgeCollider2D edgeCollider;
    public Animator playerAnim;

    [Header("Health Properties")]
    private SpriteRenderer sprRend;         // sprite renderer of the player
    public int maxHealth = 3;               // maximum possible health of the player
    public int curHealth = 3;               // current health of the player
    public float healthGraceAmt = 0.6f;     // invulnerability time before can take damage again
    public bool canHurt = true;             // flag for whether the player is vulnerable to attacks
    private Color origColor;                // stores the original color to revert back to
    public Color hurtColor;                 // color for when player is hurt and grace period is activated
    public HealthUI healthGUI;              // GUI for the health 

    // petrification properties
    [Header("Petrification Properties")]
    private SpriteRenderer petrifyRaySpr;           // sprite render for the petrification ray (toggles on and off)
    private Transform petrifyRayTrans;              // reference to transform for the petrification ray (to flip when not facing right)
    private DetectionRange_GEN petrifyRayDetect;    // detection script for whether an enemy is in the petrification area
    private bool petrifyRayOn = false;              // boolean for if the ray is activated  
    public float petrifyRayOnTime = 0.3f;           // how long the ray stays activated for
    public float petrifyRayCooldown = 2.0f;         // how long before the ray can be used again
    public float reposRay = 0f; 
    private bool canPetrify = true;                 // flag for whether or not the petrification ray can be used
    private bool facingRight = false;               // checking for which direction player is facing

    [Header("Snake Grapple Properties")]
    public LayerMask grappleMask;       // layer for all grapplable surfaces
    [SerializeField] private float grav;
    [SerializeField] private float moveSpeed = 2;         // speed when it pulls you
    [SerializeField] private float moveSnakeLength = 5;    // how far can it shoot
    [SerializeField] private float moveSnakeMin = 0.8f;    // min distance of snakes
    [SerializeField] private float grabSnakeMax = 5;    // min distance of snakes
    [SerializeField] private float velocity = 0;

    public int maxPoints = 3;       // how many movement snakes can you shoot max

    private Rigidbody2D rb;
    private List<Vector2> points = new List<Vector2>();     // all working movement snakes
    public List<Vector2> edges = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        sprRend = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        origColor = sprRend.color;
        edgeCollider = grabbingSnakes.GetComponent<EdgeCollider2D>();
        grav = rb.gravityScale;

        movementSnakes.positionCount = 0;
        movementSnakes.enabled = false;
        grabbingSnakes.enabled = false;

        // search for the petrification ray child object if available
        Transform petRay = transform.Find("PetrifyRay");
        if (petRay != null)
        {
            petrifyRayDetect = petRay.GetComponent<DetectionRange_GEN>();
            petrifyRaySpr = petRay.GetComponent<SpriteRenderer>();
            petrifyRayTrans = petRay.GetComponent<Transform>();
            petrifyRaySpr.enabled = false;   // turn off the sprite renderer at the start
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
    {
        if (!Input.anyKey && !movementSnakes.enabled)
        {
            rb.gravityScale = grav;         // enabling gravity when lines are gone
        }

        // checking left click button every frame
        if (Input.GetMouseButton(0))
        {
            PlayerMovement();
        }
        else if(movementSnakes.enabled)
        {
            // when left click is let go
            velocity = 0;
            rb.gravityScale = 0;
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

        // allowing the starting point for all line rendenderers to follow the player, even when not moving
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
                petrifyRayDetect.target.GetComponent<Enemy_913>().Petrified();
            }
        }
    }

    // moving around with snakes (for now)
    void PlayerMovement()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);                 // target pos
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;        // where the player should be moving
        
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
                rb.gravityScale = 0.2f;
                movementSnakes.enabled = true;      // enable line

                Vector2 hitpoint = hit.point;
                points.Add(hitpoint);

                // line renderers get removed once max amount is deployed
                if (points.Count > maxPoints)
                {
                    points.RemoveAt(0);     // deactivates?
                    Debug.Log(points.Count);
                }
            }

            if (points.Count > 0)
            {
                //Vector2 moveTo = centroid(points.ToArray());
                // current pos and target
                //Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);     // target pos
                //float distToTarget = Vector2.Distance(transform.position, mousePos);
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
        return petrifyRayDetect.targetInSight
        && petrifyRayDetect.target.tag == "enemy"
        && !petrifyRayDetect.target.GetComponent<Enemy_920>().isFrozen;
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
        Vector2 rayPos = petrifyRayTrans.position;

        // ray on + detection
        petrifyRayOn = true;
        petrifyRaySpr.enabled = true;
        canPetrify = false;
        /*
        if (!facingRight)
        {
            if (petrifyRaySpr.flipY)
            {
                petrifyRayTrans.position = new Vector2(rayPos.x, rayPos.y);
            }
            else
            {
                petrifyRaySpr.flipY = true;
                //rayPos *= -1;
                petrifyRayTrans.position = new Vector2(-rayPos.x+reposRay, rayPos.y);
                Debug.Log("faceright shot");
            }
        }
                
        else
        {
            if (petrifyRaySpr.flipY)
            {
                petrifyRaySpr.flipY = false;
                petrifyRayTrans.position = new Vector2(-rayPos.x+ reposRay, rayPos.y);
            }
            else
            {
                petrifyRayTrans.position = new Vector2(rayPos.x, rayPos.y);
            }
        }*/

        yield return new WaitForSeconds(petrifyRayOnTime);

        // ray off
        petrifyRayOn = false;
        petrifyRaySpr.enabled = false;

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
