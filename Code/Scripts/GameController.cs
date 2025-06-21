using System.Collections.Generic;
using Godot;

public partial class GameController : Node3D
{
    #region Properties

    private const string DebugMenuRelPath = "res://Scenes/Debug/DebugMenu.tscn";

    public DebugMenu DebugMenu { get; private set; }
    private CameraController cameraController;
    public GameStateManager GameStateManager { get; private set; }
    private ThrowLocationBall throwLocationBall;
    public PlayerManager PlayerManager { get; private set; }
    public DiceManager DiceManager { get; private set; }
    public ScoreManager ScoreManager { get; private set; }
    public RoundManager RoundManager { get; private set; }
    public StageManager StageManager { get; private set; }
    public UiManager UiManager { get; private set; }
    #endregion

    #region Initialization
    public override void _Ready()
    {
        base._Ready();
        Configuration.SetUpConfiguration();
        cameraController = this.GetChildByName<CameraController>("CameraController");
        throwLocationBall = FindChild("DiceTable").GetChildByName<ThrowLocationBall>("ThrowLocationBall");
        UiManager = new UiManager(this.GetChildByName<Control>("ControlParent"));
        GameStateManager = new GameStateManager();
        PlayerManager = new PlayerManager();
        DiceManager = new DiceManager( PlayerManager,
            this.GetChildByName<Node3D>("DiceHolder"),
            this.GetChildByName<Node3D>("OutOfPlayDiceLocation"),
            throwLocationBall.throwLocation);
        ScoreManager = new ScoreManager();
        RoundManager = new RoundManager();
        StageManager = new StageManager();

        if (Configuration.ConfigValues.IsDebug)
        {
            DebugMenu = GD.Load<PackedScene>(DebugMenuRelPath).Instantiate<DebugMenu>();
            this.GetChildByName<Control>("ControlParent").AddChild(DebugMenu);
            DebugMenu.Initialize(this);
        }

        SetupGameStateEnterExitActions();

        StartGame();
    }

