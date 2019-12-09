using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager_Scr : MonoBehaviour
{
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

        if (scene.buildIndex == 0)
            Destroy(GameObject.FindWithTag("GameManager"));
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
