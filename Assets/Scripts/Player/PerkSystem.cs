using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Perk
{
    public string perkName;
    public string description;
    public int level;
    public int maxLevel;
    public float baseXpRequirement;
    public float healthBonus;
    public float damageBonus;

    // Calculate XP requirement for the next level using exponential growth
    public float GetXpRequirement()
    {
        return baseXpRequirement * Mathf.Pow(2, level);
    }

    // Check if the perk can be leveled up
    public bool CanLevelUp(float currentXp)
    {
        return level < maxLevel && currentXp >= GetXpRequirement();
    }

    // Level up the perk
    public void LevelUp()
    {
        if (level < maxLevel)
        {
            level++;
            // Increase health and damage bonuses
            healthBonus += 10; // Example increment
            damageBonus += 5;  // Example increment
        }
    }
}

public class PerkSystem : MonoBehaviour
{
    public static PerkSystem Instance { get; private set; }
    public List<Perk> perks = new List<Perk>();
    public float currentXp;
    public PlayerData playerData; // Reference to PlayerData

    void Awake()
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

    // Add XP to the system
    public void AddXp(float xp)
    {
        currentXp += xp;
        CheckForLevelUps();
        UpdatePlayerData(); // Update player data when XP changes
    }

    // Check and apply level-ups for all perks
    private void CheckForLevelUps()
    {
        foreach (var perk in perks)
        {
            if (perk.CanLevelUp(currentXp))
            {
                perk.LevelUp();
                Debug.Log($"{perk.perkName} leveled up to {perk.level}!");
            }
        }
    }

    // Add a new perk
    public void AddPerk(Perk perk)
    {
        perks.Add(perk);
    }

    public void UpdatePlayerData()
    {
        // Apply health bonus from perks
        playerData.maxHealth = 100 + perks.Sum(perk => (int)perk.healthBonus);

        // Modify jump height based on perks
        playerData.jumpHeight = 4f + perks.Sum(perk => perk.level * 0.1f); // Example modification

        // Apply other perk effects as needed
    }
}
