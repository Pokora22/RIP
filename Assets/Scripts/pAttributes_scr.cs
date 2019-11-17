using System.Collections;
using System.Collections.Generic;
using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class pAttributes_scr : MonoBehaviour
{
    // Start is called before the first frame update
    public int summonsLimit = 5;
    public float maxHealth = 3f;
    private float health;

    private float currentExp;
    private float currentLvl;
    [SerializeField] private float baseExpReq = 50;
    private float nextLvlExpReq;
    private PlayerController_scr summoner;
    private TextMeshProUGUI hudZombieCount;
    [SerializeField] private Sprite[] phylacteri;
    private GameObject[] livesDisplay;
    private Image expBar;
    private GameObject levelNotification;
    [SerializeField] private List<Artifact_scr> playerInventory;
    [SerializeField] private float useArtifactPeriod = 1f;
    [SerializeField] private float invulnerableTime = 0.5f;
    private bool invulnerable;
    

    void Start()
    {
        nextLvlExpReq = baseExpReq;
        health = maxHealth;
        summoner = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_scr>();
        hudZombieCount = GameObject.FindWithTag("UIZombieCount").GetComponentInChildren<TextMeshProUGUI>();
        livesDisplay = GameObject.FindGameObjectsWithTag("UIPhylactery");
        expBar = GameObject.FindWithTag("UIExpBar").GetComponent<Image>();
        levelNotification = GameObject.FindWithTag("UILevelNotification");
        playerInventory = new List<Artifact_scr>();
        invulnerable = false;
        
        updateHud();
    }

    public void damage()
    {
        if (!invulnerable)
        {
            if (--health < 1)
                SceneManager.LoadScene(2); //TODO: Maybe a game over screen

            StartCoroutine(toggleInvulnerable(invulnerableTime));
            updateHud();
        }
    }

    public void addExp(float exp)
    {
        currentExp += exp;

        if (currentExp > nextLvlExpReq)
        {
            
            float overflow = exp - (nextLvlExpReq - currentExp); //Get exp that would go over the amnt required

            currentLvl++;
            Debug.Log("Advanced to " + currentLvl + " level.");
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
            livesDisplay[i].GetComponent<Image>().sprite = i < health ? phylacteri[1] : phylacteri[0]; //TODO: Add some limiter to not ooi on test numbers

        expBar.fillAmount = currentExp / nextLvlExpReq;
        
        hudZombieCount.SetText(" x " + (summoner.minions.Count + summoner.minionsAway.Count));
        
        levelNotification.SetActive(currentLvl > 0); //TODO: When skill points are in, check that against 0
    }

    

    public void addItem(Artifact_scr item)
    {
        playerInventory.Add(item);
        Debug.Log("Received " + item);
    }

    private IEnumerator useArtifacts()
    {
        yield return new WaitForSeconds(useArtifactPeriod);
        foreach (Artifact_scr artifact in playerInventory)
            if (!artifact.useAbility())
                playerInventory.Remove(artifact);
    }

    private IEnumerator toggleInvulnerable(float time)
    {
        invulnerable = true;
        yield return new WaitForSeconds(time);
        invulnerable = false;
    }
}
