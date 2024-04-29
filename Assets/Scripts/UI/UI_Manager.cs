using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject livesIcon;                   // the icon to show for health
    private List<Sprite> livesList;         // running list of the life bar icons - doesn't destroy just unenables when health is lost

    
    void Start()
    {
        livesList.Append<Sprite>(Resources.Load<Sprite>("UI/health1"));
        livesList.Append<Sprite>(Resources.Load<Sprite>("UI/health2"));
        livesList.Append<Sprite>(Resources.Load<Sprite>("UI/health3"));
    }

    // idk make fuction back to playercode
    void Update()
    {
        
    }
}
