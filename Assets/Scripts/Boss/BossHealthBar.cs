using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private HealthSystem bossHealthSystem;

    private void Start()
    {
        if (bossHealthSystem != null)
        {
            healthSlider.maxValue = bossHealthSystem.GetMaxHealth();
            healthSlider.value = bossHealthSystem.currentHealth;
            
            bossHealthSystem.OnHealthChanged += UpdateHealthBar;
            bossHealthSystem.OnDeath += HandleBossDeath;
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        healthSlider.value = currentHealth;
    }

    private void HandleBossDeath()
    {
        healthSlider.gameObject.SetActive(false);
        // Optional: Add death effects
    }

    private void OnDestroy()
    {
        if (bossHealthSystem != null)
        {
            bossHealthSystem.OnHealthChanged -= UpdateHealthBar;
            bossHealthSystem.OnDeath -= HandleBossDeath;
        }
    }
}