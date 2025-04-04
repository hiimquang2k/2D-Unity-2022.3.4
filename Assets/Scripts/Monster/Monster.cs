using UnityEngine;
using System;
using System.Collections.Generic;

public interface IMonsterBehavior
{
    Transform Target { get; }
    Animator Animator { get; }
    Rigidbody2D Rigidbody { get; }
    MonsterData MonsterData { get; }
    void SwitchState(MonsterStateType newState);
    void Patrol();
    void Chase();
    void Idle();
    void Attack();
}

public class Monster : MonoBehaviour, IMonsterBehavior
{
    [Header("Monster Settings")]
    [SerializeField] public MonsterData monsterData;
    [SerializeField] public Animator animator;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private DamageSystem damageSystem;
    [SerializeField] private CooldownSystem cooldownSystem;
    
    public Transform target;
    private MonsterState currentState;
    private readonly Dictionary<MonsterStateType, MonsterState> stateMap = new();

    public Transform Target => target;

    public Animator Animator => animator;

    public Rigidbody2D Rigidbody => rb;

    public MonsterData MonsterData => monsterData;

    private void Awake()
    {
        InitializeComponents();
        InitializeMonster();
        InitializeStates();
        SwitchState(MonsterStateType.Idle);
    }

    private void InitializeComponents()
    {
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (damageSystem == null) damageSystem = GetComponent<DamageSystem>();
        if (cooldownSystem == null) cooldownSystem = GetComponent<CooldownSystem>();

        if (damageSystem != null && healthSystem != null)
            damageSystem.healthSystem = healthSystem;

        if (healthSystem == null)
            Debug.LogError($"HealthSystem not found on {name}");
        if (animator == null)
            Debug.LogError($"Animator not found on {name}");
    }

    private void InitializeMonster()
    {
        if (monsterData == null)
        {
            Debug.LogError($"MonsterData not set for {name}");
            return;
        }

        if (healthSystem != null)
        {
            healthSystem.SetMaxHealth(monsterData.maxHealth);
            healthSystem.OnHealthChanged += OnHealthChanged;
            healthSystem.SetCurrentHealth(monsterData.maxHealth);
        }
        else
        {
            Debug.LogError($"HealthSystem not found on {name}");
        }

        target = null;
    }

    private void InitializeStates()
    {
        stateMap.Clear();
        stateMap[MonsterStateType.Idle] = new IdleState(this);
        stateMap[MonsterStateType.Patrol] = new PatrolState(this);
        stateMap[MonsterStateType.Chase] = new ChaseState(this);
        stateMap[MonsterStateType.Attack] = new AttackState(this);
        stateMap[MonsterStateType.Death] = new DeathState(this);
    }

    public void SwitchState(MonsterStateType newState)
    {
        if (currentState != null)
            currentState.Exit();

        if (!stateMap.TryGetValue(newState, out var newStateInstance))
        {
            Debug.LogError($"State {newState} not found in state map");
            return;
        }

        currentState = newStateInstance;
        currentState.Enter();
    }

    private void Update()
    {
        if (currentState != null)
            currentState.Update();

        cooldownSystem?.UpdateCooldown();
    }

    public void Attack()
    {
        if (currentState == null || cooldownSystem == null)
            return;

        if (!cooldownSystem.IsOnCooldown("attack"))
        {
            currentState.Attack();
            cooldownSystem.StartCooldown(monsterData.attackCooldown, "attack");
        }
    }

    public void OnTakeDamage(float damage, DamageType damageType = DamageType.Normal, Vector2 damageSource = default)
    {
        if (damageSystem == null)
        {
            Debug.LogError($"DamageSystem not found on {name}");
            return;
        }

        if (damage <= 0)
        {
            Debug.LogWarning($"Attempted to take non-positive damage ({damage}) on {name}");
            return;
        }

        int damageAmount = Mathf.FloorToInt(damage);
        damageSystem.TakeDamage(damageAmount, damageType, damageSource);
    }

    private void OnDeath()
    {
        SwitchState(MonsterStateType.Death);
    }

    public void Die()
    {
        if (animator != null)
            animator.SetTrigger("death");

        if (audioSource != null && monsterData.deathSounds != null && monsterData.deathSounds.Length > 0)
            audioSource.PlayOneShot(monsterData.deathSounds[UnityEngine.Random.Range(0, monsterData.deathSounds.Length)]);

        if (monsterData.deathDropChance > 0 && monsterData.possibleDrops != null && monsterData.possibleDrops.Length > 0)
        {
            if (UnityEngine.Random.value <= monsterData.deathDropChance)
            {
                GameObject drop = monsterData.possibleDrops[UnityEngine.Random.Range(0, monsterData.possibleDrops.Length)];
                Instantiate(drop, transform.position, Quaternion.identity);
            }
        }

        if (monsterData.deathEffect != null)
            Instantiate(monsterData.deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public void Patrol() => SwitchState(MonsterStateType.Patrol);
    public void Chase() => SwitchState(MonsterStateType.Chase);
    public void Idle() => SwitchState(MonsterStateType.Idle);

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning($"Attempted to set null target for {name}");
            return;
        }

        target = newTarget;
    }

    public Transform GetTarget() => target;

    public float GetCurrentHealth() => healthSystem?.CurrentHealth ?? 0;
    public float GetMaxHealth() => monsterData?.maxHealth ?? 0;

    public void AttackState() => SwitchState(MonsterStateType.Attack);

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        // Handle health changes here if needed
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= OnHealthChanged;
        }
    }
}

public enum MonsterStateType
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Death
}