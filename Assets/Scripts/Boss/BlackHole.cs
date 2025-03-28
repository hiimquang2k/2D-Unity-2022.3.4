using UnityEngine;
using System.Collections;

public class BlackHole : MonoBehaviour
{
    [SerializeField] private float pullForce = 10f;
    [SerializeField] private float pullRadius = 5f;
    [SerializeField] private float damageRadius = 1f;
    [SerializeField] private int damagePerSecond = 10;
    [SerializeField] private float damageInterval = 0.5f;
    private bool isPlayerInDamageRadius = false;

    private void Update()
{
    Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, pullRadius); 
    foreach (Collider2D obj in nearbyObjects)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null && obj.CompareTag("Player"))
        {
            Vector2 directionToBlackHole = (transform.position - obj.transform.position).normalized;
            rb.AddForce(directionToBlackHole * pullForce, ForceMode2D.Force);

            // Check for collision and apply damage
            float distance = Vector2.Distance(transform.position, obj.transform.position);
            if (distance < damageRadius)
            {
                if (!isPlayerInDamageRadius)
                {
                    // Player just entered damage radius
                    isPlayerInDamageRadius = true;
                    HandlePlayerCollision(obj);
                }
            }
        }
    }
}

    private void HandlePlayerCollision(Collider2D playerCollider)
    {
        DamageSystem damageSystem = playerCollider.GetComponentInParent<DamageSystem>();
        if (damageSystem != null)
        {
            // Disable knockback while in black hole
            damageSystem.SetKnockbackable(false);
        
            // Start damage coroutine
            StartCoroutine(ApplyDoTDamage(damageSystem));
        }
    }

    private IEnumerator ApplyDoTDamage(DamageSystem damageSystem)
    {
        while (isPlayerInDamageRadius)
        {
            // Use DamageType.DoT for black hole damage
            damageSystem.TakeDamage(damagePerSecond, DamageType.DoT);
            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void ResetPlayerInvulnerability(Collider2D playerCollider)
    {
        DamageSystem damageSystem = playerCollider.GetComponentInParent<DamageSystem>();
        if (damageSystem != null)
        {
            // Reset knockback state
            damageSystem.SetKnockbackable(true);
        
            // Reset damage state
            damageSystem.SetDamageable(true);
            Debug.Log("Reset player invulnerability successfully");
        }

        if (damageSystem == null)
        {
            Debug.LogError("Invalid damage system");
        }
    }
    private void OnDestroy()
    {
        // Store the player collider before destruction
        Collider2D playerCollider = null;
        
        // Check for nearby player
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, pullRadius);
        foreach (Collider2D obj in nearbyObjects)
        {
            if (obj.CompareTag("Player"))
            {
                playerCollider = obj;
                break;
            }
        }

        // Reset player invulnerability if player was in damage radius
        if (isPlayerInDamageRadius && playerCollider != null)
        {
            ResetPlayerInvulnerability(playerCollider);
        }
    }

    // Visualization for editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageRadius); // Show damage radius
    }
}