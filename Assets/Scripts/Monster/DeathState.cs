using UnityEngine;

public class DeathState : IMonsterState
{
    private readonly Monster _monster;
    private float _deathTimer;
    private bool _deathSequenceStarted;

    public DeathState(Monster monster)
    {
        _monster = monster;
    }

    public void Enter()
    {
        // Stop movement
        _monster.Move(Vector2.zero);
        
        // Trigger death animation
        _monster.Animator.SetTrigger("Death");
        
        // Stop physics
        var rb = _monster.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            rb.isKinematic = true;
        }
        // Disable collisions
        var collider = _monster.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // Disable damage
        var damageSystem = _monster.GetComponent<DamageSystem>();
        if (damageSystem != null) damageSystem.SetDamageable(false);

        // Grant XP reward
        PerkSystem.Instance.AddXp(_monster.Data.xpReward);

        // Start death sequence
        _deathTimer = 2f; // Time until destruction
        _deathSequenceStarted = true;

        // Play effects
        if (_monster.Data.deathSound != null)
            AudioSource.PlayClipAtPoint(_monster.Data.deathSound, _monster.transform.position);
        
        if (_monster.Data.deathEffect != null)
            Object.Instantiate(_monster.Data.deathEffect, _monster.transform.position, Quaternion.identity);
    }

    // Required by IMonsterState interface
    public void Update()
    {
        if (!_deathSequenceStarted) return;
        
        _deathTimer -= Time.deltaTime;
        
        // Optional fade-out effect
        var spriteRenderer = _monster.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && _deathTimer < 1f)
        {
            Color color = spriteRenderer.color;
            color.a = _deathTimer; // Fade over last second
            spriteRenderer.color = color;
        }

        if (_deathTimer <= 0)
        {
            Object.Destroy(_monster.gameObject);
        }
    }

    // Required by IMonsterState interface
    public void Exit()
    {
        // Typically not used for death state
        // But must be present to satisfy the interface
    }
}