using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Prologue : MonoBehaviour
{
    public GameObject continueButt;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void EndPrologue()
    {
        animator.SetBool("end", true);
        continueButt.SetActive(true);
    }
}
