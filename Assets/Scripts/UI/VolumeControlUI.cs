using UnityEngine;
using UnityEngine.UI;

public class VolumeControlUI : MonoBehaviour
{
    public Slider volumeSlider;

    private void Start()
    {
        // Set initial slider value
        if (AudioManager.Instance != null)
        {
            volumeSlider.value = AudioManager.Instance.GetMasterVolume();
        }
    }

    public void OnVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(volume);
        }
    }

    public void PlayButtonPressSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonPressSound();
        }
    }
}
