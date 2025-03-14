using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private float effectDuration = 1f;

    // Reference to the spawner that created this health pack
    private HealthPackSpawner spawner;

    // Set the spawner reference
    public void Initialize(HealthPackSpawner spawnerRef)
    {
        spawner = spawnerRef;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is the player
        if (collision.CompareTag("Player"))
        {
            // Try to get the health component from the player
            HealthSystem playerHealth = collision.GetComponent<HealthSystem>();

            if (playerHealth != null)
            {
                // Heal the player
                playerHealth.Heal(healAmount);

                // Play pickup sound if assigned
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // Spawn pickup effect if assigned
                if (pickupEffect != null)
                {
                    GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    Destroy(effect, effectDuration);
                }

                // Notify spawner before destroying
                if (spawner != null)
                {
                    spawner.HandleHealthPackDestroyed();
                }

                // Destroy the health pack
                Destroy(gameObject);
            }
        }
    }
}
