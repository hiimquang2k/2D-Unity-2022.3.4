using UnityEngine;

public class PatrolState : MonsterState
{
    private Vector2 patrolDirection;
    private float patrolSpeed;
    private float patrolTime;
    private float patrolTimer;

    public PatrolState(Monster monster) : base(monster)
    {
        patrolSpeed = monster.monsterData.patrolSpeed;
    }

    public override void Enter()
    {
        patrolTimer = 0f;
        patrolDirection = new Vector2(1, 0);
        monster.animator.SetBool("isMoving", true);
    }

    public override void Update()
    {
        if (monster.target != null)
        {
            monster.SwitchState(MonsterStateType.Chase);
            return;
        }

        patrolTimer += Time.deltaTime;
        if (patrolTimer >= monster.monsterData.patrolTime)
        {
            patrolDirection *= -1;
            patrolTimer = 0f;
        }

        monster.rb.velocity = patrolDirection * patrolSpeed;
    }

    public override void Exit()
    {
        monster.rb.velocity = Vector2.zero;
        monster.animator.SetBool("isMoving", false);
    }
}
