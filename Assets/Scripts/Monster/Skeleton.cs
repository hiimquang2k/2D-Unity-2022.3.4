using UnityEngine;
using System;

public class Skeleton : Monster
{
    public event Action<Skeleton> OnDeath;
    private Necromancer _summoner;
    private HealthSystem _healthSystem;
    [SerializeField] private bool _statesInitialized = false;
private void Awake()
{
    _healthSystem = GetComponent<HealthSystem>();
    if (_healthSystem != null)
    {
        _healthSystem.OnDeath += TriggerDeath;
    }
}

private void OnDestroy()
{
    if (_healthSystem != null)
    {
        _healthSystem.OnDeath -= TriggerDeath;
    }
}
    protected override void InitializeStates()
    {
            if (_statesInitialized) 
    {
        Debug.LogWarning("States already initialized for " + gameObject.name);
        return;
    }

    Debug.Log("Initializing states for " + gameObject.name);
        stateMachine.AddState(MonsterStateType.Death, new PooledDeathState(this));
        stateMachine.AddState(MonsterStateType.Idle, new IdleState(this, true));
        stateMachine.AddState(MonsterStateType.Chase, new ChaseState(this, Data.decisionInterval));
        stateMachine.AddState(MonsterStateType.Attack, new SkeletonAttackState(this));
        
        if (Data.canPatrol)
        {
            stateMachine.AddState(MonsterStateType.Patrol, new PatrolState(this));
        }

        _statesInitialized = true;
    }

    public void Initialize(Necromancer summoner)
    {
        _summoner = summoner;
        ResetSkeleton();
    }
    public void ResetSkeleton()
    {
        // Reset health, colliders, etc.
        GetComponent<Collider2D>().enabled = true;
        GetComponent<DamageSystem>().SetDamageable(true);
        
        // Reset visual
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color color = sr.color;
        color.a = 1f;
        sr.color = color;
    }

private bool _isDying = false; // Add this flag

public void TriggerDeath()
{
    // Prevent multiple death triggers
    if (_isDying) return;
    _isDying = true;

    // 1. First ensure states exist (MUST happen before any state switching)
    if (!stateMachine.HasState(MonsterStateType.Death))
    {
        Debug.LogWarning("Death state missing! Force-initializing...");
        InitializeStates();
    }

    // 2. Notify listeners
    OnDeath?.Invoke(this);

    // 3. Switch state (now guaranteed to exist)
    stateMachine.SwitchState(MonsterStateType.Death);

    // 4. Cleanup
    OnDeath = null;
}

public void PoolInitialize()
{
    _isDying = false;
    _statesInitialized = false; // Force re-init
    InitializeStates();
    ResetSkeleton();
    stateMachine.SwitchState(MonsterStateType.Idle);
}
}