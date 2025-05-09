using UnityEngine;

public class SlimeRetreatState : IMonsterState
{
    private readonly Slime _slime;
    private float _retreatTimer;
    private Vector2 _retreatDirection;

    public SlimeRetreatState(Slime slime)
    {
        _slime = slime;
    }

    public void Enter()
    {
        _retreatTimer = 0f;
        _retreatDirection = Vector2.zero;
    }

    public void Update()
    {
        _retreatTimer += Time.deltaTime;

        if (_retreatTimer < _slime._slimeData.retreatDuration)
        {
            if (_retreatDirection == Vector2.zero)
            {
                _retreatDirection = (_slime.Target.position - _slime.transform.position).normalized;
            }

            Vector2 retreatDirection = -_retreatDirection;
            _slime.Move(retreatDirection * _slime._slimeData.retreatSpeed);
        }
        else
        {
            _slime.stateMachine.SwitchState(MonsterStateType.Idle);
        }
    }

    public void Exit()
    {
        _retreatDirection = Vector2.zero;
    }
}