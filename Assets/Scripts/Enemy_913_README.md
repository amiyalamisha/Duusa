# Enemy 9_13

This documents the details and code descriptions of the enemy AI character. It is capable of horizontal patrolling movement, shooting, and player detection. This enemy design uses the following finite-state machine behavior definition:

![enemy_913-fsm](https://docs.google.com/drawings/d/1-viVV_wPhjOHPqwXBb0CqndYZszuLX2V_jGMNBudFQE/edit?usp=sharing)

## Prefab Components

Enemy 9_13 is made up of the following elements:

- **Base Game Object**: Contains the main definition of the enemy behavior in the [Enemy_913.cs](Enemy_913.cs) script. Details of these properties are described more in the [Code Description](#code-description) section. This parent object also includes SpriteRenderer, BoxCollider2D, and Rigidbody2D components.
- **PatrolRange**: An object with a PolygonCollider2D component that detects when the player has entered a "line-of-sight" during patrolling. It can be modified to fit any polygon by setting the 'Points' property. By default it is shaped like a cone and faces in the movement direction of the enemy. It has a [DetectionRange](DetectionRange.cs) script to notify the main script when the player has entered or exited the range of the collider element. 
- **ShootRange**: An object with a CircleCollider2D component that detects when the player has exited the shooting range area during the 'chase' and 'shoot' behaviors. It has a [DetectionRange](DetectionRange.cs) script to notify the main script when the player has exited the range of the collider element. 
- **Projectile**: An empty transform that hosts the [Enemy Projectile](minor_scripts/EnemyProjectileAttack.cs) script to allow for bullet attacks at the player. Bullet spawn from the position this transform is placed at relative to the enemy.

## Code Description

The following table describes variable properties that can be modified for each Enemy entity within the Inspector editor.

| Variable | Description | Default Value |
| :------- | :---------- | :------------ |
| Patrol Speed | How fast the enemy moves during its patrol phase | 2 |
| Chase Speed | How fast the enemy moves while chasing the player | 4 |
| Cur State | The current AI state of the enemy. Uses the AIState enum | Patrol |
| Patrol Pts | A list of transforms that indicate where the enemy can patrol to. 0 means the enemy will not patrol and will stand where it is. 1 point means the enemy will return to its position after chasing the player. 2+ means it will cycle through all the points | null |
| Wait Patrol Time | How long the enemy will wait at a patrol point before moving to the next | 2(s) |
| Patrol Min Dist | The minimum distance needed to reach a particular patrol point. Recommended to modify based on size of the enemy | 0.5 |

`AIState` is action node definitions of the enemy agent. It has 4 possible states:
```
    public enum AIState{
        Idle,               // does nothing
        Patrol,             // walking around to patrol points
        Chase,              // chasing after the player
        Shoot               // stops in place tries to shoot the player
    }
```

The following table describes key functions within the [Enemy_913.cs](Enemy_913.cs) script.

| Function Name | Inputs | Outputs | Description | Usage |
| :-----------: | :----- | :------ | :---------- | :---- |
| `InRange2D` | Transform a, Transform b, float d | bool | Detects whether a point (a) is within a minimum distance (d) of another point (b). Checks both X and Y positions | `InRangeX(transform,target,minDist) // checks if the current transform is within a minDist of a target transform` |
| `InRangeX` | Transform a, Transform b, float d | bool | Detects whether a point (a)'s X value is within a minimum distance (d) of another point (b)'s X value. | `InRangeX(transform,target,minDist) // checks if the current transform is within a minDist of a target transform based only on the horizontal axis` |
| `GoToTarget` | Transform target, float magnitude, float minDist, bool ignoreY | none | Moves the enemy to a target position at a certain speed (magnitude) up until a certain distance (minDist). Can ignore Y value verticality | `GoToTarget(target, 4, 3.0f); // goes to a target position at a speed of 4 until it is within 3.0 units of the target` |
| `TranstitionState` | `AIState` nextState, float transTime | none | Waits a few seconds before transtitioning to another `AIState` | `StartCoroutine(TranstitionState(AIState.Patrol, 2.0f)); \\ transtitions back to patroling after waiting 2 seconds` |

For more details and other functions, please see the [Enemy_913.cs](Enemy_913.cs) script.



