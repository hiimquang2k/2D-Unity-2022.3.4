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
    private Animator animator; // Animator for triggering animations
    private int monsterDamage = 10;
    private DamageType damageType = DamageType.Physical;
    private void Start()
    {
        animator = GetComponent<Animator>(); // Initialize the Animator
    }

    // Method to apply damage to the target


    // Method to handle collision with monsters
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            healthSystem.TakeDamage(monsterDamage, damageType); 
            Animator monsterAnimator = collision.GetComponent<Animator>();
            if (monsterAnimator != null)
            {
                monsterAnimator.SetTrigger("hit"); // Trigger the hit animation
            }
        }
    }
}
