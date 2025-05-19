using UnityEngine;
using System;

public class Necromancer : Monster
{
    public event Action<Necromancer> OnDeath;
    private int _currentSkeletons;
    private float _lastSummonTime;
    private HealthSystem _healthSystem;
    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath += TriggerDeath;
        }
    }

    protected override void InitializeStates()
    {
        stateMachine.AddState(MonsterStateType.Attack, new AttackState(this));
        stateMachine.AddState(MonsterStateType.Chase, new ChaseState(this));
        stateMachine.AddState(MonsterStateType.Summon, new SummonState(this));
        stateMachine.AddState(MonsterStateType.Idle, new IdleState(this));
        stateMachine.AddState(MonsterStateType.Patrol, new PatrolState(this));
        stateMachine.AddState(MonsterStateType.Death, new DeathState(this));
        stateMachine.SwitchState(MonsterStateType.Idle);
    }

    protected override void Update()
    {
        base.Update();

        if (ShouldSummon())
        {
            stateMachine.SwitchState(MonsterStateType.Summon);
        }
    }

    private bool ShouldSummon()
    {
        return _currentSkeletons < ((NecromancerData)Data).maxSkeletons &&
               Time.time > _lastSummonTime + ((NecromancerData)Data).summonCooldown;
    }

    // Called via Animation Event
    public void SummonSkeleton()
    {
        if (_currentSkeletons >= ((NecromancerData)Data).maxSkeletons)
        {
            Debug.LogWarning("Max skeletons reached!");
            return;
        }

        // Instantiate new skeleton directly
        // Get random X offset while maintaining Y position
        float randomXOffset = UnityEngine.Random.Range(-2f, 2f);
        Vector2 spawnPosition = new Vector2(
            transform.position.x + randomXOffset,
            transform.position.y
        );

        GameObject skeletonObj = GameObject.Instantiate(
            ((NecromancerData)Data).skeletonPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Skeleton skeleton = skeletonObj.GetComponent<Skeleton>();
        if (skeleton == null)
        {
            Debug.LogError("Skeleton prefab is missing Skeleton component!");
            return;
        }

        skeleton.Initialize(this);

        skeleton.OnDeath += OnSkeletonDeath;
        _currentSkeletons++;
        _lastSummonTime = Time.time;
    }

    private void OnSkeletonDeath(Skeleton skeleton)
    {
        if (skeleton == null) return;

        _currentSkeletons--;
        skeleton.OnDeath -= OnSkeletonDeath;
    }
    public void Initialize()
    {
        InitializeStates();
        stateMachine.SwitchState(MonsterStateType.Idle);
    }
    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath -= TriggerDeath;
        }
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