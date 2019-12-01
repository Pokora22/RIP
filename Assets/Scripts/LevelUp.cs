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

        List<int> allFuns = new List<int>() {0, 1, 2, 3, 4, 5};        

        for(int i = 0; i < 3; i++)
        {
            int fun = allFuns[Random.Range(0, allFuns.Count)];
            buttons[i] = Instantiate(lvlUpButton, levelUpPanel.transform.GetChild(1));
            buttons[i].GetComponent<UpgradeButton>().RegisterButton(this, fun);
            allFuns.Remove(fun);
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
