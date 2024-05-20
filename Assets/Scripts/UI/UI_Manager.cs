using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public class UI_Manager : MonoBehaviour
{
    public Animator healthAnimator;
    public Dialogue sceneDialogue;
    public AudioSource music;

    public static UI_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    public Image livesIcon;                   // the icon to show for health
    private Sprite livesSprite;
    private List<Sprite> livesList;         // running list of the life bar icons - doesn't destroy just unenables when health is lost

    
    void Start()
    {
        if(livesIcon != null)
        {
            livesSprite = livesIcon.GetComponent<Sprite>();

        }
        
        //if (!music.isPlaying)
        //{
            //music.Play();
        //}
        
        /*
        if (SceneManager.GetActiveScene().name == "1CaveLevelDialogue")
        {
            sceneDialogue.TriggerDialogue();
        }*/
    }

    public void UpdateHealthUI(int currentHealth)
    {
        healthAnimator.SetInteger("animHealth", currentHealth);         // changes health ui

    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);       // changing to next scene in squeance
    }

}
