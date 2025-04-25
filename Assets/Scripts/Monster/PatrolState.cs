using UnityEngine;

public class PatrolState : IMonsterState
{
    private Monster _monster;
    private Vector2 _patrolCenter;
    private Vector2 _currentTarget;
    
    public PatrolState(Monster monster)
    {
        _monster = monster;
        _patrolCenter = _monster.transform.position;
    }

    public void Enter()
    {
        _monster.Animator.SetBool("IsWalking", true);
        SetNewPatrolTarget();
    }

    public void Update()
    {
        // Check if reached target
        if (Vector2.Distance(_monster.transform.position, _currentTarget) < 0.1f)
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Idle);
            return;
        }

        // Move toward target
        Vector2 direction = (_currentTarget - (Vector2)_monster.transform.position).normalized;
        _monster.Move(direction * _monster.Data.moveSpeed);

        // Player detection
        if (_monster.Target != null && 
            Vector2.Distance(_monster.transform.position, _monster.Target.position) < _monster.Data.aggroRange)
        {
            _monster.stateMachine.SwitchState(_monster.Data.canChase ? 
                MonsterStateType.Chase : 
                MonsterStateType.Attack);
        }
    }

    public void Exit()
    {
        _monster.Animator.SetBool("IsWalking", false);
    }

    private void SetNewPatrolTarget()
    {
        _currentTarget = _patrolCenter + Random.insideUnitCircle * _monster.Data.patrolRadius;
    }
}