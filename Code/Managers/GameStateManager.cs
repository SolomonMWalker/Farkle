using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GameStateManager
{
    public static readonly List<GameState> CameraZoomGameStates = [GameState.TableZoomAnimation, GameState.UserPerspectiveZoomAnimation];
    private bool _isPostCameraZoomStateSet = false;
    private GameState _postCameraZoomState;
    private StateMachine _gameStateMachine;

    public GameStateManager()
    {
        _gameStateMachine = new StateMachine("GameStateMachine", GameState.Instantiated.ToString());
        var gameStates = Enum.GetNames(typeof(GameState)).ToList();
        gameStates.Remove(GameState.Instantiated.ToString());
        foreach (var state in gameStates)
        {
            _gameStateMachine.AddState(state);
        }

        //Create select dice substates
        _gameStateMachine.TryGetStateByName(GameState.SelectDice.ToString(), out var selectDiceSubstate);
        selectDiceSubstate.SubStates = [SelectDiceSubstate.SelectingDice.ToString(), SelectDiceSubstate.FarkledGameOver.ToString(),
            SelectDiceSubstate.FarkledNotGameOver.ToString()];

        //Normal game states

        //Instantiated to Preroll
        TryCreateStateLinkage(GameState.Instantiated, GameState.PreRoll);

        //Preroll to FindRollPosition or TableZoomAnimation
        TryCreateStateLinkage(GameState.PreRoll, GameState.FindRollPosition);
        TryCreateStateLinkage(GameState.PreRoll, GameState.TableZoomAnimation);

        //FindRollPosition to FindRollStrength or Preroll
        TryCreateStateLinkage(GameState.FindRollPosition, GameState.FindRollStrength);
        TryCreateStateLinkage(GameState.FindRollPosition, GameState.PreRoll);

        //FindRollStrength to ThrowDice or Preroll
        TryCreateStateLinkage(GameState.FindRollStrength, GameState.ThrowDice);
        TryCreateStateLinkage(GameState.FindRollStrength, GameState.PreRoll);

        //ThrowDice to TableZoomAnimation
        TryCreateStateLinkage(GameState.ThrowDice, GameState.TableZoomAnimation);

        //Rolling to SelectDice
        TryCreateStateLinkage(GameState.Rolling, GameState.SelectDice);

        //TableZoomAnimation to SelectDice or SetTable or GameOver
        TryCreateStateLinkage(GameState.TableZoomAnimation, GameState.Rolling);
        TryCreateStateLinkage(GameState.TableZoomAnimation, GameState.SetTable);
        TryCreateStateLinkage(GameState.TableZoomAnimation, GameState.GameOver);

        //SetTable to UserPerspectiveZoomAnimation
        TryCreateStateLinkage(GameState.SetTable, GameState.UserPerspectiveZoomAnimation);

        //SelectDice to UserPerspectiveZoomAnimation or GameOver
        TryCreateStateLinkage(GameState.SelectDice, GameState.UserPerspectiveZoomAnimation);
        TryCreateStateLinkage(GameState.SelectDice, GameState.GameOver);

        //UserPerspectiveZoomAnimation to PreRoll
        TryCreateStateLinkage(GameState.UserPerspectiveZoomAnimation, GameState.PreRoll);

        //GameOver to UserPerspectiveZoomAnimation
        TryCreateStateLinkage(GameState.GameOver, GameState.UserPerspectiveZoomAnimation);
    }

    private bool TryCreateStateLinkage(GameState fromState, GameState toState)
    {
        if (!_gameStateMachine.TryCreateStateLinkage(fromState.ToString(), toState.ToString()))
        {
            GD.PrintErr($"Could not create state linkage from state {fromState} to state {toState}.");
            return false;
        }
        return true;
    }

    public GameState GetCurrentGameState() => Enum.Parse<GameState>(_gameStateMachine.GetCurrentState().Name);

    public bool TryProgressState(GameState nextState)
    {
        if (!_gameStateMachine.TryChangeState(nextState.ToString()))
        {
            GD.PrintErr($"Can't progress from state {GetCurrentGameState()} to state {nextState}.");
            return false;
        }
        return true;
    }

    public bool TryProgressStateWithCameraZoomBetween(GameState cameraZoomState, GameState nextState)
    {
        if (!CameraZoomGameStates.Contains(cameraZoomState) || CameraZoomGameStates.Contains(nextState))
        {
            GD.PrintErr($"Can't progress from state {GetCurrentGameState()} to state {nextState} with a camera zoom state of {cameraZoomState} in between.");
            return false;
        }
        _postCameraZoomState = nextState;
        _isPostCameraZoomStateSet = true;
        return TryProgressState(cameraZoomState);
    }

    public GameState GetAndErasePostCameraZoomState()
    {
        if (_isPostCameraZoomStateSet == false)
        {
            GD.PrintErr($"Trying to use post-camera-zoom game state, but its not set.");
        }
        _isPostCameraZoomStateSet = false;
        return _postCameraZoomState;
    }

    public void Farkle(bool isGameOver)
    {
        var farkleSubState = isGameOver ? SelectDiceSubstate.FarkledGameOver : SelectDiceSubstate.FarkledNotGameOver;
        _gameStateMachine.TryChangeStateSubState(GameState.SelectDice.ToString(), farkleSubState.ToString());
    }

    public void ClearFarkle()
    {
        _gameStateMachine.TryChangeStateSubState(GameState.SelectDice.ToString(), SelectDiceSubstate.SelectingDice.ToString());
    }

    public SelectDiceSubstate GetSelectDiceSubstate()
    {
        _gameStateMachine.TryGetStateByName(GameState.SelectDice.ToString(), out var selectDiceState);
        selectDiceState.TryGetSubstate(out var subState);
        return Enum.Parse<SelectDiceSubstate>(subState);
    }

    public void AddOnStateEnterOrExitAction(EnterOrExit enterOrExit, GameState gameState, Action action)
    {
        if (enterOrExit == EnterOrExit.Enter)
        {
            _gameStateMachine.AddEnterStateAction(gameState.ToString(), action);
        }
        else
        {
            _gameStateMachine.AddExitStateAction(gameState.ToString(), action);
        }
    }

    public void AddOnStateEnterOrExitAction(EnterOrExit enterOrExit, GameState gameState, IEnumerable<Action> actions)
    {
        foreach (Action action in actions)
        {
            AddOnStateEnterOrExitAction(enterOrExit, gameState, action);
        }
    }
}

public enum GameState
{
    Instantiated,
    GameOver,
    PreRoll,
    FindRollPosition,
    FindRollStrength,
    ThrowDice,
    Rolling,
    TableZoomAnimation,
    SelectDice,
    UserPerspectiveZoomAnimation,
    SetTable,
}

public enum SelectDiceSubstate
{
    FarkledGameOver,
    FarkledNotGameOver,
    SelectingDice,
}

public enum EnterOrExit
{
    Enter,
    Exit
}