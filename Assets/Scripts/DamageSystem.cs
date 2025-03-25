using UnityEngine;

public enum DamageType
{
    Normal,
    Physical,
    Magical,
    Fire,
    Ice,
    Lightning
}

public class DamageSystem : MonoBehaviour
{
    public HealthSystem healthSystem; // Reference to the HealthSystem
    private Animator animator; // Animator for triggering animations
    private int monsterDamage = 10;
    private DamageType damageType = DamageType.Physical;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 10f; // Force of the knockback
    [SerializeField] private float knockbackDuration = 0.25f; // Duration of the knockback effect
    
    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private PlayerMovement playerMovement; // Reference to the PlayerMovement component
    private bool isKnockedBack = false; // Flag to track knockback state
    private float knockbackTimer = 0f; // Timer for knockback duration
    
    private void Start()
    {
        animator = GetComponent<Animator>(); // Initialize the Animator
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        playerMovement = GetComponent<PlayerMovement>(); // Get the PlayerMovement component
    }
    
    private void Update()
    {
        // Handle knockback timer
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
                // Re-enable player movement control after knockback
                if (playerMovement != null)
                    playerMovement.enabled = true;
            }
        }
    }

    // Method to apply knockback effect
    public void ApplyKnockback(Vector2 direction)
    {
        if (rb != null)
        {
            // Disable player movement control during knockback
            if (playerMovement != null)
                playerMovement.enabled = false;
                
            // Apply the knockback force
            rb.velocity = Vector2.zero; // Reset current velocity
            rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);
            
            // Set knockback state
            isKnockedBack = true;
            knockbackTimer = knockbackDuration;
        }
    }

    // Method to handle collision with monsters
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            // Apply damage
            healthSystem.TakeDamage(monsterDamage, damageType); 
            
            // Calculate knockback direction (away from the monster)
            Vector2 knockbackDirection = transform.position - collision.transform.position;
            
            // Apply knockback
            ApplyKnockback(knockbackDirection);
            
            // Trigger animations
            if (animator != null)
                animator.SetTrigger("hurt"); // Trigger hurt animation on player
                
            Animator monsterAnimator = collision.GetComponent<Animator>();
            if (monsterAnimator != null)
            {
                monsterAnimator.SetTrigger("hit"); // Trigger the hit animation on monster
            }
        }
    }
}