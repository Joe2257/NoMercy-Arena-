using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

//The MainMenu script manage all the function of the main menu of the game like: buttons and settings.
public class MainMenu : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject _settingsWindow;
    [SerializeField] private GameObject _quitWindow;

    [Header("Settings")]
    [SerializeField] private Dropdown _resolutionDropDown;
    [SerializeField] private Slider   _volumeSlider;
    [SerializeField] private Dropdown _qualityDropDown;

    
    private Resolution[] _resolutions;

    List<string> _resolutionOptions = new List<string>();

    public AudioMixer audioMixer;

    void Start()
    {
        InitializeSettingsOptions();
    }

    //____________Buttons_____________\\

    public void OnStartButtonClick()
    {
        SceneManager.LoadScene("Level_1");
    }

    public void OnSettingsButtonClick()
    {
        _settingsWindow.SetActive(true);

        _qualityDropDown.value = QualitySettings.GetQualityLevel();
    }

    public void OnCloseSettingsButtonClick()
    {
        _settingsWindow.SetActive(true);
    }

    public void OnCreditsButtonClick()
    {
        SceneManager.LoadScene("Credits");
    }

    public void OnQuitButtonClick()
    {
        _quitWindow.SetActive(true);
    }

    public void CloseQuitWindow()
    {
        _quitWindow.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    //____________Settings_____________\\

    private void InitializeSettingsOptions()
    {
        //SetVolume
        if (!PlayerPrefs.HasKey("Volume"))
        {
            _volumeSlider.value = 0f;
            audioMixer.SetFloat("MasterMixer", _volumeSlider.value);
        }
        else
        {
            _volumeSlider.value = PlayerPrefs.GetFloat("Volume");
            audioMixer.SetFloat("MasterMixer", _volumeSlider.value);
        }

        //SetQuality
        if (!PlayerPrefs.HasKey("Quality"))
        {
            QualitySettings.SetQualityLevel(2);

            if(_settingsWindow.activeSelf)
                _qualityDropDown.value = QualitySettings.GetQualityLevel();
        }
        else
          QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality"));


        //SetResolution
        if(!PlayerPrefs.HasKey("Resolution"))
        {
            _resolutions = Screen.resolutions;
            _resolutionDropDown.ClearOptions();

            int currentResolutionIndex = 0;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                string option = _resolutions[i].width + " x " + _resolutions[i].height;

                _resolutionOptions.Add(option);

                if (_resolutions[i].width == Screen.currentResolution.width &&
                    _resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            _resolutionDropDown.AddOptions(_resolutionOptions);
            _resolutionDropDown.value = currentResolutionIndex;
            _resolutionDropDown.RefreshShownValue();
        }
       
    }

    //____________SettingsFunctions_____________\\

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = _resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, true);
    }

    public void SetVolumeFromSlider(float volume)
    {
        audioMixer.SetFloat("MasterMixer", volume);
    }

    public void SetQualityIndex(int qualityIndex)
    {
        qualityIndex = _qualityDropDown.value;

        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void CloseAndSaveSettings()
    {
        PlayerPrefs.SetFloat("Volume",     _volumeSlider.value);
        PlayerPrefs.SetInt  ("Quality",    _qualityDropDown.value);

        _settingsWindow.SetActive(false);
    }

    public void ResetSettings()
    {
        PlayerPrefs.DeleteAll();

        InitializeSettingsOptions();
    }
}
