using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private float spawnDistance = 15f;
    [SerializeField] private float despawnDistance = 25f;
    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private int maxActiveMobs = 12;
    [SerializeField] private float spawnCooldown = 2.5f;
    [SerializeField] private int maxSpawnAttempts = 5;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Mob Prefabs")]
    [SerializeField] private List<GameObject> mobPrefabs;
    [SerializeField] private Transform mobContainer;

    [Header("References")]
    [SerializeField] private Transform player;

    private Dictionary<string, Queue<GameObject>> mobPool = new Dictionary<string, Queue<GameObject>>();
    private List<GameObject> activeMobs = new List<GameObject>();
    private float cooldownTimer;
    private Vector2 lastPlayerPosition;
    private bool spawningEnabled = true;
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
            if (IsValidSpawnPoint(candidate)) return candidate;
        }
        return Vector2.zero;
    }

    private Vector2 CalculateCandidatePosition(Vector2 direction)
    {
        Vector2 basePosition = (Vector2)player.position + (direction * spawnDistance);
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 spawnPos = basePosition + randomOffset;

        RaycastHit2D groundHit = Physics2D.Raycast(
            spawnPos + Vector2.up * 2f,
            Vector2.down,
            4f,
            groundLayer
        );

        return groundHit.collider ? groundHit.point : spawnPos;
    }

    private bool IsValidSpawnPoint(Vector2 position)
    {
        return Physics2D.OverlapCircle(position, 0.5f, groundLayer);
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
        return mobPrefabs[Random.Range(0, mobPrefabs.Count)];
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
        
        // Ensure states are initialized before switching
        if (monster is Necromancer necro)
        {
            necro.Initialize();
        }
        
        // Now it's safe to switch to Idle state
        monster.stateMachine.SwitchState(MonsterStateType.Idle);
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
        foreach (GameObject prefab in mobPrefabs)
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

    // Modify ShouldSpawn
    private bool ShouldSpawn()
    {
        return spawningEnabled && 
               activeMobs.Count < maxActiveMobs && 
               cooldownTimer <= 0;
    }
    private void OnDrawGizmosSelected()
    {
        if (!player) return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, spawnDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, despawnDistance);
    }
}