using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraLock : MonoBehaviour
{
    // stolen from https://www.youtube.com/watch?v=Fqht4gyqFbo

    public Transform target;
    public Vector3 offset;
    [Range(1,10)]
    public float smoothing = 2.0f;

    public List<Transform> boundPts;
    private Vector2 minBounds;
    private Vector2 maxBounds;


    // Start is called before the first frame update
    void Start() {

        if(boundPts.Count >= 4){

            float minX = boundPts.Select( v => v.position.x).AsQueryable().Min();
            float maxX = boundPts.Select( v => v.position.x).AsQueryable().Max();
            float minY = boundPts.Select( v => v.position.y).AsQueryable().Min();
            float maxY = boundPts.Select( v => v.position.y).AsQueryable().Max();

            minBounds = new Vector2(minX, minY);
            maxBounds = new Vector2(maxX, maxY);
        }else{
            minBounds = new Vector2(-Mathf.Infinity,-Mathf.Infinity);
            maxBounds = new Vector2(Mathf.Infinity,Mathf.Infinity);
        }
    }

    // Update is called once per frame
    void Update(){
        Follow();
    }

    // follows a target up until the bounds
    void Follow(){
        Vector3 targPos = target.position + offset;

        // check if out of bounds
        Vector3 boundPos = new Vector3(
            Mathf.Clamp(targPos.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(targPos.y, minBounds.y, maxBounds.y),
            offset.z
        );

        Vector3 smoothPos = Vector3.Lerp(transform.position, targPos, smoothing*Time.deltaTime);
        transform.position = smoothPos;
    }
}
