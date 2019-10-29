using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcAudio_scr : MonoBehaviour
{
    [SerializeField] private AudioClip[] clipsRaise, clipsZombieAttack, clipsHumanAttack, clipsZombieDeath, clipsHumanDeath;

    public enum CLIP_TYPE{RAISE, ATTACK, DEATH}
    
    private AudioSource m_AudioSource;
    // Start is called before the first frame update
    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void playClip(CLIP_TYPE type)
    {
        float currentPitch = m_AudioSource.pitch;
        m_AudioSource.pitch = Random.Range(.8f, 1.2f);
        
        switch (type) 
        {
            case CLIP_TYPE.RAISE:
                m_AudioSource.PlayOneShot(clipsRaise[Random.Range(0, clipsRaise.Length)], Random.Range(.8f, 1.2f));
                break;
            case CLIP_TYPE.ATTACK:
                if(CompareTag("Minion"))
                    m_AudioSource.PlayOneShot(clipsZombieAttack[Random.Range(0, clipsZombieAttack.Length)], Random.Range(.8f, 1.2f));
                else
                    m_AudioSource.PlayOneShot(clipsHumanAttack[Random.Range(0, clipsHumanAttack.Length)], Random.Range(.8f, 1.2f));
                break;
            case CLIP_TYPE.DEATH:
                if(CompareTag("Minion"))
                    m_AudioSource.PlayOneShot(clipsZombieDeath[Random.Range(0, clipsZombieDeath.Length)], Random.Range(.8f, 1.2f));
                else
                    m_AudioSource.PlayOneShot(clipsHumanDeath[Random.Range(0, clipsHumanDeath.Length)], Random.Range(.8f, 1.2f));
                break;
        }
    }
}
