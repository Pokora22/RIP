using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcAudio_scr : MonoBehaviour
{
    [SerializeField] private AudioClip[] clipsRaise, clipsAttack, clipsDeath, clipsAlert;

    public enum CLIP_TYPE{RAISE, ATTACK, DEATH, ALERT}
    
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
                if(clipsRaise.Length > 0)
                    m_AudioSource.PlayOneShot(clipsRaise[Random.Range(0, clipsRaise.Length)], Random.Range(.8f, 1.2f));
                break;
            case CLIP_TYPE.ATTACK:
                if(clipsAttack.Length > 0)
                    m_AudioSource.PlayOneShot(clipsAttack[Random.Range(0, clipsAttack.Length)], Random.Range(.8f, 1.2f));
                break;
            case CLIP_TYPE.DEATH:
                if(clipsDeath.Length > 0)
                    m_AudioSource.PlayOneShot(clipsDeath[Random.Range(0, clipsDeath.Length)], Random.Range(.8f, 1.2f));
                break;
            case CLIP_TYPE.ALERT:
                if(clipsAlert.Length > 0)
                    m_AudioSource.PlayOneShot(clipsAlert[Random.Range(0, clipsAlert.Length)], Random.Range(.8f, 1.2f));
                break;
        }
    }
}
