using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
