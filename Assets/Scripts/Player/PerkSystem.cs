using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public class PerkSystem : MonoBehaviour
{
    public static PerkSystem Instance { get; private set; }
    
    [Header("References")]
    public PlayerData playerData;
    
    [Header("XP Settings")]
    public float xpMultiplier = 1.5f; // How much more XP is needed for next level
    
    [Header("Level Up")]
    public int perkPointsPerLevel = 1;
    
    // Serialized field for the perk data
    [SerializeField] private PerkSystemData perkData = new PerkSystemData();
    
    // Public properties to access the data
    public int CurrentXp => perkData.currentXp;
    public int CurrentLevel => perkData.currentLevel;
    public int XpToNextLevel => perkData.xpToNextLevel;
    public int AvailablePoints => perkData.availablePoints;
    public PerkCategory HealthPerks => perkData.healthPerks;
    public PerkCategory DamagePerks => perkData.damagePerks;
    public PerkSystemData PerkData => perkData;

    // Events for when XP/level changes
    [System.Serializable] public class XpChangedEvent : UnityEvent<int, int> { }
    [System.Serializable] public class LevelUpEvent : UnityEvent<int> { }
    
    public XpChangedEvent OnXpChanged = new XpChangedEvent(); // currentXp, xpToNextLevel
    public LevelUpEvent OnLevelUp = new LevelUpEvent(); // newLevel
    
    private void Awake()
    {
        // Set up singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize default perks if no data is loaded yet
            if (perkData == null || perkData.healthPerks == null || perkData.damagePerks == null)
            {
                if (perkData == null)
                    perkData = new PerkSystemData();
                InitializeDefaultPerks();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadPerkData(PerkSystemData dataToLoad)
    {
        if (dataToLoad == null)
        {
            // Initialize with default values if no data is provided
            perkData = new PerkSystemData();
            InitializeDefaultPerks();
        }
        else
        {
            // Deep copy the data to avoid reference issues
            perkData = new PerkSystemData
            {
                currentXp = dataToLoad.currentXp,
                currentLevel = dataToLoad.currentLevel,
                xpToNextLevel = dataToLoad.xpToNextLevel,
                availablePoints = dataToLoad.availablePoints,
                healthPerks = new PerkCategory
                {
                    categoryName = dataToLoad.healthPerks?.categoryName ?? "Health",
                    currentPoints = dataToLoad.healthPerks?.currentPoints ?? 0,
                    levels = dataToLoad.healthPerks?.levels != null ? 
                        new List<PerkLevel>(dataToLoad.healthPerks.levels) : 
                        new List<PerkLevel>()
                },
                damagePerks = new PerkCategory
                {
                    categoryName = dataToLoad.damagePerks?.categoryName ?? "Damage",
                    currentPoints = dataToLoad.damagePerks?.currentPoints ?? 0,
                    levels = dataToLoad.damagePerks?.levels != null ? 
                        new List<PerkLevel>(dataToLoad.damagePerks.levels) : 
                        new List<PerkLevel>()
                }
            };
        }

        
        // Ensure perk categories are properly initialized
        if (perkData.healthPerks.levels == null || perkData.healthPerks.levels.Count == 0)
        {
            InitializeDefaultHealthPerks();
        }
        
        if (perkData.damagePerks.levels == null || perkData.damagePerks.levels.Count == 0)
        {
            InitializeDefaultDamagePerks();
        }
        
        // Update any systems that depend on perks
        UpdateHealthSystem();
        UpdateDamageSystem();
        
        // Notify UI to update
        OnXpChanged?.Invoke(perkData.currentXp, perkData.xpToNextLevel);
        OnLevelUp?.Invoke(perkData.currentLevel);
    }
    
    private void InitializeDefaultPerks()
    {
        perkData.healthPerks = new PerkCategory { categoryName = "Health" };
        perkData.damagePerks = new PerkCategory { categoryName = "Damage" };
        
        InitializeDefaultHealthPerks();
        InitializeDefaultDamagePerks();
    }
    
    private void InitializeDefaultHealthPerks()
    {
        perkData.healthPerks.levels = new List<PerkLevel>
        {
            new PerkLevel { pointsRequired = 1, bonusValue = 20f },
            new PerkLevel { pointsRequired = 3, bonusValue = 30f },
            new PerkLevel { pointsRequired = 6, bonusValue = 50f }
        };
    }
    
    private void InitializeDefaultDamagePerks()
    {
        perkData.damagePerks.levels = new List<PerkLevel>
        {
            new PerkLevel { pointsRequired = 1, bonusValue = 5f },
            new PerkLevel { pointsRequired = 3, bonusValue = 10f },
            new PerkLevel { pointsRequired = 6, bonusValue = 15f }
        };
    }

    // Call this when a monster is defeated
    public void AddXp(int xpAmount)
    {
        if (xpAmount <= 0) return;
        
        perkData.currentXp += xpAmount;
        
        // Check for level up
        while (perkData.currentXp >= perkData.xpToNextLevel)
        {
            LevelUp();
        }
        
        // Notify listeners (UI, etc.)
        OnXpChanged?.Invoke(perkData.currentXp, perkData.xpToNextLevel);
    }

    private void LevelUp()
    {
        perkData.currentXp -= perkData.xpToNextLevel;
        perkData.currentLevel++;
        
        // Increase XP needed for next level
        perkData.xpToNextLevel = Mathf.RoundToInt(perkData.xpToNextLevel * xpMultiplier);
        
        // Grant perk points
        perkData.availablePoints += perkPointsPerLevel;
        
        // Update player stats
        if (playerData != null)
        {
            // Add any level-based stat increases here
            // Example: playerData.maxHealth += 10;
        }
        
        // Notify listeners
        OnLevelUp?.Invoke(perkData.currentLevel);
        OnXpChanged?.Invoke(perkData.currentXp, perkData.xpToNextLevel);
        
        // Play level up effect
        // AudioManager.Instance.Play("LevelUp");
        Debug.Log($"Level Up! Now level {perkData.currentLevel}. Gained {perkPointsPerLevel} perk points!");
    }

    public void AddPointToHealth()
    {
        if (perkData.availablePoints <= 0) return;
        
        perkData.healthPerks.AddPoint();
        perkData.availablePoints--;
        
        // Update any systems that depend on health perks
        UpdateHealthSystem();
    }

    public void AddPointToDamage()
    {
        if (perkData.availablePoints <= 0) return;
        
        perkData.damagePerks.AddPoint();
        perkData.availablePoints--;
        
        // Update any systems that depend on damage perks
        UpdateDamageSystem();
    }

    private void UpdateHealthSystem()
    {
        if (playerData == null) return;
        
        float healthBonus = perkData.healthPerks.GetTotalBonus();
        // Update PlayerData with health bonus
        int newMaxHealth = playerData.maxHealth + (int)healthBonus;
        playerData.maxHealth = newMaxHealth;
        Debug.Log($"Health Bonus Updated: +{healthBonus} health");
        
        // Update the HealthSystem component
        var healthSystem = FindObjectOfType<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.SetMaxHealth(newMaxHealth);
            // Also update current health to match the new max if needed
            if (healthSystem.CurrentHealth < healthSystem.GetMaxHealth())
            {
                healthSystem.SetCurrentHealth(healthSystem.CurrentHealth);
            }
        }
        else
        {
            Debug.LogWarning("HealthSystem component not found in the scene!");
        }
    }

    private void UpdateDamageSystem()
    {
        if (playerData == null) return;
        
        float damageBonus = perkData.damagePerks.GetTotalBonus();
        // Update PlayerData with damage bonus
        playerData.attackDamage1 = playerData.attackDamage1 + (int)damageBonus;
        playerData.attackDamage2 = playerData.attackDamage2 + (int)damageBonus;
        Debug.Log($"Damage Bonus Updated: +{damageBonus} damage");
    }

    // Call this when you want to add points directly (e.g., for testing)
    public void AddAvailablePoints(int points = 1)
    {
        perkData.availablePoints += points;
        Debug.Log($"Added {points} perk points. Total: {perkData.availablePoints}");
    }
    
    // Call this from NPC interaction to unlock the first damage perk
    public void UnlockFirstDamagePerk()
    {
        if (perkData.damagePerks.levels.Count > 0 && 
            (perkData.damagePerks.levels[0].pointsRequired <= 0 || !perkData.damagePerks.levels[0].isActive))
        {
            perkData.damagePerks.levels[0].isActive = true;
            perkData.damagePerks.currentPoints = Mathf.Max(perkData.damagePerks.currentPoints, 1);
            perkData.damagePerks.SetCurrentLevel(Mathf.Max(perkData.damagePerks.currentLevel, 1));
            UpdateDamageSystem();
            Debug.Log("First damage perk unlocked!");
            
            // Save the game after unlocking the perk
            // if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
        }
    }

    // For testing - call this from a button or key press
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) // Changed from 'P' to 'O' to avoid conflict with perk menu
        {
            AddXp(50); // Add 50 XP for testing
        }
    }
}
