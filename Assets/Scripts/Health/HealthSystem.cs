using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

// Create a scriptable object to store health data
[CreateAssetMenu(fileName = "NewHealthData", menuName = "Game/Health Data")]
public class HealthData : ScriptableObject
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public float invulnerabilityDuration = 1.0f;
    
    [Header("Visual Feedback")]
    public float flashDuration = 0.1f;
    public Color flashColor = Color.red;
    public int numberOfFlashes = 3;
}

public class HealthSystem : MonoBehaviour
{
    [Header("Health Configuration")]
    [SerializeField] private HealthData healthData;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isInvulnerable = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    public delegate void HealthChangedEvent(int currentHealth, int maxHealth);
    public event HealthChangedEvent OnHealthChanged;

    public delegate void DeathEvent();
    public event DeathEvent OnDeath;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // If no health data is assigned, create a default instance
        if (healthData == null)
        {
            healthData = ScriptableObject.CreateInstance<HealthData>();
            Debug.LogWarning("No HealthData assigned to " + gameObject.name + ". Using default values.");
        }
    }

    private void Start()
    {
        CurrentHealth = healthData.maxHealth;
        UpdateUI();
    }
    public void TakeDamage(int damage, DamageType damageType)
    {
        if (damage <= 0) return; // Ensure damage is positive
        if (isInvulnerable || CurrentHealth <= 0) return;

        // Apply damage reduction based on damage type if needed
        // This is where you could implement resistances or vulnerabilities
        int damageToApply = damage;

        // Apply the damage
        CurrentHealth = Mathf.Max(0, CurrentHealth - damageToApply);

        // Visual feedback
        if (spriteRenderer != null && !isFlashing)
        {
            StartCoroutine(FlashRoutine());
        }

        // Apply invulnerability
        StartCoroutine(InvulnerabilityRoutine());

        // Check if entity has died
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (CurrentHealth <= 0) return; // Can't heal if dead

        CurrentHealth = Mathf.Min(healthData.maxHealth, CurrentHealth + amount);
    }

    private void Die()
    {
        // Trigger death event
        OnDeath?.Invoke();
        
        Debug.Log(gameObject.name + " has died.");
        
        // Disable player controls if this is the player
        if (gameObject.CompareTag("Player"))
        {
            // Implement logic to disable player controls
            // e.g., PlayerController.Instance.DisableControls();
        }
        
        // Play death animation if applicable
        // e.g., animator.SetTrigger("Die");
        
        // Destroy the game object if it's a monster
        if (gameObject.CompareTag("Monster"))
        {
            Destroy(gameObject);
        }
    }

    private void UpdateUI()
    {
        // Remove UI update logic
    }

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;
        
        for (int i = 0; i < healthData.numberOfFlashes; i++)
        {
            spriteRenderer.color = healthData.flashColor;
            yield return new WaitForSeconds(healthData.flashDuration);
            spriteRenderer.color = originalColor;
            
            if (i < healthData.numberOfFlashes - 1)
            {
                yield return new WaitForSeconds(healthData.flashDuration);
            }
        }
        
        isFlashing = false;
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(healthData.invulnerabilityDuration);
        isInvulnerable = false;
    }

    // Public getters/setters
    public int GetMaxHealth() => healthData.maxHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0, healthData.maxHealth);
            OnHealthChanged?.Invoke(currentHealth, healthData.maxHealth);
        }
    }
    public float GetHealthPercentage() => (float)CurrentHealth / healthData.maxHealth;
    public bool IsInvulnerable() => isInvulnerable;
    
    // Method to manually set max health if needed
    public void SetMaxHealth(int newMaxHealth)
    {
        // Create a runtime instance of the health data if we want to modify it at runtime
        // This prevents modifying the original scriptable object asset
        if (healthData != null)
        {
            HealthData runtimeData = Instantiate(healthData);
            runtimeData.maxHealth = newMaxHealth;
            healthData = runtimeData;
            
            // Adjust current health if needed
            if (CurrentHealth > newMaxHealth)
            {
                CurrentHealth = newMaxHealth;
            }
            
            UpdateUI();
            OnHealthChanged?.Invoke(CurrentHealth, healthData.maxHealth);
        }
    }

    public void SetCurrentHealth(int newHealth)
    {
        Debug.Log("Setting current health to: " + newHealth);
        CurrentHealth = newHealth;
    }
}