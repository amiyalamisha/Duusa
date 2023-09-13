using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Camera cam;
    public LineRenderer lr;
    public Animator playerAnim;

    public LayerMask grappleMask;       // layer for all grapplable surfaces
    public float moveSpeed = 2;         // speed when it pulls you
    public float grappleLength = 10;    // how far can it shoot

    public int maxPoints = 3;       // how many grapples can you shoot max

    private Rigidbody2D rb;         // KEEPING GRAVITY OFF FOR NOW
    private List<Vector2> points = new List<Vector2>();     // all working grapples

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lr.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // checking button every frame
        if (Input.GetMouseButton(0))
        {
            lr.enabled = true;      // enable line

            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);     // target pos
            Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
            //Debug.Log(direction);
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, grappleLength, grappleMask);
            // trying to randomize in the general range of where the first hit point is aiming

            if(hit.collider != null)
            {
                Vector2 hitpoint = hit.point;
                points.Add(hitpoint);
                
                // line renderers get removed once max amount is deployed
                if(points.Count > maxPoints)
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

                rb.MovePosition(Vector2.MoveTowards(transform.position, mousePos, Time.deltaTime * moveSpeed));
                //rb.position = Vector2.Add(transform.position, points[0]);

                lr.positionCount = 0;
                lr.positionCount = points.Count * 2;

                for (int n = 0, j = 0; n < points.Count * 2; n += 2, j++)
                {
                    lr.SetPosition(n, transform.position);
                    lr.SetPosition(n + 1, points[j]);
                }
            }
        }
        /*
        if (!Input.anyKey)
        {
            
        }*/

        // only detach manually
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Detatch();
        }
    }

    // only detaching will get rid of lrs and make player fall
    void Detatch()
    {
        // no active lr points
        lr.positionCount = 0;
        points.Clear();
        lr.enabled = false;
    }

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
