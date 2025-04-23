// Skeleton.cs
public class Skeleton : Monster
{
    protected override void Start()
    {
        base.Start();
        // Create chase state with custom interval
        var chaseState = new ChaseState(this, 0.2f);
        stateMachine.AddState(MonsterStateType.Attack, new AttackState(this));
        stateMachine.AddState(MonsterStateType.Chase, chaseState);
        stateMachine.SwitchState(MonsterStateType.Chase);
    }
}
