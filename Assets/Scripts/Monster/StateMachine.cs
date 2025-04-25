using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private readonly Dictionary<MonsterStateType, IMonsterState> _states = new();
    private IMonsterState _currentState;
    private IMonsterState _previousState;

    // Public properties
    public MonsterStateType? CurrentStateType => GetStateType(_currentState);
    public MonsterStateType? PreviousStateType => GetStateType(_previousState);
    public IMonsterState CurrentState => _currentState;
    public IMonsterState PreviousState => _previousState;

    /// <summary> Adds a new state or replaces existing one </summary>
    /// <returns>True if state was added, false if replaced</returns>
    public bool AddState(MonsterStateType type, IMonsterState state)
    {
        if (state == null)
        {
            Debug.LogError($"Cannot add null state for {type}");
            return false;
        }

        bool exists = _states.ContainsKey(type);
        _states[type] = state;
        return !exists;
    }

    /// <summary> Removes a state if it exists and isn't current </summary>
    /// <returns>True if state was removed</returns>
    public bool RemoveState(MonsterStateType type)
    {
        if (_currentState != null && _states.TryGetValue(type, out var state) && state == _currentState)
        {
            Debug.LogError($"Cannot remove current state: {type}");
            return false;
        }
        return _states.Remove(type);
    }

    /// <summary> Switches to a new state </summary>
    /// <returns>True if transition succeeded</returns>
    public bool SwitchState(MonsterStateType newState)
    {
        if (!_states.TryGetValue(newState, out var nextState))
        {
            Debug.LogError($"State {newState} not found!");
            return false;
        }

        if (_currentState == nextState)
        {
            Debug.LogWarning($"Already in state {newState}");
            return false;
        }

        _previousState = _currentState;
        _currentState?.Exit();
        _currentState = nextState;
        _currentState?.Enter();
        return true;
    }

    /// <summary> Returns to previous state if possible </summary>
    /// <returns>True if reverted to previous state</returns>
    public bool SwitchToPrevious()
    {
        if (_previousState == null)
        {
            Debug.LogWarning("No previous state exists");
            return false;
        }

        var previousType = GetStateType(_previousState);
        if (!previousType.HasValue)
        {
            Debug.LogError("Previous state not registered in state machine");
            return false;
        }

        return SwitchState(previousType.Value);
    }

    /// <summary> Gets the type of a state instance </summary>
    public MonsterStateType? GetStateType(IMonsterState state)
    {
        if (state == null) return null;
        
        foreach (var pair in _states)
        {
            if (pair.Value == state) return pair.Key;
        }
        return null;
    }

    /// <summary> Checks if a state exists </summary>
    public bool HasState(MonsterStateType type) => _states.ContainsKey(type);

    /// <summary> Updates the current state </summary>
    public void Update() => _currentState?.Update();

    /// <summary> Clears all states and resets the machine </summary>
    public void Reset()
    {
        _currentState?.Exit();
        _currentState = null;
        _previousState = null;
        _states.Clear();
    }
}