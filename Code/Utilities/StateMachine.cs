using System;
using System.Collections.Generic;
using System.Linq;

public class StateMachine
{
    public readonly string StateMachineName;
    private State _currentState;
    private HashSet<State> _states = [];

    public StateMachine(string stateMachineName, string initialStateName = "Initial")
    {
        StateMachineName = stateMachineName;
        AddState(initialStateName);
        TryGetStateByName(initialStateName, out var initialState);
        _currentState = initialState;
    }

    public State GetCurrentState() => _currentState;

    public bool TryChangeState(string nextStateName)
    {
        if (!TryGetStateByName(nextStateName, out var nextState)) { return false; }
        _currentState.ExitActions.ForEach(a => a());
        _currentState = nextState;
        _currentState.EnterActions.ForEach(a => a());
        return true;
    }

    public void AddStates(IEnumerable<string> strings)
    {
        foreach (string s in strings)
        {
            AddState(s);
        }
    }

    public void AddState(string name)
    {
        if (TryGetStateByName(name, out _)) { return; }
        _states.Add(new(name, [], []));
    }

    public bool TryCreateStateLinkage(string fromStateName, string toStateName)
    {
        if ( fromStateName.Equals(toStateName) ||
            !TryGetStateByName(fromStateName, out var fromState) ||
            !TryGetStateByName(toStateName, out var toState)
        ) { return false; }
        fromState.NextStates.Add(toState);
        return true;
    }

    public bool TryRemoveStateLinkage(string fromStateName, string toStateName)
    {
        if (!TryGetStateByName(fromStateName, out var fromState) || !TryGetStateByName(toStateName, out var toState)) { return false; }
        fromState.NextStates.Remove(toState);
        return true;
    }

    public void AddEnterStateAction(string stateName, Action action)
    {
        if (!TryGetStateByName(stateName, out var state)) { return; }
        state.EnterActions.Add(action);
    }

    public void AddExitStateAction(string stateName, Action action)
    {
        if (!TryGetStateByName(stateName, out var state)) { return; }
        state.ExitActions.Add(action);
    }

    public bool TryChangeStateSubState(string stateName, string newSubState)
    {
        if (!TryGetStateByName(stateName, out var state)) { return false; }
        return state.TrySetSubState(newSubState);        
    }

    public bool TryGetStateByName(string name, out State state)
    {
        state = _states.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?
            _states.First(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) : null;
        return state != null;
    }
}

public class State
{
    public string Name { get; set; }
    public List<State> NextStates { get; set; }
    public List<string> SubStates { get; set; }
    public List<Action> EnterActions { get; set; }
    public List<Action> ExitActions { get; set; }
    private int _currentSubState = -1;

    public State(string name, List<State> nextStates, List<string> subStates = null)
    {
        Name = name;
        NextStates = nextStates;
        EnterActions = [];
        ExitActions = [];

        if (subStates != null && subStates.Count > 0) { _currentSubState = 0; }
    }

    public bool TryGetSubstate(out string subState)
    {
        subState = "";
        if (SubStates is null || SubStates.Count == 0) { return false; }
        subState = SubStates[_currentSubState];
        return true;
    }

    public bool TrySetSubState(string subState)
    {
        if (SubStates is null || !SubStates.Contains(subState)) { return false; }
        _currentSubState = SubStates.IndexOf(subState);
        return true;
    }
}