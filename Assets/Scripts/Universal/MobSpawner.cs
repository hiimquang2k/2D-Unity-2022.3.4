using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MobSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private float spawnDistance = 15f;
    [SerializeField] private float despawnDistance = 25f;
    [SerializeField] private float spawnWidth = 16f;
    [SerializeField] private float spawnHeight = 10f;
    [SerializeField] private int maxActiveMobs = 12;
    [SerializeField] private float spawnCooldown = 2.5f;
    [SerializeField] private int maxSpawnAttempts = 5;
    [SerializeField] private float minHeightAbovePlayer = 1f;
    [SerializeField] private float minHeightAboveGround = 1f;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Mob Prefabs")]
    [SerializeField] private List<GameObject> defaultMobPrefabs;
    [SerializeField] private Transform mobContainer;

    [Header("Spawn Zones")]
    [SerializeField] private List<SpawnZone> spawnZones = new List<SpawnZone>();

    [Header("References")]
    [SerializeField] private Transform player;

    private Dictionary<string, Queue<GameObject>> mobPool = new Dictionary<string, Queue<GameObject>>();
    private List<GameObject> activeMobs = new List<GameObject>();
    private float cooldownTimer;
    private Vector2 lastPlayerPosition;
    private bool spawningEnabled = true;
    private SpawnZone currentZone = null;

    private void Start()
    {
        InitializeReferences();
        InitializeMobPool();
    }

    private void InitializeReferences()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
        if (!mobContainer) mobContainer = transform;
        lastPlayerPosition = player.position;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;
        UpdateSpawning();
        ManageMobLifecycle();
        UpdatePlayerTracking();
    }

    private void UpdateSpawning()
    {
        if (ShouldSpawn())
        {
            AttemptSpawn();
            cooldownTimer = spawnCooldown;
        }
    }

    private void AttemptSpawn()
    {
        Vector2 spawnDirection = GetSpawnDirection();
        Vector2 spawnPosition = FindValidSpawnPosition(spawnDirection);

        if (spawnPosition != Vector2.zero)
        {
            SpawnMob(spawnPosition);
        }
    }

    private Vector2 GetSpawnDirection()
    {
        Vector2 rawDirection = ((Vector2)player.position - lastPlayerPosition).normalized;
        return rawDirection.magnitude < 0.1f ? Vector2.right : rawDirection;
    }

    private Vector2 FindValidSpawnPosition(Vector2 direction)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 candidate = CalculateCandidatePosition(direction);
            if (IsValidSpawnPoint(candidate))
            {
                return candidate;
            }
        }
        return Vector2.zero;
    }

    private Vector2 CalculateCandidatePosition(Vector2 direction)
    {
        // Calculate base position in front of the player
        Vector2 basePosition = (Vector2)player.position + (direction * spawnDistance);

        // Calculate random offset within rectangle bounds
        float randomX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);

        // Find ground height at this x position
        float groundHeight = FindGroundHeight(basePosition.x + randomX);

        // Calculate y position above ground and player
        float minY = Mathf.Max(player.position.y, groundHeight) + minHeightAboveGround;
        float randomY = Random.Range(0, spawnHeight);

        return new Vector2(basePosition.x + randomX, minY + randomY);
    }

    private float FindGroundHeight(float xPosition)
    {
        // Raycast down from above the player to find ground height
        Vector2 rayStart = new Vector2(xPosition, player.position.y + 10f);
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 20f, groundLayer);

        return hit.collider ? hit.point.y : player.position.y;
    }

    private bool IsValidSpawnPoint(Vector2 position)
    {
        // Check if there's ground below this position
        RaycastHit2D groundCheck = Physics2D.Raycast(
            position + Vector2.up * 0.5f, // Start slightly above
            Vector2.down,
            spawnHeight + 1f, // Check all the way down through our spawn height
            groundLayer
        );

        // Position is valid if:
        // 1. It's above the player (y position check)
        // 2. There is ground below it
        // 3. There are no obstacles at the spawn position itself
        bool hasGroundBelow = groundCheck.collider != null;
        bool isAbovePlayer = position.y > player.position.y;
        bool isClearAtPosition = !Physics2D.OverlapPoint(position, groundLayer);

        return isAbovePlayer && hasGroundBelow && isClearAtPosition;
    }

    private void SpawnMob(Vector2 position)
    {
        GameObject mobPrefab = GetRandomMobPrefab();
        GameObject mob = GetPooledMob(mobPrefab);

        mob.transform.position = position;
        mob.SetActive(true);
        activeMobs.Add(mob);

        InitializeMonsterComponents(mob);
    }

    private GameObject GetRandomMobPrefab()
    {
        if (currentZone != null && currentZone.allowedMobs.Count > 0)
        {
            return currentZone.allowedMobs[Random.Range(0, currentZone.allowedMobs.Count)];
        }
        return defaultMobPrefabs[Random.Range(0, defaultMobPrefabs.Count)];
    }

    private GameObject GetPooledMob(GameObject prefab)
    {
        string key = prefab.name;

        if (mobPool.TryGetValue(key, out Queue<GameObject> queue) && queue.Count > 0)
        {
            return queue.Dequeue();
        }

        GameObject newMob = Instantiate(prefab, mobContainer);
        return newMob;
    }

    private void InitializeMonsterComponents(GameObject mob)
    {
        Monster monster = mob.GetComponent<Monster>();
        if (!monster) return;

        monster.Target = player;

        if (monster is Necromancer necro)
        {
            necro.Initialize();
        }

        //monster.stateMachine.SwitchState(MonsterStateType.Idle);
    }

    private void ManageMobLifecycle()
    {
        for (int i = activeMobs.Count - 1; i >= 0; i--)
        {
            GameObject mob = activeMobs[i];
            if (!mob)
            {
                activeMobs.RemoveAt(i);
                continue;
            }

            if (Vector2.Distance(mob.transform.position, player.position) > despawnDistance)
            {
                ReturnMobToPool(mob);
            }
        }
    }

    private void ReturnMobToPool(GameObject mob)
    {
        mob.SetActive(false);
        activeMobs.Remove(mob);

        Monster monster = mob.GetComponent<Monster>();
        if (monster) monster.stateMachine.SwitchState(MonsterStateType.Idle);

        string key = mob.name.Replace("(Clone)", "").Trim();
        if (!mobPool.ContainsKey(key))
        {
            mobPool[key] = new Queue<GameObject>();
        }
        mobPool[key].Enqueue(mob);
    }

    private void InitializeMobPool()
    {
        foreach (GameObject prefab in defaultMobPrefabs)
        {
            string key = prefab.name;
            mobPool[key] = new Queue<GameObject>();

            for (int i = 0; i < 3; i++)
            {
                GameObject mob = Instantiate(prefab, mobContainer);
                mob.SetActive(false);
                mobPool[key].Enqueue(mob);
            }
        }
    }

    private void UpdatePlayerTracking()
    {
        lastPlayerPosition = player.position;
        UpdateCurrentZone();
    }

    private void UpdateCurrentZone()
    {
        foreach (var zone in spawnZones)
        {
            if (zone.IsWithinZone(player.position))
            {
                if (zone.CanSpawnMobs())
                {
                    currentZone = zone;
                    return;
                }
            }
        }
        currentZone = null;
    }

    public void ToggleSpawning(bool enable)
    {
        spawningEnabled = enable;
        if (!enable) ForceDespawnAllMobs();
    }

    private void ForceDespawnAllMobs()
    {
        foreach (GameObject mob in activeMobs.ToArray())
        {
            ReturnMobToPool(mob);
        }
    }

    private bool ShouldSpawn()
    {
        return spawningEnabled &&
               activeMobs.Count < maxActiveMobs &&
               cooldownTimer <= 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (!player) return;

        // Draw spawn rectangle
        Gizmos.color = Color.green;
        Vector2 rectCenter = (Vector2)player.position +
                           (Vector2.right * spawnDistance) +
                           Vector2.up * (minHeightAbovePlayer + spawnHeight / 2f);
        Vector2 rectSize = new Vector2(spawnWidth, spawnHeight);
        Gizmos.DrawWireCube(rectCenter, rectSize);

        // Draw despawn distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, despawnDistance);

        // Draw spawn zones
        if (spawnZones != null)
        {
            foreach (var zone in spawnZones)
            {
                if (zone.zoneCenter != null)
                {
                    // Draw zone outline
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(zone.zoneCenter, zone.zoneRadius);
                    
                    // Draw zone center point
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(zone.zoneCenter, 0.2f);
                    
                    // Draw zone name text
                    Handles.Label(zone.zoneCenter, zone.zoneName);
                    
                    // Draw allowed mobs text
                    if (zone.allowedMobs.Count > 0)
                    {
                        Vector2 textPosition = zone.zoneCenter + Vector2.up * (zone.zoneRadius + 0.5f);
                        string mobsText = "Allowed Mobs:\n";
                        foreach (GameObject mob in zone.allowedMobs)
                        {
                            mobsText += mob.name + "\n";
                        }
                        Handles.Label(textPosition, mobsText);
                    }
                }
            }
        }
    }
}