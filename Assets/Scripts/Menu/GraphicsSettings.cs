using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicsSettings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropDown;
    public Toggle fullScreenToggle;

    Resolution[] resolutions;
    // Start is called before the first frame update
    void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropDown.ClearOptions();

        List<string> options = new List<string>();
        int defaultResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            Debug.Log(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                defaultResolutionIndex = i;
            }
        }
        resolutionDropDown.AddOptions(options);

        int savedResIndex = PlayerPrefs.GetInt("ResolutionPreference", defaultResolutionIndex);
        
        int savedFullScreenInt = PlayerPrefs.GetInt("FullscreenPreference", 1);
        bool isFullScreen = savedFullScreenInt == 1; 
        

        resolutionDropDown.value = savedResIndex;
        resolutionDropDown.RefreshShownValue();
        
        if (fullScreenToggle != null) 
        {
            fullScreenToggle.isOn = isFullScreen;
        }

        Screen.SetResolution(resolutions[savedResIndex].width, resolutions[savedResIndex].height, isFullScreen);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        PlayerPrefs.SetInt("ResolutionPreference", resolutionIndex);
    }
    public void SetQuality(int QualityIndex)
    {

        QualitySettings.SetQualityLevel(QualityIndex);

    }

    public void SetFullScreen(bool isFullScreen)
    {

        Screen.fullScreen = isFullScreen;

        PlayerPrefs.SetInt("FullscreenPreference", isFullScreen ? 1 : 0);
    }
}
