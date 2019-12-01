using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[CreateAssetMenu(fileName = "New Artifact", menuName = "Artifact/Permanent Minion Mod")]
public class PermanentMinionModifierItem : Artifact
{
    public float lifeLeechMod, maxHealthMod, movementSpeedMod, attackSpeedMod, damageMod, reflectMod;
    
    public override bool activate()
    {
        modifyValues();
        GameObject.FindWithTag("Player").GetComponent<PlayerController_scr>().newMinionEvent.AddListener(modifyValues);
        return false; //Do not remove
    }

    public override bool deactivate()
    {
        modifyValues();
        GameObject.FindWithTag("Player").GetComponent<PlayerController_scr>().newMinionEvent.RemoveListener(modifyValues);
        return false; //Do not remove
    }

    private void modifyValues()
    {
        GameObject[] minions = GameObject.FindGameObjectsWithTag("Minion");

        foreach (GameObject minion in minions)
        {
            Attributes_scr attributes = minion.GetComponent<Attributes_scr>();

            if (!attributes.modified)
            {
                attributes.maxHealth += maxHealthMod;
                attributes.health += maxHealthMod;
                attributes.attackDamage += damageMod;
                attributes.reflectDamage += reflectMod;
                attributes.moveSpeedMultiplier += movementSpeedMod;
                attributes.attackSpeed += attackSpeedMod;
                attributes.lifeLeech += lifeLeechMod;
            }
            else
            {
                attributes.maxHealth -= maxHealthMod;
                attributes.health -= maxHealthMod;
                attributes.attackDamage -= damageMod;
                attributes.reflectDamage -= reflectMod;
                attributes.moveSpeedMultiplier -= movementSpeedMod;
                attributes.attackSpeed -= attackSpeedMod;
                attributes.lifeLeech -= lifeLeechMod;
            }

            Debug.Log("Minion attributes modified");
            attributes.modified = !attributes.modified;
        }
    }
}
