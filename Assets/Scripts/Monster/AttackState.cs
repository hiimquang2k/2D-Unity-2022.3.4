using UnityEngine;

public class AttackState : IMonsterState
{
    protected Monster _monster;
    private bool _attackExecuted;
    
    // Animation hashes
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");

    public AttackState(Monster monster)
    {
        _monster = monster;
    }

    public virtual void Enter()
    {
        _attackExecuted = false;
        
        // Trigger attack animation
        int attackVariant = SelectAttackVariant();
        _monster.Animator.SetInteger(AttackIndex, attackVariant);
        _monster.Animator.SetTrigger(AttackTrigger);
        
        // Face target
        Vector2 direction;
        if (_monster.Target != null)
        {
            direction = (_monster.Target.position - _monster.transform.position).normalized;
            _monster.directionManager.SetInitialDirection(direction);
        }
        else
        {
            // Fallback to current facing direction if no target
            direction = _monster.directionManager.IsFacingRight ? Vector2.right : Vector2.left;
        }
        _monster.directionManager.SetInitialDirection(direction);
    }

    public virtual void Update() { } // Empty now

    public virtual void Exit()
    {
        _monster.Animator.ResetTrigger(AttackTrigger);
    }

    // Called by animation event
    public virtual void ExecuteAttack()
    {
        if (_attackExecuted) return;
        _attackExecuted = true;

        Vector2 attackOrigin = _monster.transform.position;
        Vector2 attackDirection = _monster.Target != null 
            ? (_monster.Target.position - _monster.transform.position).normalized
            : _monster.transform.right;

        // Platformer-friendly box overlap
        Vector2 attackSize = new Vector2(
            _monster.Data.attackRange, 
            _monster.Data.attackHeight // Add this to MonsterData
        );
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackOrigin + attackDirection * _monster.Data.attackRange * 0.5f,
            attackSize,
            0
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent<DamageSystem>(out var damageSystem))
            {
                damageSystem.ApplyDamage(
                    _monster.Data.attackDamage,
                    GetDamageType(),
                    attackOrigin
                );
            }
        }

        SpawnAttackEffects(attackOrigin, attackDirection);
        
        // Track cooldown in Monster
        _monster.lastAttackTime = Time.time;
        _monster.currentAttackCooldown = GetRandomCooldown();
    }

    // Called by animation event at the end
    public virtual void EndAttack()
    {
        _monster.stateMachine.SwitchState(MonsterStateType.Chase);
    }

    protected virtual float GetRandomCooldown()
    {
        return _monster.Data.attackCooldown * 
               Random.Range(1f - _monster.Data.attackVariance, 1f + _monster.Data.attackVariance);
    }

    protected virtual int SelectAttackVariant() => 1;

    protected virtual DamageType GetDamageType() => DamageType.Physical;

    protected virtual void SpawnAttackEffects(Vector2 position, Vector2 direction)
    {
        if (_monster.Data.attackEffect != null)
            Object.Instantiate(
                _monster.Data.attackEffect,
                position,
                Quaternion.LookRotation(Vector3.forward, direction)
            );
    }
}