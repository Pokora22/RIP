using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager_Scr : MonoBehaviour
{
    //TODO: This should possible not be kept between scenes, but have an instance in each scene? Otherwise - find a callback from scene manager (Lookup OnSceneLoaded / OnEnable)
    public AudioClip[] music;

    private AudioSource audioSource;
    [SerializeField] private int[] baseNumberOfUnitsPerScene;

    public static int BaseNumberOfUnits { get; set; }

    public static float DifficultyMod
    {
        get => _difficultyMod;
        set
        {
            //Static for access, can't initialize min/max values in editor like this
            if (value >= .5f && value <= 5f)
                _difficultyMod = value;
        }
    }

    private static float _difficultyMod;
    


    private void OnEnable()
    {
        DifficultyMod = 1f;
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
        BaseNumberOfUnits = baseNumberOfUnitsPerScene[scene.buildIndex];
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
