using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class AudioSettings : MonoBehaviour
{
    public AudioMixer Mixer;
    [Header("UI 슬라이더 연결 (동기화용)")]
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider bgmSlider;
    private void Start()
    {
        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 0f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0f);
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 0f);

        Mixer.SetFloat("Master", savedMaster);
        Mixer.SetFloat("SFX", savedSFX);
        Mixer.SetFloat("BGM", savedBGM);

        if (masterSlider != null) masterSlider.value = savedMaster;
        if (sfxSlider != null) sfxSlider.value = savedSFX;
        if (bgmSlider != null) bgmSlider.value = savedBGM;

        gameObject.SetActive(false);
    }
    public void SetVolumeMaster(float Volume)
    {
        if (Volume <= 0.001f)Mixer.SetFloat("Master", -80f); 
        else Mixer.SetFloat("Master", Mathf.Log10(Volume) * 20f);
        PlayerPrefs.SetFloat("MasterVolume", Volume); 
    }
    

    public void SetVolumeEffects(float Volume)
    {
        if (Volume <= 0.001f) Mixer.SetFloat("SFX", -80f);
        else Mixer.SetFloat("SFX", Mathf.Log10(Volume) * 20f);
        PlayerPrefs.SetFloat("SFXVolume", Volume);
    }


    public void SetVolumeMusic(float Volume)
    {
        if (Volume <= 0.001f) Mixer.SetFloat("BGM", -80f);
        else Mixer.SetFloat("BGM", Mathf.Log10(Volume) * 20f);
        PlayerPrefs.SetFloat("BGMVolume", Volume);
    }
    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}
