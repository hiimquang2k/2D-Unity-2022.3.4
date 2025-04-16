using System.Collections.Generic;

public class StateMachine
{
    private Dictionary<MonsterStateType, IMonsterState> _states = new();
    private IMonsterState _currentState;

    public void AddState(MonsterStateType type, IMonsterState state) => _states[type] = state;
    public void RemoveState(MonsterStateType type) => _states.Remove(type);
    
    public void SwitchState(MonsterStateType newState)
    {
        _currentState?.Exit();
        _currentState = _states[newState];
        _currentState?.Enter();
    }

    public void Update() => _currentState?.Update();
}
