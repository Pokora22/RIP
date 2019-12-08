using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer AudioMixer;

    private Slider masterVolumeSlider, musicVolumeSlider, sfxVolumeSlider;
    private TMP_Dropdown qualityDropdown;

    private void Start()
    {
        masterVolumeSlider = GameObject.FindWithTag("MasterVolumeSlider").GetComponent<Slider>();
        musicVolumeSlider = GameObject.FindWithTag("MusicVolumeSlider").GetComponent<Slider>();
        sfxVolumeSlider = GameObject.FindWithTag("SFXVolumeSlider").GetComponent<Slider>();
        qualityDropdown = GameObject.FindWithTag("QualityDropdown").GetComponent<TMP_Dropdown>();
        
        Debug.Log(qualityDropdown);
        Debug.Log(sfxVolumeSlider);

        SyncWithPlayerPrefs();
    }

    public void SyncWithPlayerPrefs()
    {
        float masterVol = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : 1;
        float musicVol = PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 1;
        float sfxVol = PlayerPrefs.HasKey("SFXVolume") ? PlayerPrefs.GetFloat("SFXVolume") : 1;
        int quality = PlayerPrefs.HasKey("QualityLevel")
            ? PlayerPrefs.GetInt("QualityLevel")
            : QualitySettings.GetQualityLevel();        
        
        masterVolumeSlider.SetValueWithoutNotify(masterVol);
        SetMasterVolume(masterVol);
        
        musicVolumeSlider.SetValueWithoutNotify(musicVol);
        SetMusicVolume(musicVol);
        
        sfxVolumeSlider.SetValueWithoutNotify(sfxVol);
        SetSFXVolume(sfxVol);
        
        qualityDropdown.SetValueWithoutNotify(quality);
        SetQuality(quality);
    }

    public void SetMasterVolume(float volume)
    {
        AudioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        AudioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        AudioMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality * 2);
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
    }
}
