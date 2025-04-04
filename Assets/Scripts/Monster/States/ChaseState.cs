using UnityEngine;

public class ChaseState : MonsterState
{
    private float chaseSpeed;

    public ChaseState(Monster monster) : base(monster)
    {
        chaseSpeed = monster.monsterData.chaseSpeed;
    }

    public override void Update()
    {
        if (monster.target == null)
        {
            monster.SwitchState(MonsterStateType.Idle);
            return;
        }

        Vector2 direction = (monster.target.position - monster.transform.position).normalized;
        monster.rb.velocity = direction * chaseSpeed;

        // Check if close enough to attack
        float distance = Vector2.Distance(monster.transform.position, monster.target.position);
        if (distance <= monster.monsterData.attackRange)
        {
            monster.SwitchState(MonsterStateType.Attack);
        }
    }

    public override void Enter()
    {
        monster.animator.SetBool("isMoving", true);
    }

    public override void Exit()
    {
        monster.rb.velocity = Vector2.zero;
        monster.animator.SetBool("isMoving", false);
    }
}
