using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyBehavior : MonoBehaviour
{
    public float speed;
    public float distance;

    private bool movingRight = true;

    public Transform groundDetection;
    public Transform enemyBack;
    public Animator enemyAnim;

    public GameObject bullet;
    public Transform bulletPos;
    private GameObject player;

    public float posHigherGround;       // what position are you in air

    private float timer;

    public bool isShooting;         // is this enemy instance shooting?

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Player");
        player = temp[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y > posHigherGround)
        {
            Destroy(gameObject);
        }

        transform.Translate(Vector2.right * speed * Time.deltaTime);
        
        //RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, distance);
        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, distance, LayerMask.GetMask("EnemyGround"));
        
            
        if(groundInfo.collider == false)
        {
            enemyAnim.SetBool("running", true);
            if (movingRight)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
                movingRight = false;
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                movingRight = true;
            }
        }

        if (isShooting)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            
            if(distance < 20)
            {
                timer += Time.deltaTime;

                if (timer > 4)
                {
                    timer = 0;
                    shoot();
                }
            }
        }
    }

    void shoot()
    {
        Instantiate(bullet, bulletPos.position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Destroy(gameObject);
            Debug.Log("hit");
        }
    }
}
