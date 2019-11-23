using UnityEngine;

public class TreasureChest_scr : MonoBehaviour
{
    private Artifact itemInside;
    private Animator animator;
    private Inventory_scr inventory;
    private bool hasTreasure = false;
    void Start()
    {
        animator = GetComponent<Animator>();
        inventory = GameObject.FindWithTag("GameManager").GetComponent<Inventory_scr>();
        int artifactCount = inventory.allArtifacts.Count;
        if (artifactCount > 0)
        {
            int index = Random.Range(0, artifactCount);
            itemInside = inventory.allArtifacts[index];
            inventory.allArtifacts.RemoveAt(index);
            hasTreasure = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool("Open", true);
            if (hasTreasure)
            {
                inventory.AddItem(itemInside);
            }
        }
    }
}
