using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class TreasureChest_scr : MonoBehaviour
{
    private Artifact_scr itemInside;
    private Animator animator;
    private bool hasTreasure = false;
    void Start()
    {
        animator = GetComponent<Animator>();
        int artifactCount = Inventory_scr.allArtifacts.Count;
        if (artifactCount > 0)
        {
            itemInside = Inventory_scr.allArtifacts[Random.Range(0, artifactCount)];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool("Open", true);
            if (hasTreasure)
            {
                other.GetComponent<pAttributes_scr>().addItem(itemInside);
            }
        }
    }
}
