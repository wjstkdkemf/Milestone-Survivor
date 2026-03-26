using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FirstTimeSetup : MonoBehaviour
{
    private const string FirstTimeKey = "HasOpenedBefore";
    [SerializeField] private GameObject GameManager;
    public AudioMixer Mixer;
    public void Start()
    {
        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 0f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0f);
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 0f);

        Mixer.SetFloat("Master", savedMaster);
        Mixer.SetFloat("SFX", savedSFX);
        Mixer.SetFloat("BGM", savedBGM);

        Resolution[] resolutions = Screen.resolutions;
        
        int defaultResolutionIndex = resolutions.Length - 1; 

        int savedResIndex = PlayerPrefs.GetInt("ResolutionPreference", defaultResolutionIndex);
        int savedFullScreenInt = PlayerPrefs.GetInt("FullscreenPreference", 1);
        bool isFullScreen = savedFullScreenInt == 1; 

        if (savedResIndex >= resolutions.Length) 
        {
            savedResIndex = defaultResolutionIndex;
        }

        Screen.SetResolution(resolutions[savedResIndex].width, resolutions[savedResIndex].height, isFullScreen);
        
        Debug.Log("게임 부팅 완료: 저장된 해상도 및 오디오 적용됨");
    }
    /*void Start()
    {
        if (!PlayerPrefs.HasKey(FirstTimeKey))
        {
            // First time running the game
            RunFirstTimeSetup();

            // Set the flag to indicate the game has been opened
            PlayerPrefs.SetInt(FirstTimeKey, 1);
            PlayerPrefs.Save(); // Save changes to PlayerPrefs
        }
        else
        {
            Debug.Log("Game has been opened before.");
        }
    }
    */
    private void RunFirstTimeSetup()
    {
        GameManager.GetComponent<CharacterSelectionManager>().ResetCharacters();
        GameManager.GetComponent<PowerUpManager>().RefundPowerUp();
        GameManager.GetComponent<StageSelection>().ResetStages();
    }
}
