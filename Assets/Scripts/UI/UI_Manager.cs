using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject livesIcon;                   // the icon to show for health
    private Sprite livesSprite;
    private List<Sprite> livesList;         // running list of the life bar icons - doesn't destroy just unenables when health is lost

    
    void Start()
    {
        livesSprite = livesIcon.GetComponent<Sprite>();

        livesList.Append<Sprite>(Resources.Load<Sprite>("UI/health1"));
        livesList.Append<Sprite>(Resources.Load<Sprite>("UI/health2"));
        livesList.Append<Sprite>(Resources.Load<Sprite>("UI/health3"));
    }

    public void UpdateHealthUI(int currentHealth)
    {/*
        switch (currentHealth)
        {
            case 1: 
                livesSprite = 
        }*/
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);       // changing to next scene in squeance
    }
}
