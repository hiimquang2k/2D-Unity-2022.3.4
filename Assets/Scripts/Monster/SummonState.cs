
public class SummonState : IMonsterState
{
    private Necromancer _necromancer;

    public SummonState(Necromancer necromancer) => _necromancer = necromancer;

    public void Enter()
    {
        _necromancer.Animator.SetTrigger("Summon");
        _necromancer.SummonSkeleton();
    }

    public void Update() => _necromancer.stateMachine.SwitchState(MonsterStateType.Chase);
    public void Exit() {}
}
