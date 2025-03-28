using UnityEngine;
using System.Collections;

public class BlackHoleBoss : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float idealDistance = 10f; // Ideal distance from player
    [SerializeField] private float distanceThreshold = 2f; // Tolerance for ideal distance
    [SerializeField] private DirectionManager directionManager;

    [Header("Black Hole Settings")]
    [SerializeField] private GameObject blackHolePrefab;
    [SerializeField] private float blackHoleSpawnCooldown = 10f;
    [SerializeField] private float blackHoleLifetime = 5f;

    [Header("Health Settings")]
    [SerializeField] private HealthData healthData;
    public float damageReceiveRatio = 0.1f;
    private HealthSystem healthSystem;

    private Transform player;
    private Rigidbody2D rb;
    private float lastBlackHoleTime;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        if (directionManager == null)
        {
            directionManager = GetComponent<DirectionManager>();
            if (directionManager == null)
            {
                directionManager = gameObject.AddComponent<DirectionManager>();
            }
        }
        // Initialize health system
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem == null)
        {
            healthSystem = gameObject.AddComponent<HealthSystem>();
        }

        // Subscribe to death event
        healthSystem.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        // Add boss death logic here
        Destroy(gameObject);
    }

    private void TakeDamage(int damage)
    {
        damage = (int)(damage * damageReceiveRatio);
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage, DamageType.Normal);
        }
    }

    private void Update()
    {
        if (player == null) return;

        HandlePositioning();
        HandleBlackHoleSpawn();
    }

    private void HandlePositioning()
    {
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Determine movement direction
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        directionManager.UpdateDirection(directionToPlayer.x, directionToPlayer.y);
        
        if (distanceToPlayer > idealDistance + distanceThreshold)
        {
            // Move closer to player
            rb.velocity = directionToPlayer * moveSpeed;
        }
        else if (distanceToPlayer < idealDistance - distanceThreshold)
        {
            // Move away from player
            rb.velocity = -directionToPlayer * moveSpeed;
        }
        else
        {
            // Maintain position
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleBlackHoleSpawn()
    {
        if (Time.time - lastBlackHoleTime >= blackHoleSpawnCooldown)
        {
            SpawnBlackHole();
            lastBlackHoleTime = Time.time;
        }
    }

    private void SpawnBlackHole()
    {
        if (blackHolePrefab != null && player != null)
        {
            // Spawn black hole at player position
            Vector2 spawnPosition = (Vector2)player.transform.position + 
                Random.insideUnitCircle * 1f; 
            
            GameObject blackHole = Instantiate(blackHolePrefab, spawnPosition, Quaternion.identity);
            
            // Add destroy coroutine
            StartCoroutine(DestroyBlackHoleAfterDelay(blackHole));
        }
    }

    private IEnumerator DestroyBlackHoleAfterDelay(GameObject blackHole)
    {
        yield return new WaitForSeconds(blackHoleLifetime);
        Destroy(blackHole);
    }

    // Visualization for editor
    private void OnDrawGizmosSelected()
    {
        // Draw ideal distance circle
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, idealDistance);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, idealDistance - distanceThreshold);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, idealDistance + distanceThreshold);
        }
    }
}