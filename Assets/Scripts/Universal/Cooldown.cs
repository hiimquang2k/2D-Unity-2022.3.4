using UnityEngine;
using System.Collections.Generic;

public class CooldownSystem : MonoBehaviour
{
    private Dictionary<string, float> cooldowns = new Dictionary<string, float>();
    private Dictionary<string, bool> isOnCooldown = new Dictionary<string, bool>();

    public bool IsOnCooldown(string abilityName)
    {
        if (isOnCooldown.TryGetValue(abilityName, out bool isCooldown))
        {
            return isCooldown;
        }
        return false;
    }

    public void StartCooldown(float duration, string abilityName)
    {
        cooldowns[abilityName] = duration;
        isOnCooldown[abilityName] = true;
    }

    public void UpdateCooldown()
    {
        foreach (var abilityName in new List<string>(isOnCooldown.Keys))
        {
            if (isOnCooldown[abilityName])
            {
                cooldowns[abilityName] -= Time.deltaTime;
                if (cooldowns[abilityName] <= 0)
                {
                    isOnCooldown[abilityName] = false;
                    cooldowns[abilityName] = 0f; // Reset cooldown to 0 when done
                }
            }
        }
    }
}