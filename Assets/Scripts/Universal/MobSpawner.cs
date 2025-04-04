using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private float spawnDistance = 20f;    // How far ahead to spawn mobs
    [SerializeField] private float despawnDistance = 30f;  // How far away to despawn mobs
    [SerializeField] private float spawnRadius = 10f;      // Radius around spawn point to randomly place mobs
    [SerializeField] private int maxActiveMobs = 15;       // Maximum number of active mobs at once
    [SerializeField] private float spawnCooldown = 3f;     // Time between spawn attempts
    
    [Header("Mob Prefabs")]
    [SerializeField] private List<GameObject> mobPrefabs;  // List of available mob prefabs
    [SerializeField] private Transform mobContainer;       // Parent object for spawned mobs
    
    [Header("References")]
    [SerializeField] private Transform player;             // Player transform to track
    
    // Pool of inactive mobs
    private Dictionary<string, Queue<GameObject>> mobPool = new Dictionary<string, Queue<GameObject>>();
    
    // List of currently active mobs
    private List<GameObject> activeMobs = new List<GameObject>();
    
    // Last known player position for movement direction calculation
    private Vector3 lastPlayerPosition;
    
    // Cooldown timer
    private float cooldownTimer = 0f;
    
    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        if (mobContainer == null)
        {
            mobContainer = transform;
        }
        
        lastPlayerPosition = player.position;
        
        // Initialize mob pools
        InitializeMobPool();
    }
    
    private void Update()
    {
        // Update cooldown timer
        cooldownTimer -= Time.deltaTime;
        
        // Get player movement direction
        Vector3 playerDirection = (player.position - lastPlayerPosition).normalized;
        
        // If player hasn't moved much, use forward direction
        if (playerDirection.magnitude < 0.1f)
        {
            playerDirection = player.forward;
        }
        
        // Try to spawn mobs if we're under the limit and cooldown is ready
        if (activeMobs.Count < maxActiveMobs && cooldownTimer <= 0f)
        {
            SpawnMobAhead(playerDirection);
            cooldownTimer = spawnCooldown;
        }
        
        // Check for mobs to despawn
        ManageMobLifecycle();
        
        // Update last position
        lastPlayerPosition = player.position;
    }
    
    private void InitializeMobPool()
    {
        // Create initial pools for each mob type
        foreach (GameObject mobPrefab in mobPrefabs)
        {
            string mobType = mobPrefab.name;
            mobPool[mobType] = new Queue<GameObject>();
            
            // Pre-spawn 5 of each type
            for (int i = 0; i < 5; i++)
            {
                GameObject mob = Instantiate(mobPrefab, mobContainer);
                mob.SetActive(false);
                mobPool[mobType].Enqueue(mob);
            }
        }
    }
    
    private void SpawnMobAhead(Vector3 playerDirection)
    {
        // Choose spawn position ahead of player in their movement direction
        Vector3 spawnCenter = player.position + playerDirection * spawnDistance;
        
        // Randomize within spawn radius (on the xz plane for most games)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
        Vector3 spawnPosition = spawnCenter + randomOffset;
        
        // Raycast to find ground level
        if (Physics.Raycast(spawnPosition + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            spawnPosition.y = hit.point.y;
        }
        
        // Choose a random mob type
        GameObject mobPrefab = mobPrefabs[Random.Range(0, mobPrefabs.Count)];
        string mobType = mobPrefab.name;
        
        // Get mob from pool or create new one
        GameObject mob = GetMobFromPool(mobType);
        
        // Position and activate the mob
        mob.transform.position = spawnPosition;
        mob.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        mob.SetActive(true);
        
        // Add to active mobs list
        activeMobs.Add(mob);
    }
    
    private GameObject GetMobFromPool(string mobType)
    {
        // Try to get from pool
        if (mobPool.ContainsKey(mobType) && mobPool[mobType].Count > 0)
        {
            return mobPool[mobType].Dequeue();
        }
        
        // Create new if pool is empty
        GameObject mobPrefab = mobPrefabs.Find(p => p.name == mobType);
        GameObject newMob = Instantiate(mobPrefab, mobContainer);
        return newMob;
    }
    
    private void ReturnMobToPool(GameObject mob)
    {
        // Deactivate
        mob.SetActive(false);
        
        // Remove from active list
        activeMobs.Remove(mob);
        
        // Add to appropriate pool
        string mobType = mob.name.Replace("(Clone)", "").Trim();
        if (!mobPool.ContainsKey(mobType))
        {
            mobPool[mobType] = new Queue<GameObject>();
        }
        
        mobPool[mobType].Enqueue(mob);
    }
    
    private void ManageMobLifecycle()
    {
        // Check distance for each active mob
        for (int i = activeMobs.Count - 1; i >= 0; i--)
        {
            GameObject mob = activeMobs[i];
            if (mob == null)
            {
                // Remove null references (destroyed mobs)
                activeMobs.RemoveAt(i);
                continue;
            }
            
            float distanceToPlayer = Vector3.Distance(mob.transform.position, player.position);
            
            // Despawn if too far
            if (distanceToPlayer > despawnDistance)
            {
                ReturnMobToPool(mob);
            }
        }
    }
    
    // Optional: Visualization for debugging
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        // Show spawn distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, spawnDistance);
        
        // Show despawn distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, despawnDistance);
    }
}