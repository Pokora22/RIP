using System.Collections;
using System.Collections.Generic;
using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerAttributes_scr : MonoBehaviour
{
    // Start is called before the first frame update
    public int summonsLimit = 5;
    public float maxHealth = 3f;
    private float health;

    private float currentExp;
    private float currentLvl;
    private int skillPoints = 0;
    [SerializeField] private float baseExpReq = 50;
    private float nextLvlExpReq;
    private PlayerController_scr summoner;
    private TextMeshProUGUI hudZombieCount;
    [SerializeField] private Sprite[] phylacteri;
    private GameObject[] livesDisplay;
    private Image expBar;
    private GameObject levelNotification;

    private void Awake()
    {
        //Initialize values for other things to use
        PlayerPrefs.SetFloat("DifficultyMod", 0);
        PlayerPrefs.SetInt("MinionsKept", 0); //set held minions to 0 at start of game
    }

    void Start()
    {
        nextLvlExpReq = baseExpReq;
        health = maxHealth;
        currentLvl = 0;
        summoner = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_scr>();
        hudZombieCount = GameObject.FindWithTag("UIZombieCount").GetComponentInChildren<TextMeshProUGUI>();
        livesDisplay = GameObject.FindGameObjectsWithTag("UIPhylactery");
        expBar = GameObject.FindWithTag("UIExpBar").GetComponent<Image>();
        levelNotification = GameObject.FindWithTag("UILevelNotification");
        
        updateHud();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
            addExp(100);
    }

    public void damage()
    {
        if (--health < 1)
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene("GameOver"); //TODO: Maybe a game over screen
        }

        updateHud();
    }

    public bool heal()
    {
        if (health + 1 <= maxHealth)
        {
            health++;
            updateHud();
            return true;
        }

        return false;
    }

    public void addExp(float exp)
    {
        currentExp += exp;

        if (currentExp > nextLvlExpReq)
        {
            
            float overflow = exp - (nextLvlExpReq - currentExp); //Get exp that would go over the amnt required

            currentLvl++;
            skillPoints++;
            currentExp = 0;

            nextLvlExpReq += baseExpReq * 1.5f; //TODO: Better formula for next lvl

            if (overflow > 0)
                addExp(overflow); //Add the exp after new lvl if there's any over the amnt required before

            
        }
        updateHud();
    }

    public void updateHud()
    {
        if(livesDisplay.Length >= health)
        for (int i = 0; i < maxHealth; i++)
            livesDisplay[i].GetComponent<Image>().sprite = i < health ? phylacteri[1] : phylacteri[0];

        expBar.fillAmount = currentExp / nextLvlExpReq;
        
        hudZombieCount.SetText(" x " + (summoner.minions.Count + summoner.minionsAway.Count));
        
        levelNotification.SetActive(skillPoints > 0); //TODO: When skill points are in, check that against 0
    }
}
