using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
    private GameObject levelUpPanel;
    [SerializeField] GameObject lvlUpButton;
    private GameObject[] buttons;

    // Start is called before the first frame update
    void Start()
    {
        buttons = new GameObject[3];
        levelUpPanel = GameObject.FindWithTag("UILevelNotification");
        levelUpPanel.SetActive(false);
    }

    public void levelUp(){
        // //TODO: Play sound ?
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        levelUpPanel.SetActive(true);
        Time.timeScale = 0;

        for(int i = 0; i < 3; i++){
            buttons[i] = Instantiate(lvlUpButton, levelUpPanel.transform);
            buttons[i].GetComponent<UpgradeButton>().RegisterPanel(this);
        }

        //TODO: Panel with some buttons - make buttons do things
    }

    public void ClosePanel()
    {
        for(int i = 0; i < buttons.Length; i++)
            Destroy(buttons[i]);
        
        levelUpPanel.SetActive(false);
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
