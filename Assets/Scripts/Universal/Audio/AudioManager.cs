using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    
    [Header("Audio Clips")]
    public AudioClip buttonHoverSound;
    public AudioClip buttonPressSound;
    
    [Header("Volume Settings")]
    public AudioMixer audioMixer;
    [SerializeField] private float masterVolume = 1f; // Master volume (0-1)
    public float musicVolume = 0.7f; // Default music volume
    public float sfxVolume = 1f;    // Default SFX volume

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize volumes
        if (MusicManager.Instance != null)
        {
            SetMusicVolume(musicVolume);
        }
        SetSFXVolume(sfxVolume);
    }

    private void Start()
    {
        // Apply saved volume if any
        LoadVolume();
    }

    public void PlayButtonHoverSound()
    {
        if (sfxSource && buttonHoverSound)
        {
            sfxSource.PlayOneShot(buttonHoverSound);
        }
    }

    public void PlayButtonPressSound()
    {
        if (sfxSource && buttonPressSound)
        {
            sfxSource.PlayOneShot(buttonPressSound);
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        SaveVolume();
        
        // Apply to music volume if MusicManager exists
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetAllTrackVolumes(volume * musicVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetAllTrackVolumes(volume * masterVolume);
        }
        SaveVolume();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        if (sfxSource != null)
        {
            sfxSource.volume = volume * masterVolume;
        }
        SaveVolume();
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }

    private void SaveVolume()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolume()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume");
            SetMasterVolume(masterVolume);
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            SetMusicVolume(musicVolume);
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
            SetSFXVolume(sfxVolume);
        }
    }
}
