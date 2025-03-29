using UnityEngine;

public enum DamageType
{
    Physical,
    Magical,
    Fire,
    Ice,
    Lightning,
    DoT,
    Normal
}

public class DamageSystem : MonoBehaviour
{
    public HealthSystem healthSystem; // Reference to the HealthSystem
    private Animator animator; // Animator for triggering animations

    [Header("Invulnerability Settings")]
    [SerializeField] private bool canTakeDamage = true;
    [SerializeField] private bool canBeKnockedBack = true;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 10f; // Force of the knockback
    [SerializeField] private float knockbackDuration = 0.5f; // Duration of the knockback effect
    
    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private PlayerMovement playerMovement; // Reference to the PlayerMovement component
    private bool isKnockedBack = false; // Flag to track knockback state
    private float knockbackTimer = 0f; // Timer for knockback duration
    private Vector2 lastDamageSource; // Variable to store the last damage source

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Update()
    {
        // Handle knockback timer
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0)
            {
                ResetKnockback();
            }
        }
    }

    private void ResetKnockback()
    {
        isKnockedBack = false;
        rb.velocity = Vector2.zero; // Stop any remaining velocity
        
        // Re-enable player movement control after knockback
        if (playerMovement != null)
            playerMovement.enabled = true;

        Debug.Log("Knockback reset completed");
    }

    public void TakeDamage(int amount, DamageType type = DamageType.Normal, Vector2 damageSource = default)
    {
        // Check if we can take damage
        if (!canTakeDamage)
        {
            return;
        }

        // Store the last damage source
        lastDamageSource = damageSource;

        // Apply damage
        healthSystem.TakeDamage(amount, type);

        // Handle knockback if allowed
        if (canBeKnockedBack)
        {
            HandleKnockback();
        }
    }

    private void HandleKnockback()
    {
        if (rb == null || isKnockedBack || !canBeKnockedBack)
        {
            return;
        }

        // Disable player movement control during knockback
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Reset velocity and apply knockback force
        rb.velocity = Vector2.zero;
        rb.AddForce(GetKnockbackDirection() * knockbackForce, ForceMode2D.Impulse);

        // Start knockback timer
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
    }

    private Vector2 GetKnockbackDirection()
    {
        // Get direction from damage source to player
        if (lastDamageSource != Vector2.zero)
        {
            Vector2 direction = new Vector2(transform.position.x, transform.position.y) - lastDamageSource;
            if (direction.sqrMagnitude > 0.001f) // Check if direction is non-zero
            {
                return direction.normalized;
            }
        }
        
        // Default to upward knockback if no valid damage source
        return Vector2.up;
    }

    public void SetDamageable(bool canTakeDamage)
    {
        this.canTakeDamage = canTakeDamage;
    }

    public void SetKnockbackable(bool canBeKnockedBack)
    {
        this.canBeKnockedBack = canBeKnockedBack;

        // If we're disabling knockback and currently knocked back, reset immediately
        if (!canBeKnockedBack && isKnockedBack)
        {
            ResetKnockback();
        }
    }
}