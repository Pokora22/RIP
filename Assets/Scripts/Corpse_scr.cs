using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Corpse_scr : MonoBehaviour
{
    private void OnDestroy()
    {
        if (transform.parent && transform.parent.CompareTag("CorpseContainer"))
        {
            Animator animator = transform.parent.GetComponentInChildren<Animator>();
            animator.SetBool("open", true);
        }
    }
}
