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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override string ToString()
    {
        return itemDescription;
    }
}
