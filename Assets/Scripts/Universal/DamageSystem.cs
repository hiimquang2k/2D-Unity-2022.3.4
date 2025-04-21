using UnityEngine;

public enum DamageType
{
    Physical,
    Magical,
    Fire,       // Burning DoT
    Ice,        // Slow/Freeze
    Lightning,  // Stun/Chain
    Water,      // Wet status (for synergies)
    Earth,      // Petrify/Shatter
    Pure,
    Environmental,
    DoT
}

public class DamageSystem : MonoBehaviour
{
    public HealthSystem healthSystem; // Reference to the HealthSystem
    private Animator animator; // Animator for triggering animations
    private ElementStatus elementStatus;
    [Header("Invulnerability Settings")]
    [SerializeField] private bool canTakeDamage = true;
    [SerializeField] private bool canBeKnockedBack = true;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 10f; // Force of the knockback
    [SerializeField] private float knockbackDuration = 0.5f; // Duration of the knockback effect

    [Header("Stun Settings")]
    [SerializeField] private float playerStunDuration = 1.5f;
    private bool isPlayerStunned;

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

        elementStatus = GetComponent<ElementStatus>();
    if (elementStatus == null) elementStatus = gameObject.AddComponent<ElementStatus>();
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
        if (isPlayerStunned)
        {
            playerStunDuration -= Time.deltaTime;
            if (playerStunDuration <= 0)
                ResetPlayerStun();
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
    private void ResetPlayerStun()
    {
        isPlayerStunned = false;
        playerMovement?.LockMovement(false);
        animator?.SetBool("IsStunned", false);
    }
    public void ApplyStun(float duration)
    {
        if (isPlayerStunned) return;

        isPlayerStunned = true;
        playerStunDuration = duration;

        // Block input (modify your PlayerMovement.cs)
        if (playerMovement != null)
            playerMovement.LockMovement(true);

        // Visual feedback
        animator?.SetBool("IsStunned", true);
        // Add camera shake, VFX, etc.
    }

    public void ApplyDoT(DoTEffect dotEffect)
    {
        DoTSystem dotSystem = GetComponent<DoTSystem>();
        if (dotSystem == null) dotSystem = gameObject.AddComponent<DoTSystem>();
        dotSystem.ApplyDoT(dotEffect);
    }

    // Modified ApplyDamage to handle DoT initialization
    public void ApplyDamage(int damage, DamageType type, Vector2 sourcePosition = default)
{
    if (!canTakeDamage) return;

    // Convert DamageType to Element for synergies
    Element? element = type switch
    {
        DamageType.Fire => Element.Fire,
        DamageType.Lightning => Element.Lightning,
        DamageType.Water => Element.Water,
        DamageType.Earth => Element.Earth,
        _ => null
    };

    if (element.HasValue)
    {
        ApplyElementalEffect(element.Value, 3f); // 3-second duration
    }

    // Existing damage logic
    if (type != DamageType.DoT)
    {
        healthSystem?.TakeDamage(damage, type);
        if (canBeKnockedBack) HandleKnockback();
    }
}
    public void ApplyElementalEffect(Element element, float duration)
    {
        if (elementStatus != null)
        {
            elementStatus.ApplyElement(element, duration);
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