using UnityEngine;

public class PatrolState : IMonsterState
{
    private Monster _monster;
    private Vector2 _patrolCenter;
    private Vector2 _currentTarget;
    
    public PatrolState(Monster monster) 
    {
        _monster = monster;
        _patrolCenter = GetValidPatrolCenter();
    }

    public void Enter()
    {
        _monster.Animator.SetBool("IsWalking", true);
        SetNewPatrolTarget();
    }
    private Vector2 GetValidPatrolCenter() 
    {
        // If current position is invalid, search downward for ground
        if (!Physics2D.Raycast(_monster.transform.position, Vector2.down, _monster.Data.groundCheckDistance, _monster.groundLayer))
        {
            RaycastHit2D hit = Physics2D.Raycast(_monster.transform.position, Vector2.down, 10f, _monster.groundLayer);
            if (hit.collider != null) return hit.point;
        }
        return _monster.transform.position;
    }
    private void SetNewPatrolTarget() 
    {
        const int maxAttempts = 10;
        bool validPositionFound = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate horizontal target
            float xOffset = Random.Range(-_monster.Data.patrolRadius, _monster.Data.patrolRadius);
            Vector2 candidateTarget = _patrolCenter + new Vector2(xOffset, 0);

            // Check if the target is on ground AND has platform ahead
            bool isGrounded = Physics2D.Raycast(
                candidateTarget, 
                Vector2.down, 
                _monster.Data.groundCheckDistance, 
                _monster.groundLayer
            );

            bool hasPlatformAhead = Physics2D.Raycast(
                candidateTarget + new Vector2(_monster.directionManager.IsFacingRight ? 0.5f : -0.5f, 0), 
                Vector2.down, 
                _monster.Data.groundCheckDistance, 
                _monster.groundLayer
            );

            if (isGrounded && hasPlatformAhead)
            {
                _currentTarget = candidateTarget;
                validPositionFound = true;
                break;
            }
        }

        // Fallback to patrol center if no valid target
        if (!validPositionFound)
        {
            _currentTarget = _patrolCenter;
            Debug.LogWarning("No valid patrol target found. Using center.");
        }
    }

    public void Update() 
    {
        // Calculate horizontal distance to target
        float horizontalDistance = Mathf.Abs(_monster.transform.position.x - _currentTarget.x);
        
        // Switch to Idle if close to target
        if (horizontalDistance < 0.5f)
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Idle);
            return;
        }

        // Calculate movement direction
        Vector2 direction = (_currentTarget - (Vector2)_monster.transform.position).normalized;
        _monster.Move(new Vector2(direction.x * _monster.Data.moveSpeed, 0));

        // Update facing direction
        _monster.directionManager.UpdateDirection(direction.x, 0);

        // Check for cliffs/walls
        if (!_monster.IsGrounded() || _monster.HasObstacleAhead())
        {
            _monster.stateMachine.SwitchState(MonsterStateType.Idle);
        }
    }

    public void Exit()
    {
        _monster.Animator.SetBool("IsWalking", false);
    }
}