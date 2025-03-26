using UnityEngine;

public class BlackHoleBoss : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float idealDistance = 10f; // Ideal distance from player
    [SerializeField] private float distanceThreshold = 2f; // Tolerance for ideal distance

    [Header("Black Hole Settings")]
    [SerializeField] private GameObject blackHolePrefab;
    [SerializeField] private float blackHoleSpawnCooldown = 10f;
    [SerializeField] private float blackHoleLifetime = 5f;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 3f;

    private Transform player;
    private Rigidbody2D rb;
    private float lastBlackHoleTime;
    private float currentHealth;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
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
            // Maintain position with slight orbiting
            Vector2 perpendicularDirection = new Vector2(-directionToPlayer.y, directionToPlayer.x);
            rb.velocity = perpendicularDirection * (moveSpeed * 0.5f);
        }
    }

    private void HandleBlackHoleSpawn()
    {
        if (Time.time - lastBlackHoleTime > blackHoleSpawnCooldown)
        {
            SpawnBlackHole();
            lastBlackHoleTime = Time.time;
        }
    }

    private void SpawnBlackHole()
    {
        if (blackHolePrefab != null)
        {
            // Spawn black hole at boss's position
            GameObject blackHole = Instantiate(blackHolePrefab, transform.position, Quaternion.identity);
            Destroy(blackHole, blackHoleLifetime);
        }
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