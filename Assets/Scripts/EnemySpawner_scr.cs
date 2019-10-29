using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner_scr : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private float spawnDelay, spawnUnitLimit;
    private float timer;
    
    // Start is called before the first frame update
    void Start()
    {
        timer = spawnDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer <= 0)
        {
            timer = spawnDelay;
            Instantiate(enemies[Random.Range(0, enemies.Length)], transform.position - transform.forward * 5, transform.rotation);
        }
        else
            timer -= Time.deltaTime;
    }
}
