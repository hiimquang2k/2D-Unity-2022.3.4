using UnityEngine;

// This class extends the functionality of HealthSystem to include popups
// Attach this to the same GameObject that has HealthSystem
[RequireComponent(typeof(HealthSystem))]
public class HealthSystemExtension : MonoBehaviour
{
    private HealthSystem healthSystem;
    private int lastKnownHealth;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Start()
    {
        if (healthSystem != null)
        {
            // Store initial health
            lastKnownHealth = healthSystem.CurrentHealth;
            
            // Subscribe to health change events
            healthSystem.OnHealthChanged += HandleHealthChanged;
        }
        else
        {
            Debug.LogError("HealthSystem not found on " + gameObject.name);
            enabled = false;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (HealthPopupManager.Instance != null)
        {
            // Register the health change with the manager
            HealthPopupManager.Instance.RegisterHealthChange(healthSystem, lastKnownHealth, currentHealth);
        }
        
        // Update last known health
        lastKnownHealth = currentHealth;
    }
}
