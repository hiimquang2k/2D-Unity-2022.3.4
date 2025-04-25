using UnityEngine;

public class IdleState : IMonsterState
{
    private Monster _monster;
    private float _idleTimer;
    private bool _isInitialIdle;

    public IdleState(Monster monster, bool isInitialIdle = false)
    {
        _monster = monster;
        _isInitialIdle = isInitialIdle;
    }

    public void Enter()
    {
        _monster.Animator.SetBool("IsIdle", true);
        _monster.Move(Vector2.zero);
        _idleTimer = _monster.Data.decisionInterval * Random.Range(1f, 3f); // Use existing AI interval
        
        if (_isInitialIdle)
        {
            Debug.Log($"{_monster.name} entering idle state");
        }
    }

    public void Update()
    {
        _idleTimer -= Time.deltaTime;

        // Check for player detection using MonsterData values
        if (_monster.Target != null && 
            Vector2.Distance(_monster.transform.position, _monster.Target.position) < _monster.Data.aggroRange)
        {
            if (_monster.Data.canChase)
            {
                _monster.stateMachine.SwitchState(MonsterStateType.Chase);
            }
            else if (_monster.Data.canAttack && 
                     Vector2.Distance(_monster.transform.position, _monster.Target.position) < _monster.Data.attackRange)
            {
                _monster.stateMachine.SwitchState(MonsterStateType.Attack);
            }
            return;
        }

        // Time-based transition to patrol if enabled
        if (_idleTimer <= 0 && _monster.Data.canPatrol)
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Patrol);
        }
    }

    public void Exit()
    {
        _monster.Animator.SetBool("IsIdle", false);
    }
}