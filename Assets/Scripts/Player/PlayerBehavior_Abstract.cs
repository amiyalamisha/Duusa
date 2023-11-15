using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS SCRIPT CONTAINS GENERAL PROPERTIES ACCESSED BY OTHER SCRIPTS
// REGARDLESS OF THE VERSION OF THE PLAYERBEHAVIOR
// i.e. Hurt(), edges



public abstract class PlayerBehavior_Abstract : MonoBehaviour
{

    // variables //
    public List<Vector2> edges;
    public string direction = "right";      // default to the right


    // methods //
    public abstract void Hurt(int dmgAmt);

}
