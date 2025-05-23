using System;

public class GameStateManager{

    private GameState gameState = GameState.PlayerSetup;
    private SelectDiceSubstate selectDiceSubstate = SelectDiceSubstate.SelectingDice;
    private readonly GameState[] stateProgression = [
        GameState.PreRoll,
        GameState.RollReady,
        GameState.Rolling,
        GameState.SelectDice,
        GameState.ExitDiceZoomAnimation
    ];

    public GameState StartGame()
    {
        gameState = GameState.PreRoll;
        return gameState;
    }

    public GameState StartPlayerSetup()
    {
        gameState = GameState.PlayerSetup;
        return gameState;
    } 

    public GameState ProgressState()
    {
        var stateIndexInProgression = Array.IndexOf(stateProgression, gameState);
        gameState = stateProgression[(stateIndexInProgression + 1) % stateProgression.Length];
        return gameState;
    }

    public GameState GetState => gameState;

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

    public GameState GameOver() => gameState = GameState.GameOver;

    public SelectDiceSubstate GetSelectDiceSubstate => selectDiceSubstate;
}

public enum GameState{
    PlayerSetup,
    GameOver,
    PreRoll,
    RollReady,
    Rolling,
    SelectDice,
    ExitDiceZoomAnimation,
}

public enum SelectDiceSubstate
{
    Farkled,
    SelectingDice,
}