using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayASound : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event sound;
    
    // Start is called before the first frame update
    void Start()
    {
        sound.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
