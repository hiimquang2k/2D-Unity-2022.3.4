using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlackHole : MonoBehaviour
{
    [Header("Pull Settings")]
    [SerializeField] private float pullForce = 10f;
    [SerializeField] private float pullRadius = 5f;
    
    [Header("Damage Settings")]
    [SerializeField] private float damageRadius = 1f;
    [SerializeField] private int damagePerTick = 10;
    [SerializeField] private float tickInterval = 0.5f;
    
    private HashSet<DamageSystem> affectedPlayers = new HashSet<DamageSystem>();
    private Coroutine damageCoroutine;

    private void Update()
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, pullRadius);
        foreach (Collider2D obj in nearbyObjects)
        {
            if (!obj.CompareTag("Player")) continue;
            
            // Handle physics pull
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (transform.position - obj.transform.position).normalized;
                rb.AddForce(direction * pullForce, ForceMode2D.Force);
            }

            // Handle damage system
            DamageSystem damageSystem = obj.GetComponent<DamageSystem>();
            if (damageSystem != null)
            {
                float distance = Vector2.Distance(transform.position, obj.transform.position);
                bool inDamageZone = distance < damageRadius;

                if (inDamageZone && !affectedPlayers.Contains(damageSystem))
                {
                    // Player entered damage zone
                    affectedPlayers.Add(damageSystem);
                    damageSystem.SetKnockbackable(false);
                }
                else if (!inDamageZone && affectedPlayers.Contains(damageSystem))
                {
                    // Player exited damage zone
                    affectedPlayers.Remove(damageSystem);
                    damageSystem.SetKnockbackable(true);
                }
            }
        }

        // Clean up any null references
        affectedPlayers.RemoveWhere(x => x == null);
    }

    private void OnEnable()
    {
        damageCoroutine = StartCoroutine(ApplyDoTDamage());
    }

    private void OnDisable()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }

        // Reset all affected players
        foreach (var player in affectedPlayers)
        {
            if (player != null)
            {
                player.SetKnockbackable(true);
            }
        }
        affectedPlayers.Clear();
    }

    private IEnumerator ApplyDoTDamage()
    {
        WaitForSeconds wait = new WaitForSeconds(tickInterval);
        
        while (true)
        {
            foreach (var player in affectedPlayers)
            {
                if (player != null)
                {
                    player.ApplyDamage(damagePerTick, DamageType.DoT);
                }
            }
            yield return wait;
        }
    }

    public float GetPullRadius() => pullRadius;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}