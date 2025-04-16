using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Monster/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Core Identity")]
    public MonsterType monsterType;
    public string displayName;
    
    [Header("Base Stats")]
    [Range(1, 1000)] public int maxHealth = 100;
    [Range(0.1f, 10f)] public float moveSpeed = 3f;
    [Range(0.1f, 5f)] public float acceleration = 2f;
    
    [Header("Behavior Flags")]
    public bool canPatrol = true;
    public bool canChase = true;
    public bool canAttack = true;
    public bool canSummon = false;
    
    [Header("Combat Settings")]
    [Range(0.5f, 10f)] public float attackRange = 1.5f;
    [Range(0.1f, 5f)] public float attackCooldown = 1f;
    [Range(1, 100)] public int attackDamage = 10;
    
    [Header("AI Configuration")]
    [Range(1f, 20f)] public float aggroRange = 5f;
    [Range(0.1f, 5f)] public float decisionInterval = 0.3f;
    [Range(1f, 10f)] public float patrolRadius = 5f;
    
    [Header("Summoning Settings")]
    public MonsterData summonData; // Was 'summonPrefab'
    public GameObject summonPrefab; // Actual prefab reference
    [Range(0, 10)] public int maxSummons = 3;
    [Range(5f, 60f)] public float summonCooldown = 15f;
    [Range(1f, 5f)] public float summonRadius = 2f;
    
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
    public AudioClip summonSound;
    public AudioClip deathSound;
    
    [Header("Effects")]
    public GameObject summonEffect;
    public GameObject deathEffect;
}
