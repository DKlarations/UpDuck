using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Slider masterSlider;
    public Slider ambientSlider;
    public Slider sfxSlider;
    void Start()
    {
        float playerMasterVolume = 0f;
        float playerAmbientVolume = 0f;
        float playerSfxVolume = 0f;

        #if UNITY_EDITOR
        playerMasterVolume = GetVolume("Master");
        playerAmbientVolume = GetVolume("Ambient");
        playerSfxVolume = GetVolume("Sfx");
        #else
        playerMasterVolume = PlayerPrefs.GetFloat("vol_Master", GetVolume("Master"));
        playerAmbientVolume = PlayerPrefs.GetFloat("vol_Ambient", GetVolume("Ambient"));
        playerSfxVolume = PlayerPrefs.GetFloat("vol_Sfx", GetVolume("Sfx"));
        #endif

        masterSlider.value = playerMasterVolume;
        ambientSlider.value = playerAmbientVolume;
        sfxSlider.value = playerSfxVolume;
    }
    public AudioMixer audioMixer;
    public void SetMasterVolume (float volume)
    {
        SetVolume("Master", volume);
    }
    public void SetAmbientVolume (float volume)
    {
        SetVolume("Ambient", volume);
    }
    public void SetSfxVolume (float volume)
    {
        SetVolume("Sfx", volume);
    }

    public void SetVolume(string parameterName, float volume)
    {
        Debug.Log($"Setting {parameterName} to {volume} ({Mathf.Log10(volume) * 20} dB)");
        PlayerPrefs.SetFloat($"vol_{parameterName}", volume);
        audioMixer.SetFloat(parameterName, Mathf.Log10(volume) * 20);
    }
    public float GetVolume(string parameterName)
    {
        var existed = audioMixer.GetFloat(parameterName, out var vol);
        vol = Mathf.Pow(10, vol / 20f);
        return vol;
    }
}