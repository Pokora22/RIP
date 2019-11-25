using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private float duration = 5f, range = 3f, damage = 1f;
    [SerializeField] private LayerMask canBeDamaged;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnTrap());
    }
    
    private IEnumerator SpawnTrap()
    {
        for (int i = 0; i < duration; i++)
        {
            yield return new WaitForSeconds(1f);
            
            Collider[] characters = Physics.OverlapSphere(transform.position, range, canBeDamaged);
            foreach (Collider c in characters)
            {
                c.GetComponent<Attributes_scr>().damage(damage);
            }
        }
    }
}
