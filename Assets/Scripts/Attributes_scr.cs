using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;
using AI;
using Random = UnityEngine.Random;

public class Attributes_scr : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxHealth = 10f;
    public float health;
    public float attackDamage = 1f;
    public float attackSpeed = 2f;
    public float moveSpeedMultiplier = 1f;
    public float reflectDamage = 3f;
    public float lifeLeech = 0;
    public float expValue = 5f; //Change for different minions when they're in
    public bool modified = false;
    public GameObject corpse;
    
    [SerializeField] private bool debug;
    [SerializeField] private float alertRange = 20;
    [SerializeField] private float alertChance = 20;
    [SerializeField] private float removeDelay = 5;
    private NpcAudio_scr audioPlayer;
    private bool alertUsed = false;
    private PlayerAttributes_scr playerAttr;
    private PlayerController_scr player;
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
            float diffMod = GameManager_Scr.DifficultyMod;
            maxHealth += maxHealth * (diff * diffMod / 5); //Add 20% stats per difficulty lvl
            attackDamage += attackDamage * (diff * diffMod/ 5);
            attackSpeed += attackSpeed * (diff * diffMod/ 5);
            moveSpeedMultiplier += moveSpeedMultiplier * (diff * diffMod/ 5);
        }
        else if (CompareTag("Minion"))
        {
            maxHealth += ZombieAttributes.healthMod;
            attackDamage += ZombieAttributes.attackMod;
            attackSpeed += ZombieAttributes.attackSpeedMod;
            moveSpeedMultiplier += ZombieAttributes.moveSpeedMod / moveSpeedMultiplier;
        }

        if (CompareTag("Enemy") || CompareTag("Minion"))
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Agent = GetComponent<NavMeshAgent>();
        }

        m_Collider = GetComponent<Collider>();
        
        health = maxHealth;
        
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_scr>();
        playerAttr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerAttributes_scr>();
        
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
        if (CompareTag("Player"))
            playerAttr.damage(); //Forward to player damage functions instead
        else
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
                    player.minionRemove(GetComponent<SummonAIControl>());

                Instantiate(corpse, transform.position, transform.rotation);
                Destroy(gameObject);
            }
            else
            {
                if (!alertUsed && CompareTag("Enemy"))
                    alertAllies();
            }
        }
    }

    public void damage(float dmgAmnt, Attributes_scr attacker) //Attack with reflect
    {
        if(debug)
            Debug.Log(gameObject.name + " received " +dmgAmnt + " dmg from " + attacker.gameObject.name);
            
        damage(dmgAmnt);
        if(health <= 0)
            attacker.grantHealth();
        attacker.damage(reflectDamage); //Don't reflect from reflect
    }

    public void grantHealth()
    {
        health = health + lifeLeech > maxHealth ? maxHealth : health + lifeLeech;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!invulnerable && health > 0 && other.CompareTag("AttackHitbox") && !friendlyFire(other.gameObject))
        {
            StartCoroutine(toggleInvulnerable(invulnerableTime));
            Attributes_scr attackerAttr = other.GetComponentInParent<Attributes_scr>();
            
            if (attackerAttr)
                this.damage(attackerAttr.attackDamage, attackerAttr);
        }
    }

    private bool friendlyFire(GameObject other)
    {           
        return gameObject.layer == other.layer;
    }

    private IEnumerator removeBody()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
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
                    if (TryGetComponent(out PatrolAIControl patrolAi))
                        patrolAi.setNewDestination(transform.position);
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
