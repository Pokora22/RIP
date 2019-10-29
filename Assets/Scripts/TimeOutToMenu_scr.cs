using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeOutToMenu_scr : MonoBehaviour
{
    [SerializeField] private float time = 5f;

    // Update is called once per frame
    void Update()
    {
        if (time > 0)
            time -= Time.deltaTime;
        else
            SceneManager.LoadScene(0);
    }
}
