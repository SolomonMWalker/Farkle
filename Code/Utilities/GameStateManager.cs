using System;
using Godot;

public class GameStateManager{

    private GameState gameState = GameState.PreRoll;
    private readonly GameState[] stateProgression = {
        GameState.PreRoll,
        GameState.RollReady,
        GameState.Rolling,
        GameState.SelectDice,
        GameState.ExitDiceZoomAnimation
    };

    public GameState ProgressState()
    {
        var stateIndexInProgression = Array.IndexOf(stateProgression, gameState);
        gameState = stateProgression[(stateIndexInProgression + 1) % stateProgression.Length];
        return gameState;
    }

    public GameState GetState()
    {
        return gameState;
    }

}

public enum GameState{
    PreRoll,
    RollReady,
    Rolling,
    SelectDice,
    ExitDiceZoomAnimation,
}