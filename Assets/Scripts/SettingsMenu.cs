using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer AudioMixer;

    private Slider masterVolumeSlider, musicVolumeSlider, sfxVolumeSlider;
    private Dropdown qualityDropdown;

    private void Start()
    {
        masterVolumeSlider = GameObject.FindWithTag("MasterVolumeSlider").GetComponent<Slider>();
        musicVolumeSlider = GameObject.FindWithTag("MusicVolumeSlider").GetComponent<Slider>();
        sfxVolumeSlider = GameObject.FindWithTag("SFXVolumeSlider").GetComponent<Slider>();
        qualityDropdown = GameObject.FindWithTag("QualityDropdown").GetComponent<Dropdown>();

        float masterVol = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : 0;
        float musicVol = PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 0;
        float sfxVol = PlayerPrefs.HasKey("SFXVolume") ? PlayerPrefs.GetFloat("SFXVolume") : 0;
        int quality = PlayerPrefs.HasKey("QualityLevel")
            ? PlayerPrefs.GetInt("QualityLevel")
            : QualitySettings.GetQualityLevel();
        
        masterVolumeSlider.SetValueWithoutNotify(masterVol);
        musicVolumeSlider.SetValueWithoutNotify(musicVol);
        sfxVolumeSlider.SetValueWithoutNotify(sfxVol);
        qualityDropdown.SetValueWithoutNotify(quality);
    }

    public void SetMasterVolume(float volume)
    {
        AudioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }
    
    public void SetMusicVolume(float volume)
    {
        AudioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }
    
    public void SetSFXVolume(float volume)
    {
        AudioMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    public void SetQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality * 2);
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
    }
}
