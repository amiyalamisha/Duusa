using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XYPlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 2.0f;

    void Start(){
        rb = transform.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update(){
        float hor = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime; 
        float ver = Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;
        
        rb.MovePosition(rb.position + new Vector2(hor, ver));
    }
}
