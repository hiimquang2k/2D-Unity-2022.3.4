using UnityEngine;
using System;

public class Skeleton : Monster
{
    public event Action<Skeleton> OnDeath;
    private Necromancer _summoner;

    protected override void InitializeStates()
    {
    // Use MonsterData values for configuration
    stateMachine.AddState(MonsterStateType.Idle, new IdleState(this, true));
    stateMachine.AddState(MonsterStateType.Chase, new ChaseState(this, this.Data.decisionInterval));
    stateMachine.AddState(MonsterStateType.Attack, new SkeletonAttackState(this));
    stateMachine.AddState(MonsterStateType.Death, new PooledDeathState(this));
    
    // Optional: Only add patrol if enabled in data
    if (this.Data.canPatrol)
    {
        stateMachine.AddState(MonsterStateType.Patrol, new PatrolState(this));
    }

    // Start in Idle state
    stateMachine.SwitchState(MonsterStateType.Idle);
    }

    public void Initialize(Necromancer summoner)
    {
        _summoner = summoner;
        ResetSkeleton();
    }
    public void PoolInitialize()
    {
        InitializeStates(); // Calls the protected method internally
        stateMachine.SwitchState(MonsterStateType.Idle);
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

    public void TriggerDeath()
    {
        // 1. Notify Necromancer first
        OnDeath?.Invoke(this);

        // 2. Switch to death state (this will handle visuals/cleanup)
        stateMachine.SwitchState(MonsterStateType.Death);

        // 3. Clear the event AFTER invoking it
        OnDeath = null;
    }

    private void OnDisable()
    {
        // Safety check - if we're being disabled without proper death sequence
        if (stateMachine.CurrentStateType != MonsterStateType.Death)
        {
            TriggerDeath(); // Force death cleanup
        }
    }
}