using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Cinemachine;

// Create a scriptable object to store health data
[CreateAssetMenu(fileName = "NewHealthData", menuName = "Game/Health Data")]
public class HealthData : ScriptableObject
{
    [Header("Health Settings")]
    public float invulnerabilityDuration = 0.1f;
    
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
    [SerializeField] private int maxHealth;
    public bool isInvulnerable = false;

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
        CurrentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int damage, DamageType damageType)
    {
        Debug.Log("HealthSystem: Taking damage: " + damage + " to " + gameObject.name);
        
        if (damage <= 0) return; // Ensure damage is positive
        if (isInvulnerable || CurrentHealth <= 0) return;

        int previousHealth = CurrentHealth;
        
        // Apply the damage
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

        // Only trigger shake if damage was actually taken
        if (CurrentHealth < previousHealth)
        {
            Debug.Log("HealthSystem: Damage taken, triggering camera shake");
            
            // Find the Cinemachine Virtual Camera and get the camera shake component
            CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCamera != null)
            {
                ImprovedCameraShake cameraShake = virtualCamera.GetComponent<ImprovedCameraShake>();
                if (cameraShake != null)
                {
                    Debug.Log("HealthSystem: Found camera shake component");
                    cameraShake.TriggerCameraShake(gameObject);
                }
                else
                {
                    Debug.LogError("HealthSystem: Camera shake component not found on Cinemachine Virtual Camera!");
                }
            }
            else
            {
                Debug.LogError("HealthSystem: Cinemachine Virtual Camera not found!");
            }
        }

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

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    public void Die()
    {
        // Trigger death event
        OnDeath?.Invoke();
        
        Debug.Log(gameObject.name + " has died.");
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
    public int GetMaxHealth() => maxHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
    public float GetHealthPercentage() => (float)CurrentHealth / maxHealth;
    public bool IsInvulnerable() => isInvulnerable;
    
    // Method to manually set max health if needed
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void SetCurrentHealth(int newHealth)
    {
        Debug.Log("Setting current health to: " + newHealth);
        CurrentHealth = newHealth;
    }
    public void Initialize()
    {
        CurrentHealth = maxHealth;
        UpdateUI();
    }
}