using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Artifact_scr : MonoBehaviour
{
    public string m_description;
    public Sprite m_sprite;
    
    
    /*
     * TODO: Need to design a plan for artifacts and what needs to be included in here (stats and methods to use them)
     */

    public Artifact_scr(string description)
    {
        m_description = description;
        
    }
    
    public bool useAbility()
    {
        //TODO: Add code for different items
        //TODO: Add option for finite items
        return true;
    }

    public override string ToString()
    {
        return m_description;
    }
}
