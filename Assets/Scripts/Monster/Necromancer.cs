using UnityEngine;
public class Necromancer : Monster
{
    [Header("Summoning")]
    public GameObject skeletonPrefab;
    public int maxSkeletons = 3;
    public float summonCooldown = 10f;

    private int _currentSkeletons;
    private float _lastSummonTime;

    void Start()
    {
        var chaseState = new ChaseState(this);
        stateMachine.AddState(MonsterStateType.Chase, new ChaseState(this));
        stateMachine.AddState(MonsterStateType.Summon, new SummonState(this));
        stateMachine.SwitchState(EnumeratorStateType.Chase);
    }

    protected override void Update()
    {
        base.Update();
        
        if (ShouldSummon())
            stateMachine.SwitchState(MonsterStateType.Summon);
    }

    private bool ShouldSummon() => 
        _currentSkeletons < maxSkeletons && 
        Time.time > _lastSummonTime + summonCooldown;

    public void SummonSkeleton()
    {
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 2f;
        Instantiate(skeletonPrefab, spawnPos, Quaternion.identity);
        _currentSkeletons++;
        _lastSummonTime = Time.time;
    }
}
