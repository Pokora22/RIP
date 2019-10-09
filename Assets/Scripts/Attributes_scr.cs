using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attributes_scr : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxHealth = 10f;
    private float health { get; set; }
    public float attackDamage = 1f;
    public float attackSpeed = 2f;
    public float reflectDamage = 3f;

    private SummonControl_scr summoner;
    void Start()
    {
        health = maxHealth;
        summoner = GameObject.FindGameObjectWithTag("Player").GetComponent<SummonControl_scr>();
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
            Destroy(gameObject);
            //Todo: Destroy and replace with something else depending on what was destroyed? Minion - nothing; Mobs - bodies
        }
    }

    public void damage(float dmgAmnt, Attributes_scr minionAttacking) //Attack with reflect
    {
        health -= dmgAmnt;
        minionAttacking.damage(reflectDamage); //Don't reflect from reflect

        if (health <= 0)
        {
            if(gameObject.CompareTag("Minion"))
                summoner.minionRemove(gameObject);
            
            Destroy(gameObject);
            //Todo: Destroy and replace with something else depending on what was destroyed? Minion - nothing; Mobs - bodies
        }
    }
}
