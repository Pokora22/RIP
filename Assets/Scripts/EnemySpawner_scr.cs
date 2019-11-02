using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner_scr : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private float spawnDelay, spawnUnitLimit;
    private float timer;
    private int unitsSpawned = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        float diff = PlayerPrefs.GetInt("difficulty");
        spawnDelay -= spawnDelay * (diff/4);
        spawnUnitLimit = diff * 10 + 10;
        timer = spawnDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if(unitsSpawned < spawnUnitLimit)
        {
            if (timer <= 0)
            {
                timer = spawnDelay;
                Instantiate(enemies[Random.Range(0, enemies.Length)], transform.position - transform.forward * 5,
                    transform.rotation);
                unitsSpawned++;
            }
            else
                timer -= Time.deltaTime;
        }
    }
}
