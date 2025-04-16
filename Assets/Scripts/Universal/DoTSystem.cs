using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DoTEffect
{
    public DamageType type;
    public float damagePerTick;
    public float tickInterval;
    public float duration;
    [HideInInspector] public float timeRemaining;
    [HideInInspector] public float nextTickTime;
}

public class DoTSystem : MonoBehaviour
{
    private List<DoTEffect> activeEffects = new List<DoTEffect>();
    private HealthSystem healthSystem;
    private BlackHoleBoss boss;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        boss = GetComponent<BlackHoleBoss>();
    }

    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            DoTEffect effect = activeEffects[i];
            effect.timeRemaining -= Time.deltaTime;

            if (Time.time >= effect.nextTickTime)
            {
                ApplyDoTDamage(effect);
                effect.nextTickTime = Time.time + effect.tickInterval;
            }

            if (effect.timeRemaining <= 0)
            {
                activeEffects.RemoveAt(i);
            }
        }
    }

    private void ApplyDoTDamage(DoTEffect effect)
    {
        int damage = Mathf.RoundToInt(effect.damagePerTick);
        
        if (boss != null)
        {
            // Boss handles its own damage reduction
            boss.ProcessIncomingDamage(damage, effect.type);
        }
        else
        {
            // Normal entities take full damage
            healthSystem.TakeDamage(damage, effect.type);
        }
    }

    public void ApplyDoT(DoTEffect newEffect)
    {
        // Check for existing effect of same type
        foreach (var effect in activeEffects)
        {
            if (effect.type == newEffect.type)
            {
                // Refresh duration if stronger effect
                if (newEffect.damagePerTick > effect.damagePerTick)
                {
                    effect.damagePerTick = newEffect.damagePerTick;
                    effect.timeRemaining = newEffect.duration;
                    effect.nextTickTime = Time.time + newEffect.tickInterval;
                }
                return;
            }
        }
        
        // Add new effect
        newEffect.timeRemaining = newEffect.duration;
        newEffect.nextTickTime = Time.time + newEffect.tickInterval;
        activeEffects.Add(newEffect);
    }
}
