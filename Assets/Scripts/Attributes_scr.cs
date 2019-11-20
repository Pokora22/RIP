﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Attributes_scr : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxHealth = 10f;
    public float health;
    public float attackDamage = 1f;
    public float attackSpeed = 2f;
    public float moveSpeedMultiplier = 1f;
    public float reflectDamage = 3f;
    public float expValue = 5f; //Change for different minions when they're in
    public GameObject corpse;
    
    [SerializeField] private bool debug;
    [SerializeField] private float alertRange = 20;
    [SerializeField] private float alertChance = 20;
    [SerializeField] private float removeDelay = 5;
    private NpcAudio_scr audioPlayer;
    private bool alertUsed = false;
    private pAttributes_scr playerAttr;
    private PlayerController_scr summoner;
    private GameObject attacker;
    private AiAnimator_scr m_AiAnimatorScr;
    [SerializeField] private float invulnerableTime = 0.5f;
    private bool invulnerable;
    private Rigidbody m_Rigidbody;
    private NavMeshAgent m_Agent;
    private Collider m_Collider;

    void Start()
    {
        if (CompareTag("Enemy"))
        {
            float diff = PlayerPrefs.GetInt("difficulty");
            maxHealth += maxHealth * (diff / 5); //Add 20% stats per difficulty lvl
            attackDamage += attackDamage * (diff / 5);
            attackSpeed += attackSpeed * (diff / 5);
            moveSpeedMultiplier += moveSpeedMultiplier * (diff / 5);
        }

        if (CompareTag("Enemy") || CompareTag("Minion"))
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Agent = GetComponent<NavMeshAgent>();
        }

        m_Collider = GetComponent<Collider>();
        
        health = maxHealth;
        summoner = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_scr>();
        playerAttr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<pAttributes_scr>();
        m_AiAnimatorScr = gameObject.GetComponent<AiAnimator_scr>();
        audioPlayer = GetComponent<NpcAudio_scr>();
    }

    // private void OnCollisionEnter(Collision other)
    // {
    //     if (other.gameObject.CompareTag("Barricade") && other.GetContact(0).normal == other.transform.right) //transform.right is the spiky side
    //         damage(other.gameObject.GetComponent<Attributes_scr>().attackDamage);
    // }

    public void damage(float dmgAmnt)
    {
        if (!invulnerable)
        {
            health -= dmgAmnt;

            if (health <= 0)
            {
                audioPlayer.playClip(NpcAudio_scr.CLIP_TYPE.DEATH); //TODO: Add sounds for destructibles
                if (LayerMask.LayerToName(gameObject.layer) == "Destructibles")
                {
                    StartCoroutine(removeBody());
                    return;
                }
                
                if (debug)
                    Debug.Log(gameObject.name + " dropped dead");
                
                if (gameObject.CompareTag("Enemy"))
                    playerAttr.addExp(expValue);
                else if (gameObject.CompareTag("Minion"))
                    summoner.minionRemove(GetComponent<SummonAIControl>());
                
                Instantiate(corpse, transform.position, transform.rotation);
                Destroy(gameObject);
            }
            else
            {
                if (!alertUsed && CompareTag("Enemy"))
                    alertAllies();

                StartCoroutine(toggleInvulnerable(invulnerableTime));
            }
        }
    }

    public void damage(float dmgAmnt, Attributes_scr minionAttacking) //Attack with reflect
    {
        if(debug)
            Debug.Log(gameObject.name + " received " +dmgAmnt + " dmg from " + minionAttacking.gameObject.name);
            
        damage(dmgAmnt);
        minionAttacking.damage(reflectDamage); //Don't reflect from reflect
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (health > 0 && other.CompareTag("AttackHitbox") && !friendlyFire(other.gameObject))
        {
            Attributes_scr attackerAttr = other.GetComponentInParent<Attributes_scr>();
            
            if(CompareTag("Player"))
                playerAttr.damage(); //Forward to player damage functions instead
            else if (attackerAttr)
                this.damage(attackerAttr.attackDamage, attackerAttr);
        }
    }

    private bool friendlyFire(GameObject other)
    {
        return transform.root.CompareTag(other.transform.root.tag);
    }

    private IEnumerator removeBody()
    {
        yield return new WaitForSeconds(removeDelay);
        
        while (transform.position.y > -10)
        {
            transform.position += Vector3.down * Time.deltaTime; 
            yield return null;
        }
        
        Destroy(gameObject);
        yield return null;
    }

    private void alertAllies()
    {
        if (Random.Range(0, 100) < alertChance)
        {
            float alert = Random.Range(0, maxHealth);
            int diff = PlayerPrefs.GetInt("difficulty");
            if (alert > 1 / (diff + 1) * health)
            {
                alertUsed = true;
                audioPlayer.playClip(NpcAudio_scr.CLIP_TYPE.ALERT);
                Collider[] nearbyAllies = Physics.OverlapSphere(transform.position, alertRange, gameObject.layer);
                foreach (Collider ally in nearbyAllies)
                {
                    ally.GetComponent<EnemyAIControl>().setNewDestination(transform.position);
                }
            }
        }
    }
    
    private IEnumerator toggleInvulnerable(float time)
    {
        invulnerable = true;
        yield return new WaitForSeconds(time);
        invulnerable = false;
    }
}
