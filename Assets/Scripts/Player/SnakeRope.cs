using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// modified from Brackeys' Cut the Rope Tutorial: https://www.youtube.com/watch?v=dx3jb4muLjQ
// and Rope Bridge tutorial: https://www.youtube.com/watch?v=VF3zSa3oI3I

public class SnakeRope : MonoBehaviour
{
    public Rigidbody2D hook;
    public GameObject snakeBodyPrefab;
    public GameObject snakeHead;
    public List<GameObject> linkBodySet = new List<GameObject>();
    public GameObject snakeTarget;

    public float maxPartLength = 1.5f;              // length of one part of the snake
    public float slack = 2.0f;

    void Start(){
        // GenerateSnakeGrapple(Vector2.zero);             // grapple at the origin
    }

    // makes new snakes to reach to specified point
    public void GenerateSnakeGrapple(Vector2 targPos){
        // set up
        DeleteSnakes();
        linkBodySet = new List<GameObject>();

        Rigidbody2D previousRB = hook;  
        float totDist = Vector2.Distance(transform.position, targPos);
        float num_links = Mathf.Ceil((totDist + slack) / maxPartLength);
        // Debug.Log(totDist + " => " + num_links + " links");
        // int num_links = 5;

        // make snake links
        for(int i=0;i<num_links;i++){
            GameObject link = Instantiate(snakeBodyPrefab, transform);
            HingeJoint2D joint = link.GetComponent<HingeJoint2D>();
            joint.connectedBody = previousRB;
            previousRB = link.GetComponent<Rigidbody2D>();

            linkBodySet.Add(link);
        }

        // make the head for grappling
        HingeJoint2D headJoint = snakeHead.GetComponent<HingeJoint2D>();
        headJoint.autoConfigureConnectedAnchor = false;
        headJoint.connectedBody = previousRB;
        headJoint.anchor = Vector2.zero;
        headJoint.connectedAnchor = new Vector2(-0.65f,0);
        // headJoint.connectedAnchor = new Vector2(0,0.65f);
        // snakeHead.GetComponent<HingeJoint2D>().connectedAnchor = targPos;
        // snakeHead.transform.position = targPos;

        snakeTarget.transform.position = targPos;
        // snakeTarget.GetComponent<SnakeRopeTarget>().ConnectHead(snakeHead);

    }

    public void UpdateSnakeRope(){
        // snakeHead.transform.position = targPosition;
        // update the line renderers
        Rigidbody2D previousRB = hook;
        for(int i=0;i<linkBodySet.Count;i++){
            GameObject link = linkBodySet[i];

            // make connecting line renderer
            LineRenderer snakeLine = link.GetComponent<LineRenderer>();
            snakeLine.SetPosition(0, Vector2.zero);
            snakeLine.SetPosition(1, link.transform.InverseTransformPoint(previousRB.transform.position));
            previousRB = link.GetComponent<Rigidbody2D>();
        }

        // update the head link
        LineRenderer headLine = snakeHead.GetComponent<LineRenderer>();
        headLine.SetPosition(0, Vector2.zero);
        headLine.SetPosition(1, snakeHead.transform.InverseTransformPoint(previousRB.transform.position));
    }

    // hide and delete everything
    // and this is why you never use while loops
    void DeleteSnakes(){
        foreach(var link in linkBodySet){
            Destroy(link);
        }
    }

    public void HideSnakes(){
        DeleteSnakes();
    }
}
