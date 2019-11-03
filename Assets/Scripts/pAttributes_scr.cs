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
    public float maxHealth = 3f;
    private float health { get; set; }

    private float currentExp;
    private float currentLvl;
    [SerializeField] private float baseExpReq = 50;
    private float nextLvlExpReq;
    private PlayerController_scr summoner;
    private TextMeshProUGUI hudZombieCount;
    private Rigidbody rb;
    [SerializeField] private Sprite[] phylacteri;
    private GameObject[] livesDisplay;
    private Image expBar;
    private GameObject levelNotification;
    private List<Artifact_scr> playerInventory;

    void Start()
    {
        nextLvlExpReq = baseExpReq;
        health = maxHealth;
        summoner = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_scr>();
        hudZombieCount = GameObject.FindWithTag("UIZombieCount").GetComponentInChildren<TextMeshProUGUI>();
        rb = GetComponent<Rigidbody>();
        livesDisplay = GameObject.FindGameObjectsWithTag("UIPhylactery");
        expBar = GameObject.FindWithTag("UIExpBar").GetComponent<Image>();
        levelNotification = GameObject.FindWithTag("UILevelNotification");
        playerInventory = new List<Artifact_scr>();
        updateHud();
    }

    public void damage()
    {
        //TODO: Sometimes gets hit twice with same effect. Add delay
        if (--health < 1)
            SceneManager.LoadScene(2); //TODO: Maybe a game over scren
        updateHud();
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
        for (int i = 0; i < maxHealth; i++)
            livesDisplay[i].GetComponent<Image>().sprite = i < health ? phylacteri[1] : phylacteri[0];

        expBar.fillAmount = currentExp / nextLvlExpReq;
        
        hudZombieCount.SetText(" x " + (summoner.minions.Count + summoner.minionsAway.Count));
        
        levelNotification.SetActive(currentLvl > 0); //TODO: When skill points are in, check that against 0
    }

    private void OnCollisionEnter(Collision other)
    {
        //TODO: Change barricade tag or figure some other way for this collision to happen
        if (other.gameObject.CompareTag("Barricade") && other.GetContact(0).normal == other.transform.right
        ) //transform.right is the spiky side
        {
            damage();
            rb.velocity = Vector3.zero;
            rb.AddForce((other.transform.right + (Vector3.up/20)) * 50, ForceMode.Impulse); //TODO: Push player back? Adjust force
        }
    }

    public void addItem(Artifact_scr item)
    {
        playerInventory.Add(item);
    }
}
