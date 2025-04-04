using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Monster/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Stats")]
    public MonsterType monsterType;
    public int maxHealth;
    public float damage;
    public float attackCooldown;
    public float attackRange;
    
    [Header("Movement")]
    public float patrolSpeed;
    public float chaseSpeed;
    public float patrolTime;
    
    [Header("Visuals")]
    public Sprite monsterSprite;
    public Color healthBarColor;
    public float healthBarWidth;
    public float healthBarHeight;
    
    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip[] deathSounds;
    public AudioClip hitSound;
    public AudioClip movementSound;
    
    [Header("Special Effects")]
    public GameObject hitEffect;
    public GameObject deathEffect;
    public GameObject attackEffect;
    
    [Header("Drops")]
    [Range(0, 1)]
    public float deathDropChance = 0.5f;
    public GameObject[] possibleDrops;
    
    [Header("AI Settings")]
    public float aggroRange;
    public float idleTime;
    public float chaseTime;
    public float attackDuration;
    public float deathDuration;
}