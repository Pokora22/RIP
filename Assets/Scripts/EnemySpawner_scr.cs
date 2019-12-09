using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner_scr : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private float spawnDelay, spawnUnitLimit;
    private Vector3 spawnPoint;
    private float timer;
    private int unitsSpawned = 0;
    public static int totalUnitsSpawned;
    
    // Start is called before the first frame update
    void Start()
    {
        float diff = PlayerPrefs.GetInt("difficulty");
        spawnPoint = transform.GetChild(0).position;
        spawnDelay -= spawnDelay * (diff/10);
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
                if (totalUnitsSpawned < GameManager_Scr.BaseNumberOfUnits)
                {
                    GameManager_Scr.DifficultyMod *= 1.5f;
                    spawnUnitLimit++;
                }
                else if (totalUnitsSpawned > GameManager_Scr.BaseNumberOfUnits)
                    GameManager_Scr.DifficultyMod /= 1.5f;
                
                timer = spawnDelay - GameManager_Scr.DifficultyMod;
                timer = timer < 1f ? 1f : timer;
                Instantiate(enemies[Random.Range(0, enemies.Length)], spawnPoint,
                    transform.rotation);
                unitsSpawned++;
                totalUnitsSpawned++;
            }
            else
                timer -= Time.deltaTime;
        }
    }
}
