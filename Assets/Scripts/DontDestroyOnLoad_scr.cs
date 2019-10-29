using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad_scr : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("DontDestroyOnLoad");
        if (objects.Length > 1)
            Destroy(this.gameObject);        

        DontDestroyOnLoad(this.gameObject);
    }
}
