using UnityEngine;
using System.Collections.Generic;

public class SlimeRetreatState : IMonsterState
{
    private readonly Slime _slime;
    private float _retreatTimer;
    private Vector2 _retreatDirection;
    private Vector2 _coverPosition;
    private bool _hasFoundCover;
    private const float COVER_SEARCH_RADIUS = 3f;
    private const float MIN_COVER_DISTANCE = 1.5f;

    public SlimeRetreatState(Slime slime)
    {
        _slime = slime;
    }

    public void Enter()
    {
        _retreatTimer = 0f;
        _retreatDirection = Vector2.zero;
        _hasFoundCover = false;
        _slime.Animator.SetTrigger("Retreat");
    }

    public void Update()
    {
        _retreatTimer += Time.deltaTime;

        // Find cover if we haven't already
        if (!_hasFoundCover)
        {
            FindCover();
        }

        if (_retreatTimer < _slime._slimeData.retreatDuration)
        {
            if (_hasFoundCover)
            {
                // Move towards cover
                Vector2 directionToCover = (_coverPosition - (Vector2)_slime.transform.position).normalized;
                _slime.Move(directionToCover * _slime._slimeData.retreatSpeed);
            }
            else
            {
                // Move away from target if no cover found
                Vector2 retreatDirection = -((Vector2)_slime.Target.position - (Vector2)_slime.transform.position).normalized;
                _slime.Move(retreatDirection * _slime._slimeData.retreatSpeed);
            }
        }
        else
        {
            // Switch back to jump state when retreat duration is over
            _slime.stateMachine.SwitchState(MonsterStateType.Jump);
        }
    }

    private void FindCover()
    {
        // Get all colliders within search radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_slime.transform.position, COVER_SEARCH_RADIUS);
        
        Vector2 bestCoverPosition = Vector2.zero;
        float bestCoverScore = float.MinValue;

        foreach (var collider in colliders)
        {
            // Skip if it's the slime itself or the target
            if (collider.gameObject == _slime.gameObject || collider.gameObject == _slime.Target.gameObject)
                continue;

            // Calculate cover position
            Vector2 coverPosition = (Vector2)collider.ClosestPoint(_slime.transform.position);
            
            // Calculate cover quality
            float distanceToTarget = Vector2.Distance(coverPosition, (Vector2)_slime.Target.position);
            float distanceToSlime = Vector2.Distance(coverPosition, (Vector2)_slime.transform.position);
            
            // Score based on distance to target (further is better) and distance to slime (closer is better)
            float score = distanceToTarget * 0.7f - distanceToSlime * 0.3f;
            
            if (score > bestCoverScore && distanceToSlime > MIN_COVER_DISTANCE)
            {
                bestCoverScore = score;
                bestCoverPosition = coverPosition;
            }
        }

        if (bestCoverScore > float.MinValue)
        {
            _hasFoundCover = true;
            _coverPosition = bestCoverPosition;
        }
    }

    public void Exit()
    {
        _retreatDirection = Vector2.zero;
        _hasFoundCover = false;
    }
}