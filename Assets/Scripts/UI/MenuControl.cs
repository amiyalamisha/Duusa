using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuControl : MonoBehaviour
{

    // allow dev to set the pause button here
    [Header("Input Settings")]
    [SerializeField] private KeyCode pauseBtn = KeyCode.Escape;

    // UI
    [Header("UI Objects")]
    public GameObject pauseScreen;
    public Button resumeBtn;
    public Button optionsBtn;
    public Button loadSaveBtn;
    public Button quitBtn;

    [Header("Accessible Properties")]
    public bool gamePaused = false;


    // add onclick listeners to the buttons to cause events
    void Start(){
        if(resumeBtn != null){
            resumeBtn.onClick.AddListener(() => UnpauseGame());
        }if(optionsBtn != null){
            optionsBtn.onClick.AddListener(() => OpenOptions());
        }if(loadSaveBtn != null){
            loadSaveBtn.onClick.AddListener(() => OpenSave());
        }if(quitBtn != null){
            quitBtn.onClick.AddListener(() => QuitGame());
        }

        if(gamePaused)
            PauseGame();
        else
            UnpauseGame();
    }

    
    // Update is called once per frame
    void Update(){
        // toggle pause with the button press
        if(Input.GetKeyDown(pauseBtn)){
            TogglePause();
        }
    }

    void TogglePause(){
        if(Time.timeScale == 0){
            UnpauseGame();
        }else{
            PauseGame();
        }
    }
    // pauses the game and shows the pause screen
    void PauseGame(){
        pauseScreen.SetActive(true);
        Time.timeScale = 0;
        gamePaused = true;
    }

    // unpauses the game for the player and removes the pause screen
    void UnpauseGame(){
        pauseScreen.SetActive(false);
        Time.timeScale = 1;
        gamePaused = false;
    }

    // opens the options menu
    void OpenOptions(){
        Debug.Log("Options screen opens here!");
    }

    // opens the save menu
    void OpenSave(){
        Debug.Log("Save / Load menu opens here!");
    }

    // quits the game
    // add a prompt like (are you sure you want to quit?)
    void QuitGame(){
        Debug.Log("Game will end here!");
    }
}
