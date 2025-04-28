using UnityEngine;

public class PooledDeathState : IMonsterState
{
    private Skeleton _skeleton;
    private float _deathTimer;

    public PooledDeathState(Skeleton skeleton)
    {
        _skeleton = skeleton;
    }

    public void Enter()
    {
        _skeleton.Move(Vector2.zero);
        _skeleton.Animator.SetTrigger("Death");
        _skeleton.GetComponent<Collider2D>().enabled = false;
        _deathTimer = 1.5f;

        if (_skeleton.Data.deathSound != null)
            AudioSource.PlayClipAtPoint(_skeleton.Data.deathSound, _skeleton.transform.position);
    }

    public void Update()
    {
        _deathTimer -= Time.deltaTime;

        // Handle fade out
        var sr = _skeleton.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = sr.color;
            color.a = Mathf.Clamp01(_deathTimer / 0.5f);
            sr.color = color;
        }

        if (_deathTimer <= 0 && _skeleton.gameObject.activeSelf)
        {
            // Disable AFTER death sequence is complete
            _skeleton.gameObject.SetActive(false);
        }
    }

    public void Exit() { }
}