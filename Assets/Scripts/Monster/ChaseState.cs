using UnityEngine;
public class ChaseState : IMonsterState
{
    // Public property with private backing field
    public float UpdateInterval { get; set; }
    
    private Monster _monster;
    private float _lastUpdateTime;

    public ChaseState(Monster monster, float updateInterval = 0.5f)
    {
        _monster = monster;
        UpdateInterval = updateInterval; // Set through constructor
    }

    public void Enter()
    {
        _monster.Animator.SetBool("IsChasing", true);
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        _monster.Target = Player.transform;
    }
    
    public void Update()
    {
        if (_monster.Target == null) // If target lost
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Idle);
            return;
        }

        // Calculate distance to target
        float distance = Vector2.Distance(_monster.transform.position, _monster.Target.position);

        // Transition to attack if in range
        if (distance <= _monster.Data.attackRange && _monster.Data.canAttack)
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Attack);
            return;
        }

        // Continue chasing logic
        if (Time.time - _lastUpdateTime > UpdateInterval)
        {
            Vector2 direction = (_monster.Target.position - _monster.transform.position).normalized;
            _monster.Move(direction * _monster.Data.chaseSpeed);
            _lastUpdateTime = Time.time;
        }
    }

    public void Exit()
    {
        _monster.Move(Vector2.zero);
        _monster.Animator.SetBool("IsChasing", false);
    }
}
