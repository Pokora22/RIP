using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Score : MonoBehaviour
{
    private int exp, minions, time, health;
    
    private Score _score;
    private void Awake()
    {
        _score = gameObject.GetComponent<Score>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameOver")
        {
            int timePts = time * health * 3;
            int expPts = exp * 10;
            int healthPts = health * 100;
            int minionsPts = minions * 20;
            int totalPts = timePts + expPts + healthPts + minionsPts;
            TextMeshProUGUI scoreText = GameObject.FindWithTag("ScorePane").GetComponentInChildren<TextMeshProUGUI>();
            scoreText.text = "Experience: " + expPts
                                            + "\nTime: " + timePts
                                            + "\nHealth: " + healthPts
                                            + "\nMinions: " + minionsPts
                                            + "\n\nTotal: " + totalPts;

        }
    }

    public void UpdateScore(int exp, int minions, int time, int health)
    {
        this.exp = exp;
        this.minions = minions;
        this.time = time;
        this.health = health;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
