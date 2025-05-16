using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class PerkLevel
{
    public int pointsRequired;
    public float bonusValue;
    public bool isActive = false;
}

[System.Serializable]
public class PerkCategory
{
    public string categoryName;
    public List<PerkLevel> levels;
    public int currentPoints = 0;
    public int maxLevel => levels.Count;
    public int currentLevel { get; private set; } = 0;

    public void AddPoint()
    {
        currentPoints++;
        UpdateLevel();
    }

    private void UpdateLevel()
    {
        int newLevel = 0;
        foreach (var level in levels)
        {
            if (currentPoints >= level.pointsRequired)
            {
                level.isActive = true;
                newLevel++;
            }
            else
            {
                level.isActive = false;
            }
        }
        currentLevel = newLevel;
    }

    public float GetTotalBonus()
    {
        float total = 0;
        foreach (var level in levels)
        {
            if (level.isActive)
                total += level.bonusValue;
        }
        return total;
    }
}

public class PerkSystem : MonoBehaviour
{
    public static PerkSystem Instance { get; private set; }
    
    [Header("XP Settings")]
    public int currentXp = 0;
    public int currentLevel = 1;
    public int xpToNextLevel = 100;
    public float xpMultiplier = 1.5f; // How much more XP is needed for next level
    
    [Header("Perk Categories")]
    public PerkCategory healthPerks;
    public PerkCategory damagePerks;
    
    [Header("Level Up")]
    public int perkPointsPerLevel = 1;
    public int availablePoints = 0;

    // Event for when XP/level changes
    public event Action<int, int> OnXpChanged; // currentXp, xpToNextLevel
    public event Action<int> OnLevelUp; // newLevel
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this when a monster is defeated
    public void AddXp(int xpAmount)
    {
        if (xpAmount <= 0) return;
        
        currentXp += xpAmount;
        
        // Check for level up
        while (currentXp >= xpToNextLevel)
        {
            LevelUp();
        }
        
        // Notify listeners (UI, etc.)
        OnXpChanged?.Invoke(currentXp, xpToNextLevel);
    }

    private void LevelUp()
    {
        currentXp -= xpToNextLevel;
        currentLevel++;
        
        // Increase XP needed for next level
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplier);
        
        // Grant perk points
        availablePoints += perkPointsPerLevel;
        
        // Notify listeners
        OnLevelUp?.Invoke(currentLevel);
        OnXpChanged?.Invoke(currentXp, xpToNextLevel);
        
        // Play level up effect
        // AudioManager.Instance.Play("LevelUp");
        Debug.Log($"Level Up! Now level {currentLevel}. Gained {perkPointsPerLevel} perk points!");
    }

    public void AddPointToHealth()
    {
        if (availablePoints <= 0) return;
        
        healthPerks.AddPoint();
        availablePoints--;
        
        // Update any systems that depend on health perks
        UpdateHealthSystem();
    }

    public void AddPointToDamage()
    {
        if (availablePoints <= 0) return;
        
        damagePerks.AddPoint();
        availablePoints--;
        
        // Update any systems that depend on damage perks
        UpdateDamageSystem();
    }

    private void UpdateHealthSystem()
    {
        // Update player health system with new health bonus
        float healthBonus = healthPerks.GetTotalBonus();
        // Example: PlayerStats.Instance.healthBonus = healthBonus;
        Debug.Log($"Health Bonus Updated: +{healthBonus} health");
    }

    private void UpdateDamageSystem()
    {
        // Update player damage system with new damage bonus
        float damageBonus = damagePerks.GetTotalBonus();
        // Example: PlayerCombat.Instance.damageBonus = damageBonus;
        Debug.Log($"Damage Bonus Updated: +{damageBonus} damage");
    }

    // Call this when you want to add points directly (e.g., for testing)
    public void AddAvailablePoints(int points = 1)
    {
        availablePoints += points;
        Debug.Log($"Added {points} perk points. Total: {availablePoints}");
    }

    // For testing - call this from a button or key press
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddXp(50); // Add 50 XP for testing
        }
    }
}
