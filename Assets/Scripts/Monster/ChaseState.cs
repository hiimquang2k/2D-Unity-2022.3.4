using UnityEngine;

public class ChaseState : IMonsterState
{
    public float UpdateInterval { get; set; }
    private Monster _monster;
    
    public ChaseState(Monster monster, float updateInterval = 0.5f)
    {
        _monster = monster;
        UpdateInterval = updateInterval;
    }

    public void Enter()
    {
        _monster.Animator.SetBool("IsChasing", true);
        _monster.Target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    public void Update()
    {
        if (_monster.Target == null)
        {
            Debug.Log("Target not found");
            _monster.stateMachine.SwitchState(MonsterStateType.Idle);
            return;
        }

        // Horizontal distance check
        float horizontalDistance = Mathf.Abs(_monster.Target.position.x - _monster.transform.position.x);
        float verticalDifference = Mathf.Abs(_monster.Target.position.y - _monster.transform.position.y);

        // Check vertical proximity (ensure MonsterData has this field)
        if (verticalDifference > _monster.Data.maxVerticalAggro)
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Idle);
            Debug.Log("Vertical difference too large");
            return;
        }

        // Attack check
        if (horizontalDistance <= _monster.Data.attackRange && 
            _monster.Data.canAttack && 
            Time.time - _monster.lastAttackTime >= _monster.currentAttackCooldown)
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Attack);
            return;
        }

        if (!_monster.IsGrounded() || _monster.HasObstacleAhead())
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Idle);
            Debug.Log("Not grounded or obstacle ahead");
            return;
        }
        // Movement and direction update
        Vector2 direction = (_monster.Target.position - _monster.transform.position).normalized;
        _monster.Move(new Vector2(direction.x * _monster.Data.chaseSpeed, 0));
        
        // Use DirectionManager's existing method
        _monster.directionManager.UpdateDirection(direction.x, 0);
    }

    public void Exit()
    {
        _monster.Move(Vector2.zero);
        _monster.Animator.SetBool("IsChasing", false);
    }
}