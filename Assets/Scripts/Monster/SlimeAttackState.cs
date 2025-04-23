public class SlimeAttackState : IMonsterState
{
    private readonly Slime _slime;
    private bool _attackExecuted;

    public SlimeAttackState(Slime slime)
    {
        _slime = slime;
    }

    public void Enter()
    {
        _slime.Animator.SetTrigger("Attack");
        _attackExecuted = false;
    }

    public void Update()
    {
        if (!_attackExecuted && IsAttackFrame())
        {
            ExecuteAcidSplash();
            _attackExecuted = true;
        }
    }

    private bool IsAttackFrame()
    {
        AnimatorStateInfo stateInfo = _slime.Animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= 0.5f;
    }

    private void ExecuteAcidSplash()
    {
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
            Object.Destroy(pool, 3f);
        }
    }

    public void Exit() { }
}