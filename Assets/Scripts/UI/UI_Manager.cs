using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{
    public Animator healthAnimator;

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
        livesSprite = livesIcon.GetComponent<Sprite>();
    }

    public void UpdateHealthUI(int currentHealth)
    {
        /*switch (currentHealth)
        {
            
            case 1:
                livesSprite = livesList[0]; break;
            case 2:
                livesSprite = livesList[1]; break;
            case 3:
                livesSprite = livesList[2]; break;
        }*/

        healthAnimator.SetInteger("animHealth", currentHealth);

    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);       // changing to next scene in squeance
    }

    public void CharacterTalking()
    {

    }
}
