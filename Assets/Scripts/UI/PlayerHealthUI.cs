using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health System Reference")]
    public HealthSystem healthSystem;

    [Header("UI Elements")]
    public Image healthBorder;          // New border reference
    public Image currentHealthBar;
    public Image delayedHealthBar;
    public Image bloodOverlay;

    [Header("Animation Settings")]
    public float healthLerpSpeed = 10f;
    public float delayLerpSpeed = 5f;
    public float lowHealthThreshold = 0.3f;
    public Color lowHealthColor = Color.red;

    [Header("Flash Settings")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    public float borderFlashIntensity = 0.7f;  // New border flash setting

    private float targetHealth;
    private Color originalHealthColor;
    private Color originalBorderColor;         // Store border color
    private bool isLowHealth;

    private void Start()
    {
        if (!InitializeHealthSystem()) return;

        // Initialize colors
        originalHealthColor = currentHealthBar.color;
        originalBorderColor = healthBorder.color;
        if (bloodOverlay) bloodOverlay.color = Color.clear;

        // Set initial values
        targetHealth = GetNormalizedHealth();
        UpdateHealthBarValues(targetHealth);
    }

    private bool InitializeHealthSystem()
    {
        if (healthSystem != null) return true;
        
        healthSystem = GameObject.FindWithTag("Player")?.GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            Debug.LogError("HealthSystem reference missing!");
            return false;
        }

        healthSystem.OnHealthChanged += OnHealthChanged;
        healthSystem.OnDeath += OnDeath;
        return true;
    }

    private void Update()
    {
        UpdateHealthBars();
        HandleLowHealthEffects();
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        targetHealth = (float)currentHealth / maxHealth;
        
        if (currentHealth < healthSystem.CurrentHealth)
        {
            StartCoroutine(FlashHealthEffects());
        }
    }

    private void UpdateHealthBars()
    {
        currentHealthBar.fillAmount = Mathf.Lerp(
            currentHealthBar.fillAmount,
            targetHealth,
            Time.deltaTime * healthLerpSpeed
        );

        delayedHealthBar.fillAmount = Mathf.Lerp(
            delayedHealthBar.fillAmount,
            currentHealthBar.fillAmount,
            Time.deltaTime * delayLerpSpeed
        );
    }

    private void HandleLowHealthEffects()
    {
        bool shouldBeLow = targetHealth <= lowHealthThreshold;

        if (shouldBeLow == isLowHealth) return;

        isLowHealth = shouldBeLow;
        if (isLowHealth)
        {
            StartCoroutine(PulseLowHealthEffects());
        }
        else
        {
            ResetHealthColors();
            if (bloodOverlay) bloodOverlay.color = Color.clear;
        }
    }

    private IEnumerator PulseLowHealthEffects()
    {
        currentHealthBar.color = lowHealthColor;
        
        while (isLowHealth)
        {
            // Pulse blood overlay
            float bloodAlpha = Mathf.Lerp(0.3f, 0.1f, Mathf.PingPong(Time.time, 1));
            if (bloodOverlay) bloodOverlay.color = new Color(1, 0, 0, bloodAlpha);

            // Pulse border color
            float borderPulse = Mathf.PingPong(Time.time, 1);
            healthBorder.color = Color.Lerp(
                originalBorderColor, 
                lowHealthColor, 
                borderPulse * 0.5f
            );

            yield return null;
        }
    }

    private IEnumerator FlashHealthEffects()
    {
        // Store original colors
        Color currentOriginal = currentHealthBar.color;
        Color borderOriginal = healthBorder.color;

        // Apply flash colors
        currentHealthBar.color = flashColor;
        healthBorder.color = new Color(1, 1, 1, borderFlashIntensity);

        yield return new WaitForSeconds(flashDuration);

        // Restore colors
        currentHealthBar.color = isLowHealth ? lowHealthColor : currentOriginal;
        healthBorder.color = isLowHealth ? 
            Color.Lerp(originalBorderColor, lowHealthColor, 0.5f) : 
            borderOriginal;
    }

    private void ResetHealthColors()
    {
        currentHealthBar.color = originalHealthColor;
        healthBorder.color = originalBorderColor;
    }

    private void UpdateHealthBarValues(float value)
    {
        currentHealthBar.fillAmount = value;
        delayedHealthBar.fillAmount = value;
    }

    private float GetNormalizedHealth()
    {
        return (float)healthSystem.CurrentHealth / healthSystem.GetMaxHealth();
    }

    private void OnDeath()
    {
        if (bloodOverlay) bloodOverlay.color = Color.clear;
        ResetHealthColors();
    }

    private void OnDestroy()
    {
        if (healthSystem == null) return;
        
        healthSystem.OnHealthChanged -= OnHealthChanged;
        healthSystem.OnDeath -= OnDeath;
    }
}