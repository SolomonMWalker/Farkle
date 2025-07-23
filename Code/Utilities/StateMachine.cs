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
        _currentState = GetStateByName(initialStateName);
    }

    public string GetCurrentState() => _currentState.Name;

    public bool TryChangeState(string nextStateName)
    {
        if (!_currentState.NextStates.Select(s => s.Name).Contains(nextStateName)) { return false; }
        _currentState.ExitActions.ForEach(a => a());
        _currentState = GetStateByName(nextStateName);
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
        if (GetStateByName(name) != null) { return; }
        _states.Add(new(name, [], [], [], []));
    }

    public bool TryCreateStateLinkage(string fromStateName, string toStateName)
    {
        var fromState = GetStateByName(fromStateName);
        var toState = GetStateByName(toStateName);

        if (fromState == null || toState == null) { return false; }

        fromState.NextStates.Add(toState);
        return true;
    }

    public void AddEnterStateAction(string stateName, Action action)
    {
        var state = GetStateByName(stateName);
        if (state == null) { return; }
        state.EnterActions.Add(action);
    }

    public void AddExitStateAction(string stateName, Action action)
    {
        var state = GetStateByName(stateName);
        if (state == null) { return; }
        state.ExitActions.Add(action);
    }

    private State GetStateByName(string name)
    {
        return _states.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?
            _states.First(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) : null;
    }
}

public class State
{
    public string Name { get; set; }
    public List<State> NextStates { get; set; }
    public List<State> SubStates { get; set; }
    public List<Action> EnterActions { get; set; }
    public List<Action> ExitActions { get; set; }
    private StateMachine SubStateMachine { get; set; }

    public State(string name, List<State> nextStates, List<string> subStates, List<Action> enterActions, List<Action> exitActions)
    {
        Name = name;
        NextStates = nextStates;
        EnterActions = enterActions;
        ExitActions = exitActions;

        if (subStates != null && subStates.Count > 0)
        {
            SubStateMachine = new StateMachine(name + "SubStateMachine", subStates[0]);
            foreach (string subState in subStates)
            {
                SubStateMachine.AddState(subState);
            }
            foreach (string subState in subStates)
            {
                var otherSubStates = subStates.Where(s => s != subState);
                foreach (string otherSubState in otherSubStates)
                {
                    SubStateMachine.TryCreateStateLinkage(subState, otherSubState);
                }
            }
        }

        //create method to change substates
    }
}