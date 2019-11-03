using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_scr : MonoBehaviour
{
    public static List<Artifact_scr> allArtifacts;
    
    // Start is called before the first frame update
    void Awake()
    {
        allArtifacts = new List<Artifact_scr>();
        allArtifacts.Add(new Artifact_scr("Item 1"));
        allArtifacts.Add(new Artifact_scr("Item 2"));
        allArtifacts.Add(new Artifact_scr("Item 3"));
        allArtifacts.Add(new Artifact_scr("Item 4"));
        allArtifacts.Add(new Artifact_scr("Item 5"));
        allArtifacts.Add(new Artifact_scr("Item 6"));
        Debug.Log("Added inventory items. Currently: " + allArtifacts.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
