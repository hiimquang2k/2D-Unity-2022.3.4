using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Settings Controls")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TMP_Dropdown graphicsDropdown;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [SerializeField] private PlayerData playerData;
    [SerializeField] private Button continueButton;
    private Resolution[] resolutions;

    private void Start()
    {
        // Initialize settings with saved values
        InitializeSettings();
        continueButton.interactable = playerData.saveState.hasSave;
    }

    public void OpenSettings()
    {
        AudioManager.Instance.PlayButtonPressSound();
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        AudioManager.Instance.PlayButtonPressSound();
        settingsPanel.SetActive(false);
    }

    public void ApplySettings()
    {
        // Apply volume settings
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(masterVolumeSlider.value);
            AudioManager.Instance.SetMusicVolume(musicVolumeSlider.value);
            AudioManager.Instance.SetSFXVolume(sfxVolumeSlider.value);
        }

        // Apply graphics settings
        PlayerPrefs.SetInt("QualityLevel", graphicsDropdown.value);
        QualitySettings.SetQualityLevel(graphicsDropdown.value);

        // Apply resolution and fullscreen
        Resolution resolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(resolution.width, resolution.height, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);

        // Save all settings
        PlayerPrefs.Save();

        CloseSettings();
    }

    private void InitializeSettings()
    {
        // Setup resolution dropdown
        SetupResolutionDropdown();

        // Setup graphics dropdown
        SetupGraphicsDropdown();

        // Load saved settings
        LoadSettings();
    }

    private void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupGraphicsDropdown()
    {
        graphicsDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            options.Add(QualitySettings.names[i]);
        }

        graphicsDropdown.AddOptions(options);
    }

    private void LoadSettings()
    {
        // Load volume settings
        if (AudioManager.Instance != null)
        {
            masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
            musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
            sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
        }

        // Load graphics settings
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        graphicsDropdown.value = qualityLevel;

        // Load fullscreen setting
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }

    // Your existing methods (StartGame, QuitGame, etc.)
    public void StartNewGame()
    {
        AudioManager.Instance.PlayButtonPressSound();
        // Transition to the Intro scene
        SceneTransitionManager.TransitionToScene("IntroScene", Vector3.zero);
    }

    public void OpenOptions()
    {
        // Open options menu or panel
    }

    public void ContinueGame()
    {
        AudioManager.Instance.PlayButtonPressSound();
        if (playerData.saveState.hasSave)
        {
            GameManager.Instance.LoadGame();
        }
        else
        {
            Debug.LogWarning("No save data to continue from!");
            // Optionally show a UI message to the player
        }
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlayButtonPressSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}