using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Game/Monster/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Core Identity")]
    public MonsterType monsterType;
    public string displayName;

    [Header("Base Stats")]
    [Range(1, 1000)] public int maxHealth = 100;
    [Range(0.1f, 10f)] public float moveSpeed = 3f;
    [Range(0.1f, 5f)] public float acceleration = 2f;

    [Header("XP Reward")]
    [Range(1, 100)] public int xpReward = 10;
    
    [Header("Behavior Flags")]
    public bool canPatrol = true;
    public bool canChase = true;
    public bool canAttack = true;
    public bool canSummon = false;

    [Header("Combat Settings")]
    [Range(0.1f, 5f)] public float attackHeight = 1f; 
    [Range(0.5f, 10f)] public float attackRange = 1.5f;
    [Range(0.1f, 5f)] public float attackCooldown = 1f;
    [Range(0f, 1f)] public float attackVariance = 0.2f; // NEW: Cooldown randomness
    [Range(1, 100)] public int attackDamage = 10;
    public GameObject attackEffect; // NEW: Visual effect for attacks

    [Header("AI Configuration")]
    [Range(0.1f, 10f)] public float chaseSpeed = 5f;
    [Range(1f, 20f)] public float aggroRange = 5f;
    [Range(0.1f, 5f)] public float decisionInterval = 0.3f;
    [Range(1f, 10f)] public float patrolRadius = 5f;
    [Range(1f, 5f)] public float maxVerticalAggro = 1.5f;
    [Range(0.1f, 5f)] public float groundCheckDistance = 0.3f;

    [Header("Visual Settings")]
    public RuntimeAnimatorController animatorController;
    public Sprite defaultSprite;
    public Vector2 colliderSize = new Vector2(0.5f, 0.5f);

    [Header("Animation Parameters")]
    public string moveSpeedParam = "MoveSpeed";
    public string attackTrigger = "Attack";
    public string summonTrigger = "Summon";
    public string deathTrigger = "Death";

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip deathSound;

    [Header("Effects")]
    public GameObject deathEffect;
}