using UnityEngine;
using System;

public class Skeleton : Monster
{
    public event Action<Skeleton> OnDeath;
    private Necromancer _summoner;
    private HealthSystem _healthSystem;

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
        stateMachine.AddState(MonsterStateType.Death, new DeathState(this));
        stateMachine.AddState(MonsterStateType.Idle, new IdleState(this));
        stateMachine.AddState(MonsterStateType.Chase, new ChaseState(this, Data.decisionInterval));
        stateMachine.AddState(MonsterStateType.Attack, new AttackState(this));
        
        if (Data.canPatrol)
        {
            stateMachine.AddState(MonsterStateType.Patrol, new PatrolState(this));
        }
        stateMachine.SwitchState(MonsterStateType.Idle);
    }

    public void Initialize(Necromancer summoner)
    {
        _summoner = summoner;
        ResetSkeleton();
    }

    public void ResetSkeleton()
    {
        GetComponent<Collider2D>().enabled = true;
        GetComponent<DamageSystem>().SetDamageable(true);
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color color = sr.color;
        color.a = 1f;
        sr.color = color;
    }

    private bool _isDying = false;

    public void TriggerDeath()
    {
        if (_isDying) return;
        _isDying = true;

        OnDeath?.Invoke(this);
        stateMachine.SwitchState(MonsterStateType.Death);
    }
}