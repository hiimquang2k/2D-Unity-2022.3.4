using UnityEngine;

public class SlimeJumpState : IMonsterState
{
    private readonly Slime _slime;
    private float _jumpTimer;
    private bool _isJumping;

    public SlimeJumpState(Slime slime)
    {
        _slime = slime;
    }

    public void Enter()
    {
        _jumpTimer = 0f;
        _isJumping = false;
    }

    public void Update()
    {
        _jumpTimer += Time.deltaTime;

        if (_jumpTimer >= _slime._slimeData.jumpInterval && !_isJumping)
        {
            _isJumping = true;
            _slime.Animator.SetTrigger("Jump");
            
            Vector2 jumpDirection = (_slime.Target.position - _slime.transform.position).normalized;
            _slime.Rb.AddForce(jumpDirection * _slime._slimeData.jumpForce, ForceMode2D.Impulse);
        }

        if (_isJumping && _slime.Rb.velocity.y < 0.1f)
        {
            _isJumping = false;
            _jumpTimer = 0f;
        }
    }

    public void Exit()
    {
        _isJumping = false;
    }
}