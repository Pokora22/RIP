using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HouseSpawn : MonoBehaviour
{
    private static int spawnersLeft;

    private Attributes_scr _attributesScr;
    // Start is called before the first frame update
    void Start()
    {
        spawnersLeft++;
        _attributesScr = GetComponent<Attributes_scr>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_attributesScr.health <= 0)
        {
            if (--spawnersLeft <= 0)
                GameObject.FindWithTag("GameManager").GetComponent<PlayerAttributes_scr>().EndGame();
            
            gameObject.GetComponent<EnemySpawner_scr>().enabled = false;
            this.enabled = false;
        }
    }

    private void OnDestroy()
    {
        
    }
}
