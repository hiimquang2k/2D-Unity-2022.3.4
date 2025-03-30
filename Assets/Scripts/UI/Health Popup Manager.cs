using UnityEngine;
using System.Collections.Generic;

public class HealthPopupManager : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject healthPopupPrefab;
    [SerializeField] private Transform popupParent;
    [SerializeField] private float offsetY = 1f;
    [SerializeField] private float randomOffsetX = 0.5f;
    
    [Header("Advanced Settings")]
    [SerializeField] private bool combinePopups = true;
    [SerializeField] private float combineTimeWindow = 0.2f;
    [SerializeField] private int criticalThreshold = 15; // Percentage of max health for critical
    
    // Dictionary to track recent damage/healing for each entity
    private Dictionary<HealthSystem, PopupData> recentPopups = new Dictionary<HealthSystem, PopupData>();
    
    // Singleton instance
    private static HealthPopupManager instance;
    public static HealthPopupManager Instance { get { return instance; } }
    
    // Class to store popup data
    private class PopupData
    {
        public int damageValue;
        public float timestamp;
        public bool processed;
        
        public PopupData(int value)
        {
            damageValue = value;
            timestamp = Time.time;
            processed = false;
        }
    }

    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        
        // If no parent is assigned, use this transform
        if (popupParent == null)
        {
            popupParent = transform;
        }
    }
    
    private void Update()
    {
        // Process any combined popups that need to be displayed
        if (combinePopups)
        {
            List<HealthSystem> systemsToProcess = new List<HealthSystem>();
            List<HealthSystem> systemsToRemove = new List<HealthSystem>();
            
            foreach (var entry in recentPopups)
            {
                HealthSystem healthSystem = entry.Key;
                PopupData popupData = entry.Value;
                
                // Check if the health system still exists
                if (healthSystem == null)
                {
                    systemsToRemove.Add(healthSystem);
                    continue;
                }
                
                // Check if enough time has passed to display the combined popup
                if (!popupData.processed && Time.time - popupData.timestamp >= combineTimeWindow)
                {
                    systemsToProcess.Add(healthSystem);
                }
            }
            
            // Process all systems that need popups
            foreach (var healthSystem in systemsToProcess)
            {
                DisplayCombinedPopup(healthSystem);
            }
            
            // Remove any null health systems
            foreach (var healthSystem in systemsToRemove)
            {
                recentPopups.Remove(healthSystem);
            }
        }
    }
    
    private void DisplayCombinedPopup(HealthSystem healthSystem)
    {
        if (recentPopups.ContainsKey(healthSystem))
        {
            PopupData popupData = recentPopups[healthSystem];
            
            // Create the actual popup
            int valueToShow = popupData.damageValue;
            bool isCritical = IsCriticalAmount(healthSystem, Mathf.Abs(valueToShow));
            CreatePopupObject(healthSystem.transform.position, valueToShow, isCritical);
            
            // Mark as processed
            popupData.processed = true;
            
            // Start a timer to remove the entry
            StartCoroutine(RemovePopupData(healthSystem, 0.5f));
        }
    }
    
    private System.Collections.IEnumerator RemovePopupData(HealthSystem healthSystem, float delay)
    {
        yield return new WaitForSeconds(delay);
        recentPopups.Remove(healthSystem);
    }
    
    public void RegisterHealthChange(HealthSystem healthSystem, int previousHealth, int currentHealth)
    {
        int healthDifference = currentHealth - previousHealth;
        
        // Only show popup if health actually changed
        if (healthDifference != 0)
        {
            if (combinePopups)
            {
                // Store or update damage value for combining popups
                if (recentPopups.ContainsKey(healthSystem) && !recentPopups[healthSystem].processed)
                {
                    // Add to existing popup data
                    recentPopups[healthSystem].damageValue += healthDifference;
                    recentPopups[healthSystem].timestamp = Time.time;
                }
                else
                {
                    // Create new popup data
                    recentPopups[healthSystem] = new PopupData(healthDifference);
                }
            }
            else
            {
                // Immediately display popup without combining
                bool isCritical = IsCriticalAmount(healthSystem, Mathf.Abs(healthDifference));
                CreatePopupObject(healthSystem.transform.position, healthDifference, isCritical);
            }
        }
    }
    
    private bool IsCriticalAmount(HealthSystem healthSystem, int amount)
    {
        // Determine if damage is "critical" based on percentage of max health
        if (amount == 0) return false;
        
        int maxHealth = healthSystem.GetMaxHealth();
        float damagePercent = (float)amount / maxHealth * 100f;
        
        return damagePercent >= criticalThreshold;
    }
    
    private void CreatePopupObject(Vector3 position, int value, bool isCritical)
    {
        if (healthPopupPrefab == null)
        {
            Debug.LogError("Health popup prefab is not assigned!");
            return;
        }
        
        // Add offset to make popup appear above entity
        float randomX = Random.Range(-randomOffsetX, randomOffsetX);
        Vector3 popupPos = position + new Vector3(randomX, offsetY, 0);
        
        // Create popup
        GameObject popup = Instantiate(healthPopupPrefab, popupPos, Quaternion.identity, popupParent);
        HealthPopup healthPopup = popup.GetComponent<HealthPopup>();
        
        if (healthPopup != null)
        {
            // Set text value and critical status
            healthPopup.SetValue(value, isCritical);
        }
        else
        {
            Debug.LogError("HealthPopup component not found on prefab!");
        }
    }
}
