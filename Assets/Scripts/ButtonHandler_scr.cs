using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler_scr : MonoBehaviour
{
    public void startScene(int scene){
        Debug.Log("Loading " + scene);
        SceneManager.LoadSceneAsync((int)scene);
    }

    public void exit(){
        Application.Quit(0);
    }
}
