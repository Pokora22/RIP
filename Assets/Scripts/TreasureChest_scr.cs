using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest_scr : MonoBehaviour
{
    private Artifact itemInside;
    private Animator animator;
    private Inventory_scr inventory;
    private bool hasTreasure = false, hasTrap = false;

    [SerializeField] private GameObject trap;
    [SerializeField] private Artifact healthPotion;
    [SerializeField] private float artifactChance = .5f, trapChance = .5f, healthChance = .3f;
    void Start()
    {
        animator = GetComponent<Animator>();
        inventory = GameObject.FindWithTag("GameManager").GetComponent<Inventory_scr>();
        int artifactCount = inventory.allArtifacts.Count;
        float rng = Random.Range(0, 1f);
        if (artifactCount > 0 && rng < artifactChance)
        {
            int index = Random.Range(0, artifactCount);
            itemInside = inventory.allArtifacts[index];
            inventory.allArtifacts.RemoveAt(index);
            hasTreasure = true;
        }
        else if (rng < healthChance)
        {
            itemInside = healthPotion;
            hasTreasure = true;
        }
        else
        {
            hasTrap = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool("Open", true);
            if (hasTreasure)
            {
                if(inventory.AddItem(itemInside))
                    hasTreasure = false;
            }
            else if (hasTrap)
            {
                Instantiate(trap, transform.position, transform.rotation);
                hasTrap = false;
            }
        }
    }
}
