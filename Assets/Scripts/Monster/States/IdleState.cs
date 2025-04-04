using UnityEngine;

public class IdleState : MonsterState
{
    public IdleState(IMonsterBehavior monster) : base(monster) { }

    public override void Update()
    {
        if (monster.Target != null)
        {
            monster.SwitchState(MonsterStateType.Chase);
        }
    }

    public override void Enter()
    {
        monster.Animator.SetBool("isMoving", false);
        monster.Animator.SetBool("isAttacking", false);
    }
}