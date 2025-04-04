using UnityEngine;

public class AttackState : MonsterState
{
    private float attackCooldown;

    public AttackState(IMonsterBehavior monster) : base(monster) 
    {
        attackCooldown = monster.MonsterData.attackCooldown;
    }

    public override void Enter()
    {
        monster.Animator.SetBool("isAttacking", true);
    }

    public override void Update()
    {
        if (monster.Target == null || !CanSeeTarget())
        {
            monster.SwitchState(MonsterStateType.Idle);
            return;
        }

        // Attack animation callback should trigger this
        if (monster.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            monster.Animator.SetBool("isAttacking", false);
            monster.SwitchState(MonsterStateType.Chase);
        }
    }

    public override void Exit()
    {
        monster.Animator.SetBool("isAttacking", false);
    }

    private bool CanSeeTarget()
    {
        if (monster.Target == null) return false;
        return Vector2.Distance(monster.transform.position, monster.Target.position) <= monster.MonsterData.aggroRange;
    }

    public override void Attack()
    {
        // Attack animation trigger
        monster.Animator.SetTrigger("attack");
    }
}