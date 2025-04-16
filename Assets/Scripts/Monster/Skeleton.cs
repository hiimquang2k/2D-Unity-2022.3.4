// Skeleton.cs
public class Skeleton : Monster
{
    void Start()
    {
        // Create chase state with custom interval
        var chaseState = new ChaseState(this, 0.2f);
        
        stateMachine.AddState(MonsterStateType.Chase, chaseState);
        stateMachine.SwitchState(MonsterStateType.Chase);
    }
}
