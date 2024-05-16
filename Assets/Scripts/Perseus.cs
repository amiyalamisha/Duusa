using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perseus : MonoBehaviour
{
    public float chase_speed = 5.0f;          // speed of movement for the chase behavior
    private bool facingRight = true;           // is facing the right direction (rot=0) or left (rot=180) (default right, change if otherwise)
    public bool willShootMedusa = true;         // if the enemy will shoot medusa on sight

    public enum PerseusAIState
    {
        Idle,               // doing jack-shit lol
        Chasing,         // walking around, peacefully
        Bolts,              // chasing after medusa
        Sword,              // defense from medusa grab,
        Petrified,          // a limb is turned to stone - shake?
        Breakout,           // breaking free a petrified limb
        Hurt,               // a limb is ripped off
    }
    public PerseusAIState curPerseusState;

    private SpriteRenderer shootArea;        // detection area for shooting (for debug visuals)

    // Start is called before the first frame update
    void Start()
    {
        curPerseusState = PerseusAIState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        switch (curPerseusState)
        {
            case PerseusAIState.Idle:
                break;
            case PerseusAIState.Chasing:
                /*// need detection range
                if (shootRange.target != null && !InRangeX(transform, shootRange.target, 3.0f))
                {
                    GoToTarget(shootRange.target, chase_speed, 3.0f, true);
                }
                // if lost the target, go back to patrol
                else if (shootRange.target == null)
                {
                    currentEnemyState = AIState.Idle;
                    StartCoroutine(TranstitionState(AIState.Patrol, waitPatrolTime));
                }
                // otherwise try to shoot her
                else if (InRangeX(transform, shootRange.target, 3.0f))
                {
                    currentEnemyState = AIState.Shoot;
                }*/
                break;
            case PerseusAIState.Bolts:
                break;
            case PerseusAIState.Sword:
                break;
            case PerseusAIState.Hurt:
                break;
        }
    }
}
