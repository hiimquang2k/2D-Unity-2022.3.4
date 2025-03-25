using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ElementType 
{
    Fire,
    Ice,
    Lightning,
    Poison
}

public class ElementalZoneController : MonoBehaviour
{
    [Header("Zone Transformation Settings")]
    public ElementType elementType;
    public float zoneDuration = 5f;
    public float damageTickInterval = 0.5f;
    public float damageAmount = 10f;

    [Header("Visual Effects")]
    public Color fireColor = Color.red;
    public Color iceColor = Color.cyan;
    public Color lightningColor = Color.yellow;
    public Color poisonColor = Color.green;

    private SpriteRenderer zoneRenderer;
    private bool isTransformed = false;
    private List<Collider2D> activeEnemiesInZone = new List<Collider2D>();

    private void Start()
    {
        zoneRenderer = GetComponent<SpriteRenderer>();
        if (zoneRenderer == null)
        {
            zoneRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player with specific element enters
        PlayerElementController playerElement = other.GetComponent<PlayerElementController>();
        if (playerElement != null && playerElement.currentElement == this.elementType)
        {
            TransformZone();
        }

        // Track enemies in the zone
        HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
        if (enemyHealth != null && isTransformed)
        {
            activeEnemiesInZone.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Remove enemies leaving the zone
        if (activeEnemiesInZone.Contains(other))
        {
            activeEnemiesInZone.Remove(other);
        }
    }

    private void TransformZone()
    {
        if (isTransformed) return;

        isTransformed = true;
        StartCoroutine(ZoneTransformationRoutine());
    }

    private IEnumerator ZoneTransformationRoutine()
    {
        // Apply zone color based on element type
        Color zoneColor = GetElementColor();
        zoneRenderer.color = zoneColor;

        // Start damage ticking
        StartCoroutine(ApplyElementalDamage());

        // Wait for duration
        yield return new WaitForSeconds(zoneDuration);

        // Reset zone
        ResetZone();
    }

    private IEnumerator ApplyElementalDamage()
    {
        while (isTransformed)
        {
            // Damage all enemies in the zone
            foreach (Collider2D enemy in activeEnemiesInZone)
            {
                HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                {
                    ApplyElementalDamageEffect(enemyHealth);
                }
            }

            yield return new WaitForSeconds(damageTickInterval);
        }
    }

    private void ApplyElementalDamageEffect(HealthSystem enemyHealth)
    {
        DamageType damageType = GetDamageTypeForElement(elementType);
        enemyHealth.TakeDamage(Mathf.RoundToInt(damageAmount), damageType);
    }

    private DamageType GetDamageTypeForElement(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire:
                return DamageType.Fire;
            case ElementType.Ice:
                return DamageType.Ice;
            case ElementType.Lightning:
                return DamageType.Lightning;
            default:
                return DamageType.Normal;
        }
    }

    private Color GetElementColor()
    {
        return elementType switch
        {
            ElementType.Fire => fireColor,
            ElementType.Ice => iceColor,
            ElementType.Lightning => lightningColor,
            ElementType.Poison => poisonColor,
            _ => Color.white
        };
    }

    private void ResetZone()
    {
        isTransformed = false;
        zoneRenderer.color = Color.white;
        activeEnemiesInZone.Clear();
    }
}

// Companion Player Element Controller
public class PlayerElementController : MonoBehaviour
{
    public ElementType currentElement = ElementType.Fire;

    public void ChangeElement(ElementType newElement)
    {
        currentElement = newElement;
    }
}

// Additional Enemy Movement Script for Slow Effect
public class EnemyMovement : MonoBehaviour
{
    public float normalSpeed = 5f;
    private float currentSpeed;
    private Coroutine slowCoroutine;

    private void Start()
    {
        currentSpeed = normalSpeed;
    }

    public void SlowDown()
    {
        // Stop any existing slow coroutine
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        slowCoroutine = StartCoroutine(SlowDownRoutine());
    }

    private IEnumerator SlowDownRoutine()
    {
        currentSpeed *= 0.5f;
        yield return new WaitForSeconds(2f);
        currentSpeed = normalSpeed;
    }
}