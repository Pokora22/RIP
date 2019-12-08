using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu_scr : MonoBehaviour
{
    private GameObject pauseMenu;

    public static bool gameIsPaused = false;
    
    // Start is called before the first frame update
    void Start()
    {
        pauseMenu = GameObject.FindWithTag("PauseMenu");
        Resume();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!gameIsPaused)
                Pause();
            else
                Resume();
        }
            
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Exit()
    {
        SceneManager.LoadScene(0);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    } 

    public void Pause()
    {
        pauseMenu.SetActive(true);
        GetComponentInChildren<SettingsMenu>().SyncWithPlayerPrefs();
        Time.timeScale = 0f;
        gameIsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
