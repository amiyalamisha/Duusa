using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileAttack : MonoBehaviour
{
    public GameObject bulletObj;                // bullet prefab object to shoot at medusa
    public float bulletSpeed = 5.0f;                   // bullet speed it moves at
    public float bulletTimeout = 0.5f;                 // time bullet is alive (cannot use with bulletRange)
    public bool canFire = true;                        // allow to fire another bullet
    public float reloadTime = 1.5f;                    // time allotted before firing another bullet
    // public float bulletRange;                   // distance the bullet is allowed to travel at (cannot use with bulletTimeout)


    // shoots a bullet from the current position the script is placed on
    public void FireBullet(Transform target){
        if(!canFire) return;

        // make a bullet
        GameObject b = Instantiate(bulletObj);
        b.transform.position = transform.position;
        // b.transform.LookAt(target);

        // fire with a force 
        b.GetComponent<Rigidbody2D>().AddForce((target.position - b.transform.position) * bulletSpeed, ForceMode2D.Impulse);

        // start the destroy countdown
        if(bulletTimeout > 0.0f)
            StartCoroutine(BulletDeath(b));

        // reload the bullet
        canFire = false;
        StartCoroutine(Reload());
    }

    // death countdown for the bullet after being fired
    IEnumerator BulletDeath(GameObject b){
        yield return new WaitForSeconds(bulletTimeout);
        Destroy(b);
    }

    // reload time to allow firing of another bullet
    IEnumerator Reload(){
        yield return new WaitForSeconds(reloadTime);
        canFire = true;
    }

}
