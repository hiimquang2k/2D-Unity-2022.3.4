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
    private int monsterDamage = 10;
    private DamageType damageType = DamageType.Physical;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 10f; // Force of the knockback
    [SerializeField] private float knockbackDuration = 0.5f; // Duration of the knockback effect
    
    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private PlayerMovement playerMovement; // Reference to the PlayerMovement component
    private bool isKnockedBack = false; // Flag to track knockback state
    private float knockbackTimer = 0f; // Timer for knockback duration
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
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

    public void TakeDamage(int damage, DamageType damageType)
    {
        if (healthSystem != null && !healthSystem.isInvulnerable)
        {
            // Apply damage reduction based on damage type
            int damageToApply = ApplyDamageReduction(damage, damageType);

            // Apply the damage
            healthSystem.TakeDamage(damageToApply, damageType);

            // Apply knockback if not immune
            if (!healthSystem.isInvulnerable)
            {
                ApplyKnockback();
            }

            // Trigger animations based on damage type
            TriggerDamageAnimation(damageType);
        }
    }

    private void ApplyKnockback()
    {
        if (rb != null && !isKnockedBack)
        {
            // Disable player movement control during knockback
            if (playerMovement != null)
                playerMovement.enabled = false;
                
            // Apply the knockback force
            rb.velocity = Vector2.zero; // Reset current velocity
            rb.AddForce(transform.right * knockbackForce, ForceMode2D.Impulse);
            
            // Set knockback state
            isKnockedBack = true;
            knockbackTimer = knockbackDuration;
        }
    }

    private int ApplyDamageReduction(int damage, DamageType damageType)
    {
        // Add damage reduction logic here based on damage type
        // For example:
        switch (damageType)
        {
            case DamageType.Physical:
                // Apply physical damage reduction
                return (int)(damage * 0.9f);
            case DamageType.Magical:
                // Apply magical damage reduction
                return (int)(damage * 0.8f);
            case DamageType.DoT:
                // Apply DoT damage reduction
                return (int)(damage * 0.7f);
            default:
                return damage;
        }
    }

    private void TriggerDamageAnimation(DamageType damageType)
    {
        if (animator != null)
        {
            // Trigger different animations based on damage type
            switch (damageType)
            {
                case DamageType.Physical:
                    animator.SetTrigger("PhysicalDamage");
                    break;
                case DamageType.Magical:
                    animator.SetTrigger("MagicalDamage");
                    break;
                case DamageType.Fire:
                    animator.SetTrigger("FireDamage");
                    break;
                case DamageType.Ice:
                    animator.SetTrigger("IceDamage");
                    break;
                case DamageType.Lightning:
                    animator.SetTrigger("LightningDamage");
                    break;
                case DamageType.DoT:
                    animator.SetTrigger("DoTDamage");
                    break;
                default:
                    animator.SetTrigger("Damage");
                    break;
            }
        }
    }

    // Method to handle collision with monsters
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster") && !healthSystem.isInvulnerable)
        {
            TakeDamage(monsterDamage, damageType);
        }
    }
}