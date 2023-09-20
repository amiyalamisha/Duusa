using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    public Camera cam;
    public LineRenderer movementSnakes;
    public LineRenderer grabbingSnakes;
    EdgeCollider2D edgeCollider;
    public Animator playerAnim;

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
    private List<Vector2> edges = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        edgeCollider = grabbingSnakes.GetComponent<EdgeCollider2D>();
        grav = rb.gravityScale;

        movementSnakes.positionCount = 0;
        movementSnakes.enabled = false;
        grabbingSnakes.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        // when no keys are down and lines are not enabled
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
    }

    // moving around with snakes (for now)
    void PlayerMovement()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);     // target pos
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;        // where the player should be moving

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
        /*List<Vector2> edges = new List<Vector2>();

        // hard coding only one snake for now
        Vector2 snakePoint = snake.GetPosition(1);      // for the end point position of the line
        edges.Add(new Vector2(snakePoint.x, snakePoint.y));

        edgeCollider.SetPoints(edges);*/

        Vector2 snakePoint = snake.GetPosition(1);
        edges[0] = new Vector2(snakePoint.x, snakePoint.y);
        edgeCollider.SetPoints(edges);
    }

    // turning enemies to stone
    void Petrify()
    {

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
}
