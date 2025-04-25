using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LowHealthEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30% health
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 0.7f;

    [Header("References")]
    [SerializeField] private Image bloodSplashImage;
    [SerializeField] private HealthSystem playerHealth;

    private bool _isLowHealth;
    private Tween _pulseTween;

    private void Start()
    {
        // Initialize as transparent
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
        // Fade in quickly first
        bloodSplashImage.DOFade(maxAlpha, 0.5f).OnComplete(() =>
        {
            // Start pulsing
            _pulseTween = bloodSplashImage.DOFade(minAlpha, pulseSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        });
    }

    private void StopPulse()
    {
        _pulseTween?.Kill();
        bloodSplashImage.DOFade(0, 1f); // Fade out smoothly
    }
}