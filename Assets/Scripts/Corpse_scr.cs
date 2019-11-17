using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Corpse_scr : MonoBehaviour
{
    [SerializeField] private float bodyStayTime = 3f;
    private NpcAudio_scr m_AudioPlayer;
    
    private void Start()
    {
        if (CompareTag("Minion"))
            StartCoroutine(removeBody());
    }

    private IEnumerator removeBody()
    {
        yield return new WaitForSeconds(bodyStayTime);

        float startPosY = transform.position.y;
        while (transform.position.y > startPosY - 10)
        {
            transform.position += Vector3.down * Time.deltaTime; 
            yield return null;
        }
        
        Destroy(gameObject);
        yield return null;
    }

    private void OnDestroy()
    {
        //Animate container if corpse is in container
        if (transform.parent && transform.parent.CompareTag("CorpseContainer"))
        {
            Animator animator = transform.parent.GetComponentInChildren<Animator>();
            animator.SetBool("open", true);
        }
    }
}
