using UnityEngine;

[CreateAssetMenu(fileName = "NewBlackHoleBossData", menuName = "Game/Boss/BlackHoleBossData")]
public class BlackHoleBossData : ScriptableObject
{
    [Header("Health Settings")]
    public int maxHealth = 1000;

    [Header("Meteor Settings")]
    public GameObject meteorPrefab;
    public float meteorSpawnHeight = 5f;
    public float baseMeteorFallSpeed = 5f;
    public int baseMeteorDamage = 10;
    public float baseMeteorImpactRadius = 1f;

    [Header("Black Hole Spawn Settings")]
    public GameObject blackHolePrefab;
    public float minSpawnInterval = 10f;
    public float maxSpawnInterval = 15f;
    public float spawnDistanceFromBoss = 5f;

    [Header("Teleport Settings")]
    public float teleportCooldown = 8f;
    public float minTeleportDistance = 5f;
    public float maxTeleportDistance = 10f;

    [Header("Attack Settings")]
    public float attackCooldown = 10f;
    public float attackImpactRadius = 3f;
    public int attackDamage = 20;
    public LayerMask playerLayer;

    [Header("Audio")]
    public AudioClip attackAudio;

    [Header("Animations")]
    public string spawnAnimTrigger = "Cast";
    public string preTeleportTrigger = "PreTeleport";
    public string teleportTrigger = "Teleport";
    public string attackTrigger = "Attack";
}