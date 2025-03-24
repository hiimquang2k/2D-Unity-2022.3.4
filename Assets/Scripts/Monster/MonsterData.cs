// MonsterData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Game/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Monster Info")]
    public MonsterType type;
    public Sprite monsterSprite;
    public RuntimeAnimatorController animatorController;
    public Color damageColor;
    public float damageMultiplier;

    [Header("Health & Damage")]
    public int baseHealth;
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;
    public float attackKnockbackForce;
    public DamageType attackDamageType;
    public float attackAnimationDuration;
    public float attackCooldownVariation;

    [Header("Movement")]
    public float moveSpeed;
    public bool canPatrol;
    public bool canChase;
    public bool canRandomMove;
    public float patrolSpeed;
    public float patrolWaitTime;
    public float chaseRange;

    [Header("Drops & Effects")]
    public float deathDropChance;
    public GameObject deathEffect;
    public GameObject[] possibleDrops;
    public GameObject hitEffect;
    public GameObject attackEffect;
    public GameObject idleEffect;
    public GameObject[] attackEffects;

    [Header("Sound Effects")]
    public AudioClip[] attackSounds;
    public AudioClip[] deathSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] movementSounds;
}