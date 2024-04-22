using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_920 : MonoBehaviour
{
    public int dmg = 1;

    // if the projectile hits the target, deal damage and die
    void OnTriggerEnter2D(Collider2D c){
        if(c.gameObject.tag == "Player"){
            c.gameObject.GetComponent<PlayerBehavior_1114>().Hurt(dmg);
            Destroy(this.gameObject);
        }
    }
}
