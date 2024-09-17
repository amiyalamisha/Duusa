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

    [Header("Prologue Properties")]
    private Camera mainCam;
    private float camSpeed = 2f;
    private Image [] comicList = new Image[4];

    [Header("Health UI Properties")]
    public Image livesIcon;                   // the icon to show for health
    private Sprite livesSprite;


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
        if (SceneManager.GetActiveScene().name == "0.5Prologue")
        {
            mainCam = FindObjectOfType<Camera>();

            Transform canvas = GameObject.Find("Canvas").transform;
            comicList[0] = canvas.GetChild(0).GetComponent<Image>();
            comicList[1] = canvas.GetChild(1).GetComponent<Image>();
            comicList[2] = canvas.GetChild(2).GetComponent<Image>();
            comicList[3] = canvas.GetChild(3).GetComponent<Image>();

            PlayPrologue();
        }*/
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "0.8CaveLevelDialogue")
        {
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                sceneDialogue.TriggerDialogue();
            }
        }
    }

    public void UpdateHealthUI(int currentHealth)
    {
        healthAnimator.SetInteger("animHealth", currentHealth);         // changes health ui

    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);       // changing to next scene in squeance
    }
    /*
    public void PlayPrologue()
    {
        for(int i = 0; i < 4; i++)
        {
            Debug.Log(comicList[i].transform.position);
            StartCoroutine(SwapComicCamFocus(i));
        }

    }

    IEnumerator SwapComicCamFocus(int comicNum)
    {
        yield return new WaitForSeconds(2.5f);

        Vector3 pos = Vector2.Lerp(mainCam.transform.position, comicList[comicNum].transform.position, camSpeed * Time.deltaTime);
        mainCam.transform.position = pos;
        Debug.Log(mainCam.transform.position);
    }
    */
}
