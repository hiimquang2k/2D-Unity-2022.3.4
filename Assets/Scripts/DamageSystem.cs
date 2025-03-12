using UnityEngine;

public enum DamageType
{
    Physical,
    Magical,
    Fire,
    Ice
}

public class DamageSystem : MonoBehaviour
{
    public HealthSystem healthSystem;

    // Method to apply damage
    public void ApplyDamage(int damage, DamageType damageType)
    {
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage, damageType);
        }
        else
        {
            Debug.LogWarning("HealthSystem reference is missing!");
        }
    }
}
