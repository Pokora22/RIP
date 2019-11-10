using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Artifact_scr : MonoBehaviour
{
    [SerializeField] private string itemDescription;
    
    
    /*
     * TODO: Need to design a plan for artifacts and what needs to be included in here (stats and methods to use them)
     */

    public Artifact_scr(string itemDescription)
    {
        this.itemDescription = itemDescription;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Inventory_scr.allArtifacts.Add(this); //TODO: Not sure if I should instantiate it from here
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool useAbility()
    {
        //TODO: Add code for different items
        //TODO: Add option for finite items
        return true;
    }

    public override string ToString()
    {
        return itemDescription;
    }
}
