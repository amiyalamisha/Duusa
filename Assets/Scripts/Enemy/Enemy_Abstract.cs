using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy_Abstract : MonoBehaviour
{
    // variables //
    public bool isFrozen = false;           // allows other scripts to check if the enemy is frozen

    // methods
    public abstract void Petrified();

}
