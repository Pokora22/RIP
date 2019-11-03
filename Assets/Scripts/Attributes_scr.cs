using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        
        health = maxHealth;
        summoner = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController_scr>();
        playerAttr = GameObject.FindGameObjectWithTag("Player").GetComponent<pAttributes_scr>();
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
        health -= dmgAmnt;

        if (health <= 0)
        {
            audioPlayer.playClip(NpcAudio_scr.CLIP_TYPE.DEATH); //TODO: Add sounds for destructibles
            if (LayerMask.LayerToName(gameObject.layer) == "Destructibles")
            {
                StartCoroutine(removeBody());
                return;
            }

            m_AiAnimatorScr.setDeadAnim();
            if(debug)
                Debug.Log(gameObject.name + " dropped dead");
            gameObject.layer = LayerMask.NameToLayer("Bodies");
            
            if (gameObject.CompareTag("Enemy"))
            {
                gameObject.GetComponent<EnemyAIControl>().CurrentState = EnemyAIControl.ENEMY_STATE.NONE;
                playerAttr.addExp(expValue);
            }
            
            else if (gameObject.CompareTag("Minion"))
            {
                gameObject.GetComponent<SummonAIControl>().CurrentState = SummonAIControl.MINION_STATE.NONE;
                summoner.minionRemove(gameObject);
                gameObject.GetComponent<Collider>().enabled = false;
                StartCoroutine(removeBody());
            }
        }
        else if (!alertUsed && CompareTag("Enemy"))
            alertAllies();
    }

    public void damage(float dmgAmnt, Attributes_scr minionAttacking) //Attack with reflect
    {
        if(debug)
            Debug.Log(gameObject.name + " received " +dmgAmnt + " dmg from " + minionAttacking.gameObject.name);
            
        damage(dmgAmnt);
        minionAttacking.damage(reflectDamage); //Don't reflect from reflect        

        if (gameObject.CompareTag("Enemy")){
            gameObject.GetComponent<EnemyAIControl>().SetTarget(minionAttacking.gameObject);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (health > 0 && other.CompareTag("AttackHitbox") && !friendlyFire(other.gameObject))
        {
            Attributes_scr attackerAttr = other.GetComponentInParent<Attributes_scr>();
            
            if(CompareTag("Player"))
                GetComponent<pAttributes_scr>().damage(); //Forward to player damage functions instead
            else
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
//        Rigidbody rb = GetComponent<Rigidbody>(); //TODO: This still doesn't sink into ground.. why?

//        rb.detectCollisions = false;
//        while (transform.position.y > -1)
//        {
//            rb.position += Vector3.down * Time.deltaTime; 
//            yield return null;
//        }
        
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
}
