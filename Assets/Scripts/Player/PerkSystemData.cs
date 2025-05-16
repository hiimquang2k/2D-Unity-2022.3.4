using UnityEngine;
using System.Collections.Generic;

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
    public int maxLevel => levels != null ? levels.Count : 0;
    public int currentLevel { get; private set; } = 0;

    public void AddPoint()
    {
        currentPoints++;
        UpdateLevel();
    }
    
    public void SetCurrentLevel(int level)
    {
        currentLevel = Mathf.Max(0, level);
    }

    private void UpdateLevel()
    {
        int newLevel = 0;
        if (levels != null)
        {
            foreach (var level in levels)
            {
                if (level != null && currentPoints >= level.pointsRequired)
                {
                    level.isActive = true;
                    newLevel++;
                }
                else if (level != null)
                {
                    level.isActive = false;
                }
            }
        }
        currentLevel = newLevel;
    }

    public float GetTotalBonus()
    {
        float total = 0;
        if (levels != null)
        {
            foreach (var level in levels)
            {
                if (level != null && level.isActive)
                    total += level.bonusValue;
            }
        }
        return total;
    }
}

[System.Serializable]
public class PerkSystemData
{
    public int currentXp = 0;
    public int currentLevel = 1;
    public int xpToNextLevel = 100;
    public int availablePoints = 0;
    public PerkCategory healthPerks = new PerkCategory();
    public PerkCategory damagePerks = new PerkCategory();
}
