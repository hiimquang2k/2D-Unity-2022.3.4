using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Text healthText;
    [SerializeField] private HealthSystem healthSystem;

    [Header("Settings")]
    [SerializeField] private bool showText = true;
    [SerializeField] private bool smoothFill = true;
    [SerializeField] private float fillSpeed = 5f;

    [Header("Colors")]
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float mediumHealthThreshold = 0.7f;

    private float targetFillAmount;

    private void Start()
    {
        if (healthSystem == null)
        {
            healthSystem = GameObject.FindWithTag("Player").GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                Debug.LogError("No HealthSystem found for HealthBar!");
                return;
            }
        }

        // Subscribe to health changes
        healthSystem.OnHealthChanged += UpdateHealthBar;

        // Initial update
        UpdateHealthBar(healthSystem.CurrentHealth, healthSystem.GetMaxHealth());
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealthBar;
    }

    private void Update()
    {
        if (smoothFill && fillImage.fillAmount != targetFillAmount)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * fillSpeed);

            // Snap to target if very close to avoid lingering tiny differences
            if (Mathf.Abs(fillImage.fillAmount - targetFillAmount) < 0.01f)
                fillImage.fillAmount = targetFillAmount;
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        float healthPercent = (float)currentHealth / maxHealth;

        // Update fill amount
        fillImage.fillAmount = healthPercent; // Update the fill amount directly
        targetFillAmount = healthPercent;

        // Update color based on health percentage
        if (healthPercent <= lowHealthThreshold)
            fillImage.color = lowHealthColor;
        else if (healthPercent <= mediumHealthThreshold)
            fillImage.color = mediumHealthColor;
        else
            fillImage.color = highHealthColor;

        // Update text if needed
        if (showText && healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }
}