using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{

    public AudioMixer AudioMixer;

    public void SetMasterVolume(float volume)
    {
        AudioMixer.SetFloat("masterVolume", volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        AudioMixer.SetFloat("musicVolume", volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        AudioMixer.SetFloat("sfxVolume", volume);
    }

    public void SetQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
    }
}
