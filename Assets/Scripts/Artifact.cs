using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "New Artifact", menuName = "Artifact")]
public class Artifact : ScriptableObject
{
    public string m_name, m_description;
    public Sprite m_spriteActive, m_spriteInactive;
    public enum TYPES
    {
        THORNS, HEALING_STAFF, ENERGY_SHIELD, LIFE_LEECH, PHASING_CLOAK, HEALTH_POTION, BOOTS_OF_HASTE
    }

    public TYPES type;
    
    /*
     * TODO: Need to design a plan for artifacts and what needs to be included in here (stats and methods to use them)
     */
    
    public bool activate()
    {
        switch (type)
        {
            case TYPES.HEALTH_POTION:
                return GameObject.FindWithTag("GameManager").GetComponent<pAttributes_scr>().heal();
                
        }
        //TODO: Add code for different items
        //TODO: Add option for finite items
        return false;
    }

    public bool deactivate()
    {

        return false;
    }

    public override string ToString()
    {
        return m_name +": " + m_description;
    }
}
