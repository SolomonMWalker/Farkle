using System;
using System.Collections.Generic;

public class GameStateManager
{
    private GameState gameState = GameState.Instantiated;
    private Dictionary<GameState, List<Action>> OnStateEnterActions = [], OnStateExitActions = [];
    private SelectDiceSubstate selectDiceSubstate = SelectDiceSubstate.SelectingDice;
    private readonly GameState[] stateProgression = [
        GameState.PreRoll,
        GameState.FindRollPosition,
        GameState.FindRollStrength,
        GameState.Rolling,
        GameState.SelectDice,
        GameState.ExitDiceZoomAnimation
    ];

    public GameStateManager()
    {
        foreach (GameState gs in Enum.GetValues(typeof(GameState)))
        {
            OnStateEnterActions.Add(gs, []);
            OnStateExitActions.Add(gs, []);
        }        
    }

    private void ChangeState(GameState state)
    {
        RunOnStateExitActions(gameState);
        gameState = state;
        RunOnStateEnterActions(state);
    }

    public GameState StartGame()
    {
        ClearFarkle();
        ChangeState(GameState.PreRoll);
        return gameState;
    }

    public GameState ProgressState()
    {
        
        var stateIndexInProgression = Array.IndexOf(stateProgression, gameState);
        ChangeState(stateProgression[(stateIndexInProgression + 1) % stateProgression.Length]);
        return gameState;
    }

    public GameState GameState => gameState;

    public SelectDiceSubstate Farkle()
    {
        selectDiceSubstate = SelectDiceSubstate.Farkled;
        return selectDiceSubstate;
    }

    public SelectDiceSubstate ClearFarkle()
    {
        selectDiceSubstate = SelectDiceSubstate.SelectingDice;
        return selectDiceSubstate;
    }

    public GameState GameOver()
    {
        gameState = GameState.GameOver;
        return gameState;
    }

    public SelectDiceSubstate GetSelectDiceSubstate => selectDiceSubstate;

    public void AddOnStateEnterOrExitAction(bool enter, GameState gameState, Action action)
    {
        var actionDict = enter ? OnStateEnterActions : OnStateExitActions;

        actionDict[gameState].Add(action);
    }

    public void AddOnStateEnterOrExitAction(bool enter, GameState gameState, IEnumerable<Action> actions)
    {
        foreach (Action action in actions)
        {
            AddOnStateEnterOrExitAction(enter, gameState, action);
        }
    }

    public void RunOnStateEnterActions(GameState gameState)
    {
        if (OnStateEnterActions.TryGetValue(gameState, out List<Action> actionList))
        {
            foreach (Action action in actionList)
            {
                action();
            }
        }
    }

    public void RunOnStateExitActions(GameState gameState)
    {
        if (OnStateExitActions.TryGetValue(gameState, out List<Action> actionList))
        {
            foreach (Action action in actionList)
            {
                action();
            }
        }
    }
}

public enum GameState{
    Instantiated,
    GameOver,
    PreRoll,
    FindRollPosition,
    FindRollStrength,
    Rolling,
    SelectDice,
    ExitDiceZoomAnimation,
}

public enum SelectDiceSubstate
{
    Farkled,
    SelectingDice,
}