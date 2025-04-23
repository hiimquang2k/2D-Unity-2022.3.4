using UnityEngine;

public class AttackState : IMonsterState
{
    protected Monster _monster;
    protected float _cooldownTimer;
    protected bool _attackExecuted;
    
    // Animation hashes
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");

    public AttackState(Monster monster)
    {
        _monster = monster;
    }

    public virtual void Enter()
    {
        _cooldownTimer = GetRandomCooldown();
        _attackExecuted = false;
        
        // Trigger attack animation
        int attackVariant = SelectAttackVariant();
        _monster.Animator.SetInteger(AttackIndex, attackVariant);
        _monster.Animator.SetTrigger(AttackTrigger);
        
        // Face target if exists
        if (_monster.Target != null)
        {
            Vector2 direction = (_monster.Target.position - _monster.transform.position).normalized;
            _monster.directionManager.UpdateDirection(direction.x, direction.y);
        }
    }

    public virtual void Update()
    {
        if (!_attackExecuted && IsAttackFrame())
        {
            ExecuteAttack();
            _attackExecuted = true;
        }

        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer <= 0)
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Chase);
        }
    }

    public virtual void Exit()
    {
        _monster.Animator.ResetTrigger(AttackTrigger);
    }

    protected virtual float GetRandomCooldown()
    {
        return _monster.Data.attackCooldown * 
               Random.Range(1f - _monster.Data.attackVariance, 
                           1f + _monster.Data.attackVariance);
    }

    protected virtual int SelectAttackVariant()
    {
        // Default: Single attack animation (override for multi-attack monsters)
        return 1;
    }

    protected virtual bool IsAttackFrame()
    {
        // Check animation progress or use Animation Events
        AnimatorStateInfo stateInfo = _monster.Animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime >= 0.5f && stateInfo.IsTag("Attack");
    }

    protected virtual void ExecuteAttack()
    {
        Vector2 attackOrigin = _monster.transform.position;
        Vector2 attackDirection = _monster.Target != null 
            ? (_monster.Target.position - _monster.transform.position).normalized
            : _monster.transform.right;

        // Detect targets in attack range
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackOrigin + attackDirection * _monster.Data.attackRange * 0.5f,
            _monster.Data.attackRange
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<DamageSystem>(out var damageSystem))
            {
                damageSystem.ApplyDamage(
                    _monster.Data.attackDamage,
                    GetDamageType(),
                    attackOrigin
                );
            }
        }

        SpawnAttackEffects(attackOrigin, attackDirection);
    }

    protected virtual DamageType GetDamageType()
    {
        // Default physical damage (override for elemental attacks)
        return DamageType.Physical;
    }

    protected virtual void SpawnAttackEffects(Vector2 position, Vector2 direction)
    {
        if (_monster.Data.attackEffect != null)
        {
            Object.Instantiate(
                _monster.Data.attackEffect,
                position,
                Quaternion.LookRotation(Vector3.forward, direction)
            );
        }
    }
}