    public void SetupGameStateEnterExitActions()
    {
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.PreRoll, [
            DiceManager.ReadyRollableDiceForThrow,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.FindRollPosition, [
            throwLocationBall.Animate,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.FindRollStrength, [
            throwLocationBall.StopAnimation, StartThrowForceBar,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: false, GameState.FindRollStrength, [
            UiManager.EndThrowForceBar, 
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.Rolling, [
            ThrowDice, throwLocationBall.ResetPositon,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.SelectDice, [
            DiceManager.AddDiceLeftOnTableToRollableDiceCollection, PreSelectDiceFarkleCheck,
        ]);

        if (Configuration.ConfigValues.IsDebug)
        {
            GameStateManager.AddOnStateEnterOrExitAction(enter: false, GameState.SelectDice, [
                DiceManager.EndOverride, DebugMenu.ResetDiceCollection,
            ]);
        }
    }

    #endregion

    #region MainGodotFunctions
    public override void _Process(double delta)
    {
        base._Process(delta);

        RunGame();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._Input(@event);
        HandleMouseInput(@event);
    }

    #endregion

    #region MainGameLogic
    public void RunGame()
    {
        var gameState = GameStateManager.GameState;

        switch (gameState)
        {
            case GameState.PreRoll:
                if (Input.IsActionJustPressed("Accept"))
                {
                    TryProgressState();
                }
                break;
            case GameState.FindRollPosition:
                if (Input.IsActionJustPressed("Accept"))
                {
                    TryProgressState();
                }
                break;
            case GameState.FindRollStrength:
                if (Input.IsActionJustPressed("Accept"))
                {
                    DiceManager.MultiplyVelocityByThrowForceValue(UiManager.GetThrowForceBarValue());
                    TryProgressState();
                }
                break;
            case GameState.Rolling:
                if (DiceManager.RollableDiceCollection.IsDoneRolling() && !cameraController.IsAnimationPlaying())
                {
                    TryProgressState();
                }
                break;
            case GameState.SelectDice:
                HandleSelectDiceState();
                break;
            case GameState.ExitDiceZoomAnimation:
                if (!cameraController.IsAnimationPlaying())
                {
                    TryProgressState();
                }
                break;
            case GameState.GameOver:
                HandleGameOverState();
                break;
        }
    }

    public void HandleSelectDiceState()
    {
        if (GameStateManager.GetSelectDiceSubstate is SelectDiceSubstate.Farkled && Input.IsActionJustPressed("Accept"))
        {
            ClearFarkle();
            DiceManager.ResetAllDice();
            cameraController.MoveToUserPerspectiveLocation();
            TryProgressState();
        }
        else if (Input.IsActionJustPressed("RerollSingular") && DiceManager.SelectedDiceCollection.Count() > 0
            && StageManager.TryRerollSingleDice(DiceManager.SelectedDiceCollection.Count()))
        {
            DiceManager.RerollSelectedDice();
            BuildAndSetScoreText();
            cameraController.MoveToUserPerspectiveLocation();
            TryProgressState();
        }
        else if (Input.IsActionJustPressed("RerollAll") && StageManager.TryRerollAllDice())
        {
            GD.Print("Try to reroll all dice");
            DiceManager.RerollAllDice();
            BuildAndSetScoreText();
            cameraController.MoveToUserPerspectiveLocation();
            TryProgressState();
        }
        else if (Input.IsActionJustPressed("Accept") && TryAddToRoundScore())
        {
            var scoredDiceCount = DiceManager.ScoredDiceCollection.Count() + DiceManager.SelectedDiceCollection.Count();
            if (scoredDiceCount == PlayerManager.DiceCollection.Count())
            {
                DiceManager.ResetAllDice();
            }
            else
            {
                DiceManager.ResetUnscoredDice();
            }

            cameraController.MoveToUserPerspectiveLocation();
            TryProgressState();
        }
        else if (Input.IsActionJustPressed("Confirm") && TrySubmitRoundScore())
        {
            if (StageManager.IsStageScoreHigherThanScoreToWin())
            {
                GD.Print("Stage score is higher than score to win.");
                RoundManager.Reset();
                if (!StageManager.TryGoToNextStage())
                {
                    GameOver("You won!");
                }
            }
            else if (!StageManager.TryToUseScoreAttempt())
            {
                GD.Print("No more score attempts.");
                GameOver("You ran out of score attempts and lost this stage.");
            }

            if (GameStateManager.GameState != GameState.GameOver)
            {
                GD.Print("Moving to next round.");
                BuildAndSetScoreText();
                DiceManager.ResetAllDice();
                cameraController.MoveToUserPerspectiveLocation();
                TryProgressState();
            }
        }
    }

    public void HandleGameOverState()
    {
        if (Input.IsActionJustPressed("Accept"))
        {
            StartGame();
        }
    }

    public bool TryProgressState()
    {
        if (GameStateManager.GameState is not GameState.GameOver)
        {
            GameStateManager.ProgressState();
            UiManager.SetInstructionLabel(GameStateManager.GameState, GameStateManager.GetSelectDiceSubstate);
            return true;
        }
        return false;
    }

    #endregion

    #region Mouse
    public void HandleMouseInput(InputEvent inputEvent)
    {
        if (GameStateManager.GameState == GameState.SelectDice)
        {
            if (inputEvent is InputEventMouseButton mouseButtonEvent)
            {
                if (DiceManager.TryHandleMouseButtonInputForDiceSelect(mouseButtonEvent))
                {
                    BuildAndSetScoreText();
                    if (Configuration.ConfigValues.IsDebug)
                    {
                        DebugMenu.SetNewDiceCollection(DiceManager.SelectedDiceCollection);
                    }
                }
            }
        }
    }

    #endregion

    #region PlayerSetup

    public void StartGame()
    {
        StageManager.Reset();
        RoundManager.Reset();
        ClearFarkle();
        DiceManager.ResetAllDice();
        cameraController.MoveToUserPerspectiveLocation();
        GameStateManager.StartGame();
        BuildAndSetScoreText();
        UiManager.SetInstructionLabel(GameStateManager.GameState, GameStateManager.GetSelectDiceSubstate);
    }

    #endregion

    #region Score

    public bool TryAddToRoundScore()
    {
        var scoreResult = ScoreManager.TryGetScore(DiceManager.SelectedDiceCollection);

        if (scoreResult.ResultType is CalculateScoreResultType.HasUnusedScoreDice)
        {
            scoreResult.UnusedDice.FlashRed();
            return false;
        }
        if (scoreResult.Scored)
        {
            RoundManager.AddToRoundScore(scoreResult.Score);
            BuildAndSetScoreText();
            return true;
        }

        //implicitly no dice scored
        DiceManager.SelectedDiceCollection.FlashRed();
        return false;
    }

    public bool TrySubmitRoundScore()
    {
        if (DiceManager.AreAllDiceBeingSubmittedForScore())
        {
            DiceManager.SelectedDiceCollection.FlashRed();
            return false;
        }
        var scoreResult = ScoreManager.TryGetScore(DiceManager.SelectedDiceCollection);

        if (scoreResult.ResultType is CalculateScoreResultType.HasUnusedScoreDice)
        {
            scoreResult.UnusedDice.FlashRed();
            return false;
        }
        if (scoreResult.Scored)
        {
            RoundManager.AddToRoundScore(scoreResult.Score);
            StageManager.AddToStageScore(RoundManager.RoundScore);
            BuildAndSetScoreText();
            return true;
        }
        DiceManager.SelectedDiceCollection.FlashRed();
        return false;
    }

    public void GameOver(string extraMessage = null)
    {
        BuildAndSetScoreText();
        GameStateManager.GameOver();
        UiManager.SetInstructionLabel(GameStateManager.GameState, GameStateManager.GetSelectDiceSubstate, extraMessage);
        DiceManager.ResetAllDice();
    }

    public void PreSelectDiceFarkleCheck()
    {
        if (DidRolledDiceFarkle())
        {
            Farkle();
        }
    }

    public bool DidRolledDiceFarkle()
    {
        var score = ScoreManager.TryGetScore(DiceManager.RollableDiceCollection);
        return !score.Scored && score.ResultType is CalculateScoreResultType.NoScorableDice;
    }

    public void Farkle()
    {
        GameStateManager.Farkle();
        if (!StageManager.TryToFarkle())
        {
            GameOver("You Farkled your last score attempt and lost this stage.");
        }
        UiManager.Farkle();
    }

    public void ClearFarkle()
    {
        GameStateManager.ClearFarkle();
        UiManager.ClearFarkle();
    }

    #endregion

    #region Dice
    public void ThrowDice()
    {
        DiceManager.ThrowDice();
        cameraController.MoveToDiceZoomLocation();
    }

    public void BuildAndSetScoreText()
    {
        UiManager.BuildAndSetScoreText(
            ScoreManager.TryGetScore(DiceManager.SelectedDiceCollection).Score,
            RoundManager.RoundScore,
            StageManager.ScoreAttemptsLeft,
            StageManager.StageScore,
            StageManager.GetCurrentStageScoreToWin(),
            StageManager.GetNumberOfStages(),
            StageManager.GetCurrentStageNumber(),
            StageManager.RetriesLeft);
    }

    public void StartThrowForceBar()
    {
        //ripped from https://gameidea.org/2024/12/13/making-a-health-bar-and-health-system-in-godot/
        var screenPos = cameraController.GetCamera().UnprojectPosition(throwLocationBall.GlobalPosition);
        UiManager.StartThrowForceBar(screenPos);
    }

    #endregion
}
