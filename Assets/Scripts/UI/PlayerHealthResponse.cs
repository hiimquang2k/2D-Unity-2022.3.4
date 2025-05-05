using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class LowHealthEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 0.7f;

    [Header("SFX")]
    [SerializeField] private AudioClip heartbeatSFX;
    [SerializeField] private AudioClip healthRecoverySFX;
    [SerializeField] [Range(0, 1)] private float sfxVolume = 0.7f;
    [SerializeField] private float heartbeatPitchMin = 0.9f;
    [SerializeField] private float heartbeatPitchMax = 1.1f;

    [Header("References")]
    [SerializeField] private Image bloodSplashImage;
    [SerializeField] private HealthSystem playerHealth;

    private AudioSource _audioSource;
    private bool _isLowHealth;
    private Tween _pulseTween;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void Start()
    {
        bloodSplashImage.color = new Color(1, 1, 1, 0);
    }

    private void Update()
    {
        float healthPercent = (float)playerHealth.CurrentHealth / playerHealth.GetMaxHealth();

        if (healthPercent <= lowHealthThreshold && !_isLowHealth)
        {
            StartPulse();
            _isLowHealth = true;
        }
        else if (healthPercent > lowHealthThreshold && _isLowHealth)
        {
            StopPulse();
            _isLowHealth = false;
        }
    }

    private void StartPulse()
    {
        // Initial heartbeat sound
        PlayHeartbeat(maxAlpha);
        
        bloodSplashImage.DOFade(maxAlpha, 0.5f).OnComplete(() =>
        {
            _pulseTween = bloodSplashImage.DOFade(minAlpha, pulseSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .OnStepComplete(() => PlayHeartbeat(bloodSplashImage.color.a));
        });
    }

    private void PlayHeartbeat(float currentAlpha)
    {
        if (heartbeatSFX == null) return;
        
        // Pitch increases with alpha intensity
        float pitch = Mathf.Lerp(heartbeatPitchMin, heartbeatPitchMax, currentAlpha);
        _audioSource.pitch = pitch;
        _audioSource.PlayOneShot(heartbeatSFX, sfxVolume);
    }

    private void StopPulse()
    {
        _pulseTween?.Kill();
        bloodSplashImage.DOFade(0, 1f).OnComplete(() =>
        {
            if (healthRecoverySFX != null)
            {
                _audioSource.pitch = 1f;
                _audioSource.PlayOneShot(healthRecoverySFX, sfxVolume);
            }
        });
    }
}