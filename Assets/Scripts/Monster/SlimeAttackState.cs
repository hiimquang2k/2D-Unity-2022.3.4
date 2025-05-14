using UnityEngine;

public class SlimeAttackState : AttackState
{
    private readonly Slime _slime;
    
    public SlimeAttackState(Slime slime) : base(slime)
    {
        _slime = slime;
    }

    public override void Enter()
    {
        base.Enter(); // Call the base class Enter method

        // Trigger the acid attack animation
        _slime.Animator.SetTrigger("AcidAttack");
    }

    public override void ExecuteAttack()
    {
        if (_attackExecuted) return;
        _attackExecuted = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            _slime.transform.position,
            _slime.Data.attackRange * 1.5f
        );

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<DamageSystem>(out var damageSystem))
            {
                damageSystem.ApplyDamage(
                    _slime.Data.attackDamage,
                    DamageType.Water,
                    _slime.transform.position
                );
            }
        }
        
        SpawnAcidPool();
    }
    private void SpawnAcidPool()
    {
        if (_slime.Data.attackEffect != null)
        {
            var pool = Object.Instantiate(
                _slime.Data.attackEffect,
                _slime.transform.position,
                Quaternion.identity
            );
            Object.Destroy(pool, 3f); // Destroy the pool after 3 seconds
        }
    }
}