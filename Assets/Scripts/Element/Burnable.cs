// Burnable.cs
using UnityEngine;

[RequireComponent(typeof(DamageSystem))]
public class Burnable : MonoBehaviour 
{
    [Header("Settings")]
    public bool isBurning;
    public float burnDuration = 3f;
    public int damagePerSecond = 5;
    public ParticleSystem fireVFX;

    private DamageSystem damageSystem;

    void Start() {
        damageSystem = GetComponent<DamageSystem>();
        if (fireVFX != null) fireVFX.Stop();
    }

    public void Ignite() {
        isBurning = true;
        if (fireVFX != null) fireVFX.Play();
        InvokeRepeating(nameof(ApplyBurnDamage), 0f, 1f);
        Invoke(nameof(Extinguish), burnDuration);
    }

    void ApplyBurnDamage() {
        damageSystem.ApplyDamage(damagePerSecond, DamageType.Fire);
    }

    public void Extinguish() {
        isBurning = false;
        CancelInvoke(nameof(ApplyBurnDamage));
        if (fireVFX != null) fireVFX.Stop();
    }
}