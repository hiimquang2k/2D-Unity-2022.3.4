using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpawnZone", menuName = "Game/Spawn Zone", order = 1)]
public class SpawnZone : ScriptableObject
{
    [Header("Zone Settings")]
    public string zoneName;
    public Vector2 zoneCenter;
    public float zoneRadius;
    
    [Header("Mob Types")]
    public List<GameObject> allowedMobs;
    public float spawnRateMultiplier = 1f;
    
    [Header("Spawn Conditions")]
    public bool requiresDay = false;
    public bool requiresNight = false;
    public bool requiresBossAlive = false;
    public bool requiresBossDead = false;
    
    public bool IsWithinZone(Vector2 position)
    {
        return Vector2.Distance(position, zoneCenter) <= zoneRadius;
    }
    
    public bool CanSpawnMobs()
    {
        // Add more conditions as needed
        return true;
    }
}
