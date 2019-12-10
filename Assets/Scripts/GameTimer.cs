using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private float timeLimit, gameOverEnemyLimit;    
    private float maxTime;
    private List<GameObject> spawnedEnemies;
    private Transform player;
    private Image timer;
    [SerializeField] private GameObject gameOverEnemy;    

    private Vector3 spawnPoint;

    private void Start(){
        maxTime = timeLimit;
        timer = GameObject.FindGameObjectWithTag("TimerUI").GetComponent<Image>();
        spawnPoint = Vector3.zero;
        spawnedEnemies = new List<GameObject>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        timeLimit -= Time.deltaTime;
        float fillPercent = timeLimit/maxTime;               

        timer.color = new Color(1 - fillPercent, fillPercent, 0);
        timer.fillAmount = fillPercent;

        if(timeLimit <= 0)
            SpawnGameOver();
    }

    private void SpawnGameOver(){
        if(spawnedEnemies.Count < gameOverEnemyLimit){
            GameObject enemy = Instantiate(gameOverEnemy, SpawnPoint(), Quaternion.identity);
            enemy.GetComponent<AI.PatrolAIControl>().setNewDestination(player.position);
            spawnedEnemies.Add(enemy);            
        }
    }

    private Vector3 SpawnPoint(){
        Debug.Log(NavMesh.GetAreaFromName("Walkable"));
        NavMeshHit navMeshHit;
        //Bitmask 1 << 0 == index 0 in areas
        if(NavMesh.SamplePosition(player.position - player.forward * 10f, out navMeshHit, 10f, 1<<0))
            return navMeshHit.position;
        else
            return player.position;
            // TODO:Spawn where if there's no space behind player?            
    }

    public float getTimeLeft()
    {
        return timeLimit;
    }
}
