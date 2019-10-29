using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class Attributes_scr : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxHealth = 10f;
    public float attackDamage = 1f;
    public float attackSpeed = 2f;
    public float reflectDamage = 3f;
    public float expValue = 5f; //Change for different minions when they're in
    public GameObject corpse;
    
    [SerializeField]private float health;
    [SerializeField] private bool debug;
    private pAttributes_scr playerAttr;
    private SummonControl_scr summoner;
    private GameObject attacker;
    private SummonAnimator_scr AnimatorScr;
    
    void Start()
    {
        health = maxHealth;
        summoner = GameObject.FindGameObjectWithTag("Player").GetComponent<SummonControl_scr>();
        playerAttr = GameObject.FindGameObjectWithTag("Player").GetComponent<pAttributes_scr>();
        AnimatorScr = gameObject.GetComponent<SummonAnimator_scr>();
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
            if(debug)
                Debug.Log(gameObject.name + " dropped dead");

            //Todo: Destroy and replace with something else depending on what was destroyed? Minion - nothing; Mobs - bodies
            if (gameObject.CompareTag("Enemy"))
            {
                Instantiate(corpse, transform.position, transform.rotation);
                playerAttr.addExp(expValue);
                Destroy(gameObject);
            }
            
            else if (gameObject.CompareTag("Minion"))
            {
                summoner.minionRemove(gameObject);
                AnimatorScr.setDeadAnim(true);
            }
        }        
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
}
