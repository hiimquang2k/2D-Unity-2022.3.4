using UnityEngine;

public class Necromancer : Monster
{
    private int _currentSkeletons;
    private float _lastSummonTime;

    protected override void InitializeStates()
    {
        stateMachine.AddState(MonsterStateType.Attack, new AttackState(this));
        stateMachine.AddState(MonsterStateType.Chase, new ChaseState(this));
        stateMachine.AddState(MonsterStateType.Summon, new SummonState(this));
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
        NecromancerData necroData = (NecromancerData)Data;

        if (necroData.skeletonPrefab == null)
        {
            Debug.LogError("No skeleton prefab assigned!");
            return;
        }

        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * necroData.summonRadius;
        Instantiate(necroData.skeletonPrefab, spawnPos, Quaternion.identity);

        _currentSkeletons++;
        _lastSummonTime = Time.time;

        Debug.Log($"Summoned skeleton {_currentSkeletons}/{necroData.maxSkeletons}");
    }
}