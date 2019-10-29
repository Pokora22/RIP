using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AudioManager_scr : MonoBehaviour
{
    //TODO: This should possible not be kept between scenes, but have an instance in each scene? Otherwise - find a callback from scene manager (Lookup OnSceneLoaded / OnEnable)
    public AudioClip[] music;

    private AudioSource audioSource;
    
    public int startingPitch = 4;
    public int timeToDecrease = 5;


    private void OnEnable()
    {
        
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        audioSource.clip = music[scene.buildIndex];
        audioSource.Play();
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
