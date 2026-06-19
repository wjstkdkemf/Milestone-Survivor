using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class AudioSettings : MonoBehaviour
{
    public const string MasterVolumeKey = "MasterVolume";
    public const string SFXVolumeKey = "SFXVolume";
    public const string BGMVolumeKey = "BGMVolume";

    public AudioMixer Mixer;
    [Header("UI 슬라이더 연결 (동기화용)")]
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider bgmSlider;
    private void Start()
    {
        float savedMaster = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float savedSFX = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        float savedBGM = PlayerPrefs.GetFloat(BGMVolumeKey, 1f);

        ApplySavedVolumes(Mixer);

        if (masterSlider != null) masterSlider.SetValueWithoutNotify(savedMaster);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(savedSFX);
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(savedBGM);
    }
    public void SetVolumeMaster(float Volume)
    {
        SetMixerVolume(Mixer, "Master", Volume);
        PlayerPrefs.SetFloat(MasterVolumeKey, Volume); 
    }
    

    public void SetVolumeEffects(float Volume)
    {
        SetMixerVolume(Mixer, "SFX", Volume);
        PlayerPrefs.SetFloat(SFXVolumeKey, Volume);
    }


    public void SetVolumeMusic(float Volume)
    {
        SetMixerVolume(Mixer, "BGM", Volume);
        PlayerPrefs.SetFloat(BGMVolumeKey, Volume);
    }

    public static void ApplySavedVolumes(AudioMixer mixer)
    {
        if (mixer == null)
            return;

        SetMixerVolume(mixer, "Master", PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        SetMixerVolume(mixer, "SFX", PlayerPrefs.GetFloat(SFXVolumeKey, 1f));
        SetMixerVolume(mixer, "BGM", PlayerPrefs.GetFloat(BGMVolumeKey, 1f));
    }

    public static void SetMixerVolume(AudioMixer mixer, string exposedParameter, float volume)
    {
        if (mixer == null)
            return;

        mixer.SetFloat(exposedParameter, ToDecibel(volume));
    }

    private static float ToDecibel(float volume)
    {
        return volume <= 0.001f ? -80f : Mathf.Log10(volume) * 20f;
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}
