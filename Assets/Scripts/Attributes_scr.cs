using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attributes_scr : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxHealth = 10f;
    private float health { get; set; }
    public float attackDamage = 1f;
    void Start()
    {
        health = maxHealth;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Barricade")) 
            damage(other.gameObject.GetComponent<Attributes_scr>().attackDamage);
    }

    public void damage(float dmgAmnt)
    {
        health -= dmgAmnt;

        if (health <= 0)
        {
            Destroy(gameObject);
            //Todo: Destroy and replace with something else depending on what was destroyed? Minion - nothing; Mobs - bodies
        }
    }
    
}
