using UnityEngine;
using System.Collections.Generic;

public class SlimeJumpState : IMonsterState
{
    private readonly Slime _slime;
    private float _jumpTimer;
    private bool _isJumping;
    private Vector2 _lastTargetPosition;
    private float _pathfindingCooldown;
    private const float PATHFINDING_COOLDOWN = 0.5f;
    private const float MIN_JUMP_DISTANCE = 1.5f;
    private const float MAX_JUMP_DISTANCE = 4f;

    public SlimeJumpState(Slime slime)
    {
        _slime = slime;
    }

    public void Enter()
    {
        _jumpTimer = 0f;
        _isJumping = false;
        _pathfindingCooldown = 0f;
    }

    public void Update()
    {
        _jumpTimer += Time.deltaTime;
        _pathfindingCooldown -= Time.deltaTime;

        // Check if we need to switch to attack state
        if (Vector2.Distance(_slime.transform.position, _slime.Target.position) <= _slime.Data.attackRange)
        {
            _slime.stateMachine.SwitchState(MonsterStateType.Attack);
            return;
        }

        // Check if we need to retreat
        if (_slime.Rb.velocity.y < -2f && Vector2.Distance(_slime.transform.position, _slime.Target.position) > MAX_JUMP_DISTANCE)
        {
            _slime.stateMachine.SwitchState(MonsterStateType.Retreat);
            return;
        }

        // Only try to find new path if enough time has passed or target moved significantly
        if (_pathfindingCooldown <= 0f || Vector2.Distance(_lastTargetPosition, _slime.Target.position) > 1f)
        {
            _lastTargetPosition = _slime.Target.position;
            _pathfindingCooldown = PATHFINDING_COOLDOWN;

            // Find best jump position
            Vector2 targetDirection = (_slime.Target.position - _slime.transform.position).normalized;
            float distance = Vector2.Distance(_slime.transform.position, _slime.Target.position);

            // Adjust jump force based on distance
            float jumpForceMultiplier = Mathf.Clamp(distance / MAX_JUMP_DISTANCE, 0.5f, 1f);

            if (_jumpTimer >= _slime._slimeData.jumpInterval && !_isJumping)
            {
                _isJumping = true;
                _slime.Animator.SetTrigger("Jump");
                
                // Add some randomness to jump direction
                Vector2 jumpDirection = targetDirection;
                jumpDirection += Random.insideUnitCircle * 0.2f;
                jumpDirection.Normalize();
                
                // Apply force with adjusted strength
                _slime.Rb.AddForce(jumpDirection * _slime._slimeData.jumpForce * jumpForceMultiplier, ForceMode2D.Impulse);
            }
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