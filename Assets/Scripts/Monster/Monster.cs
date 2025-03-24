// Monster.cs
using UnityEngine;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;
    private HealthSystem healthSystem;
    private Animator animator;
    private Rigidbody2D rb;
    private MonsterMovement movement;
    private AudioSource audioSource;
    protected Transform target;

    private void Start()
    {
        InitializeComponents();
        InitializeMonster();
    }

    private void InitializeComponents()
    {
        healthSystem = GetComponent<HealthSystem>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void InitializeMonster()
    {
        // Set up health system
        healthSystem.CurrentHealth = monsterData.baseHealth;
        
        // Set up animator
        if (animator != null && monsterData.animatorController != null)
        {
            animator.runtimeAnimatorController = monsterData.animatorController;
        }
        
        // Set up sprite
        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().sprite = monsterData.monsterSprite;
        }
        
        // Set up audio
        if (audioSource != null)
        {
            audioSource.clip = monsterData.attackSounds[0];
        }
        
        // Initialize movement
        if (monsterData.canPatrol)
        {
            movement = gameObject.AddComponent<PatrolMovement>();
            PatrolMovement patrol = (PatrolMovement)movement;
            patrol.MoveSpeed = monsterData.patrolSpeed;
            patrol.waitTime = monsterData.patrolWaitTime;
        }
        else if (monsterData.canChase)
        {
            movement = gameObject.AddComponent<ChaseMovement>();
            ChaseMovement chase = (ChaseMovement)movement;
            chase.MoveSpeed = monsterData.moveSpeed;
            chase.ChaseRange = monsterData.chaseRange;
        }
        else if (monsterData.canRandomMove)
        {
            movement = gameObject.AddComponent<RandomMovement>();
            RandomMovement random = (RandomMovement)movement;
            random.MoveSpeed = monsterData.moveSpeed;
            random.changeDirectionTime = monsterData.attackCooldown;
        }
    }

    public virtual void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
        
        if (audioSource != null && monsterData.attackSounds.Length > 0)
        {
            audioSource.PlayOneShot(monsterData.attackSounds[Random.Range(0, monsterData.attackSounds.Length)]);
        }
        
        // Play attack effects
        if (monsterData.attackEffect != null)
        {
            Instantiate(monsterData.attackEffect, transform.position, Quaternion.identity);
        }
    }

    private void Die()
    {
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("death");
        }
        
        // Play death sound
        if (audioSource != null && monsterData.deathSounds.Length > 0)
        {
            audioSource.PlayOneShot(monsterData.deathSounds[Random.Range(0, monsterData.deathSounds.Length)]);
        }
        
        // Handle drops
        if (Random.value <= monsterData.deathDropChance && monsterData.possibleDrops.Length > 0)
        {
            GameObject drop = monsterData.possibleDrops[Random.Range(0, monsterData.possibleDrops.Length)];
            Instantiate(drop, transform.position, Quaternion.identity);
        }
        
        // Play death effect
        if (monsterData.deathEffect != null)
        {
            Instantiate(monsterData.deathEffect, transform.position, Quaternion.identity);
        }
        
        // Destroy the monster
        Destroy(gameObject);
    }
}