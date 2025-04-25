using UnityEngine;
using System;

public class Necromancer : Monster
{
    private int _currentSkeletons;
    private float _lastSummonTime;

    protected override void InitializeStates()
    {
        stateMachine.AddState(MonsterStateType.Attack, new AttackState(this));
        stateMachine.AddState(MonsterStateType.Chase, new ChaseState(this));
        stateMachine.AddState(MonsterStateType.Summon, new SummonState(this));
        stateMachine.AddState(MonsterStateType.Idle, new IdleState(this));
        stateMachine.AddState(MonsterStateType.Patrol, new PatrolState(this));
        stateMachine.SwitchState(MonsterStateType.Idle);
    }

    protected override void Update()
    {
        base.Update();

        if (ShouldSummon())
        {
            stateMachine.SwitchState(MonsterStateType.Summon);
            Debug.Log("Attempting to summon skeleton"); // Debug line
        }
    }

    private bool ShouldSummon()
    {
        bool canSummon = _currentSkeletons < ((NecromancerData)Data).maxSkeletons &&
                        Time.time > _lastSummonTime + ((NecromancerData)Data).summonCooldown;

        Debug.Log($"Can summon: {canSummon} | Current: {_currentSkeletons} | Last summon: {Time.time - _lastSummonTime}s ago");
        return canSummon;
    }

    // Called via Animation Event
    public void SummonSkeleton()
    {
        if (_currentSkeletons >= ((NecromancerData)Data).maxSkeletons)
        {
            Debug.LogWarning("Max skeletons reached!");
            return;
        }

        Skeleton skeleton = SkeletonPool.Instance.GetFromPool(
            (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * 2f,
            Quaternion.identity);

        if (skeleton == null)
        {
            Debug.LogError("Failed to get skeleton from pool!");
            return;
        }

        skeleton.Initialize(this);

        // Verify event subscription
        Debug.Log($"Subscribing to skeleton death (Current: {_currentSkeletons})");
        skeleton.OnDeath += OnSkeletonDeath;

        _currentSkeletons++;
        _lastSummonTime = Time.time;
    }

    private void OnSkeletonDeath(Skeleton skeleton)
    {
        if (skeleton == null) return;

        _currentSkeletons--;
        Debug.Log($"Skeleton death confirmed! Current: {_currentSkeletons}");

        // Cleanup
        skeleton.OnDeath -= OnSkeletonDeath;
    }
}