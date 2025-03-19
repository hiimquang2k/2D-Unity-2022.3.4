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
    public HealthSystem healthSystem; // Reference to the HealthSystem
    private int comboCount = 0; // Track the number of combo hits
    private Animator animator; // Animator for triggering animations
    private float comboTimer = 0f; // Timer for combo attacks
    private float comboDelay = 1f; // Time window for combo input
    private int damage = 10; // Specify the damage amount
    private DamageType damageType = DamageType.Physical; // Default damage type

    private void Start()
    {
        animator = GetComponent<Animator>(); // Initialize the Animator
    }

    // Method to apply damage to the target
    public void ApplyDamage(int damage, DamageType damageType)
    {
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage, damageType); // Apply damage to the health system
        }
        else
        {
            Debug.LogWarning("HealthSystem reference is missing!");
        }
    }

    // Method to handle collision with monsters
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            HealthSystem monsterHealth = collision.GetComponent<HealthSystem>();
            if (monsterHealth != null)
            {
                // Apply damage to the monster
                monsterHealth.TakeDamage(damage, damageType); // Apply damage with a default damage type
                // Trigger monster hit animation
                Animator monsterAnimator = collision.GetComponent<Animator>();
                if (monsterAnimator != null)
                {
                    monsterAnimator.SetTrigger("hit"); // Trigger the hit animation
                }
            }
        }
    }

    // Method to execute combo attack
    public void ExecuteComboAttack(int damage, DamageType damageType)
    {
        if (comboCount > 0)
        {
            // Logic for executing combo attack
            float totalDamage = damage * (1 + (comboCount * 0.5f)); // Calculate total damage
            ApplyDamage((int)totalDamage, damageType); // Apply the calculated damage
            // Trigger animations based on combo count
            if (comboCount == 1)
            {
                animator.SetTrigger("Attack1"); // Trigger attack1 animation
            }
            else
            {
                animator.SetTrigger("Attack2"); // Trigger attack2 animation
            }
            comboCount = 0; // Reset combo count after execution
        }
        else
        {
            ApplyDamage(damage, damageType); // Apply normal damage if no combo
        }
    }

    // Method to handle user input for melee attacks
    public void Attack(int damage, DamageType damageType)
    {
        // Check for user input to trigger combo attack
        comboCount++; // Increment combo count
        Debug.Log($"Combo Count: {comboCount}");
        comboTimer = comboDelay; // Reset timer
        ExecuteComboAttack(damage, damageType); // Call combo attack method
    }

    private void Update()
    {
        // Check for user input to trigger combo attack
        if (Input.GetKeyDown(KeyCode.C)) // Example input for attack
        {
            Attack(damage, damageType); // Call the Attack method
        }
        // Update combo timer
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime; // Decrease timer
        }
        else
        {
            comboCount = 0; // Reset combo count if timer expires
        }
    }
}
