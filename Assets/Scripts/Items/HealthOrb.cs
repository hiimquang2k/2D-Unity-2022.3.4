using UnityEngine;

public class HealthOrb : MonoBehaviour
{
    [Header("Health Orb Settings")]
    [SerializeField] private float healAmount = 50f;
    [SerializeField] private float despawnTime = 10f;
    [SerializeField] private float pickupRange = 1f;
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private Animator animator;
    
    [Header("Animation Parameters")]
    [SerializeField] private string idleAnimTrigger = "Idle";

    private void Start()
    {
        // Destroy orb after despawn time
        Destroy(gameObject, despawnTime);
        
        // Start idle animation
        if (animator != null)
        {
            animator.Play(idleAnimTrigger);
        }
    }

    private void Update()
    {
        // Check for player pickup
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var healthSystem = player.GetComponent<HealthSystem>();
            if (healthSystem != null && Vector2.Distance(transform.position, player.transform.position) <= pickupRange)
            {
                Pickup(healthSystem);
            }
        }
    }

    private void Pickup(HealthSystem healthSystem)
    {
        // Play pickup effect if available
        if (pickupEffect != null)
        {
            var effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Heal the player
        healthSystem.Heal((int)healAmount);
        
        // Destroy the orb
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.gameObject;
        var healthSystem = player.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            Pickup(healthSystem);
        }
    }
}
