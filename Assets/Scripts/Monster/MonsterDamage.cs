using UnityEngine;

public class MonsterDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private DamageType damageType = DamageType.Physical;
    [SerializeField] private float damageInterval = 1f;

    private float lastDamageTime;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object is the player
        if (collision.CompareTag("Player"))
        {
            Debug.Log("In collision with Player");
            DealDamageToPlayer(collision);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Continuous damage while player remains in contact
        if (collision.CompareTag("Player"))
        {
            if (Time.time - lastDamageTime >= damageInterval)
            {
                DealDamageToPlayer(collision);
            }
        }
    }

    private void DealDamageToPlayer(Collider2D playerCollider)
    {
        // Find the DamageSystem component on the player
        DamageSystem playerDamageSystem = playerCollider.GetComponent<DamageSystem>();
        
        if (playerDamageSystem != null)
        {
            // Deal damage and use current monster's position as damage source
            playerDamageSystem.TakeDamage(damageAmount, damageType, transform.position);
            
            // Update last damage time
            lastDamageTime = Time.time;

            // Optional: Debug log for damage dealing
            Debug.Log($"{gameObject.name} dealt {damageAmount} {damageType} damage to player");
        }
    }
}