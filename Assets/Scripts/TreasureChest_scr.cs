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
            int index = Random.Range(0, artifactCount);
            itemInside = Inventory_scr.allArtifacts[index];
            Inventory_scr.allArtifacts.RemoveAt(index);
            hasTreasure = true;
            Debug.Log(itemInside);
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
