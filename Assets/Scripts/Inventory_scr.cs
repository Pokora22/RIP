using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_scr : MonoBehaviour
{
    public static List<Artifact_scr> allArtifacts = new List<Artifact_scr>();
    
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void printInventory()
    {
        Debug.Log("------------------");
        string inventory = "";
        foreach (var artifactScr in allArtifacts)
        {
            inventory += artifactScr.ToString() + " ";
        }
        Debug.Log(inventory);
        Debug.Log("------------------");
    }
}
