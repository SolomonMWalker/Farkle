using Godot;

public partial class GameController : Node3D
{
    #region Properties

    private const string DebugMenuRelPath = "res://Scenes/Debug/debug_menu.tscn";

    public DebugMenu DebugMenu { get; private set; }
    private CameraController cameraController;
    public GameStateManager GameStateManager { get; private set; }
    private ThrowLocationBall throwLocationBall;
    public PlayerManager PlayerManager { get; private set; }
    public DiceManager DiceManager { get; private set; }
    public ScoreManager ScoreManager { get; private set; }
    public TableManager TableManager { get; private set; }
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
        DiceManager = new DiceManager(PlayerManager,
            this.GetChildByName<Node3D>("DiceHolder"),
            this.GetChildByName<Node3D>("OutOfPlayDiceLocation"),
            throwLocationBall.throwLocation);
        TableManager = new TableManager(
            (TableGraph)FindChild("TableGraph", recursive: true),
            cameraController.GetCamera()
        );
        StageManager = new StageManager(PlayerManager);
        RoundManager = new RoundManager(PlayerManager);
        ScoreManager = new ScoreManager();


        if (Configuration.ConfigValues.IsDebug)
        {
            DebugMenu = GD.Load<PackedScene>(DebugMenuRelPath).Instantiate<DebugMenu>();
            this.GetChildByName<Control>("ControlParent").AddChild(DebugMenu);
            DebugMenu.Initialize(this);
        }

        SetupGameStateEnterExitActions();

        StartGame();
        TryProgressState(GameState.PreRoll);
    }

    public void SetupGameStateEnterExitActions()
    {
        GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Enter, GameState.TableZoomAnimation, [
            cameraController.MoveToTableZoomLocation
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Enter, GameState.UserPerspectiveZoomAnimation, [
            cameraController.MoveToUserPerspectiveLocation
        ]);   
        GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Enter, GameState.PreRoll, [
            DiceManager.ReadyRollableDiceForThrow,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Enter, GameState.FindRollPosition, [
            throwLocationBall.Animate,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Enter, GameState.FindRollStrength, [
            throwLocationBall.StopAnimation, StartThrowForceBar,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Exit, GameState.FindRollStrength, [
            UiManager.EndThrowForceBar, 
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Enter, GameState.ThrowDice, [
            ThrowDice, throwLocationBall.ResetPositon,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Enter, GameState.SelectDice, [
            DiceManager.AddDiceLeftOnTableToRollableDiceCollection, PreSelectDiceFarkleCheck,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Exit, GameState.SelectDice, [
            DiceManager.UnselectAllDice,
        ]);       

        if (Configuration.ConfigValues.IsDebug)
        {
            GameStateManager.AddOnStateEnterOrExitAction(enterOrExit: EnterOrExit.Exit, GameState.SelectDice, [
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
        var gameState = GameStateManager.GetCurrentGameState();

        switch (gameState)
        {
            case GameState.TableZoomAnimation or GameState.UserPerspectiveZoomAnimation:
                if (!cameraController.IsAnimationPlaying())
                {
                    TryProgressState(GameStateManager.GetAndErasePostCameraZoomState());
                }
                break;
            case GameState.PreRoll:
                if (Input.IsActionJustPressed("Accept"))
                {
                    TryProgressState(GameState.FindRollPosition);
                }
                break;
            case GameState.FindRollPosition:
                if (Input.IsActionJustPressed("Accept"))
                {
                    TryProgressState(GameState.FindRollStrength);
                }
                break;
            case GameState.FindRollStrength:
                if (Input.IsActionJustPressed("Accept"))
                {
                    DiceManager.MultiplyVelocityByThrowForceValue(UiManager.GetThrowForceBarValue());
                    TryProgressState(GameState.ThrowDice);
                }
                break;
            case GameState.ThrowDice:
                TryProgressStateWithCameraZoom(GameState.TableZoomAnimation, GameState.Rolling);
                break;
            case GameState.Rolling:
                if (DiceManager.RollableDiceCollection.IsDoneRolling())
                {
                    TryProgressState(GameState.SelectDice);
                }
                break;
            case GameState.SelectDice:
                HandleSelectDiceState();
                break;
            case GameState.GameOver:
                HandleGameOverState();
                break;
        }
    }

    public void HandleSelectDiceState()
    {
        //Before SelectDiceState, Farkle check is run by state OnEnterAction
        if (GameStateManager.GetSelectDiceSubstate() is SelectDiceSubstate.FarkledGameOver)
        {
            TryProgressStateWithCameraZoom(GameState.TableZoomAnimation, GameState.GameOver);
            GameOver("You Farkled your last score attempt");
        }
        else if (GameStateManager.GetSelectDiceSubstate() is SelectDiceSubstate.FarkledNotGameOver && Input.IsActionJustPressed("Accept"))
        {
            ClearFarkle();
            BuildAndSetScoreText();
            DiceManager.ResetAllDice();
            TryProgressStateWithCameraZoom(GameState.UserPerspectiveZoomAnimation, GameState.PreRoll);
        }
        else if (Input.IsActionJustPressed("RerollSingular") && DiceManager.SelectedDiceCollection.Count() > 0
            && StageManager.TryRerollSingleDice(DiceManager.SelectedDiceCollection.Count()))
        {
            DiceManager.RerollSelectedDice();
            BuildAndSetScoreText();
            TryProgressStateWithCameraZoom(GameState.UserPerspectiveZoomAnimation, GameState.PreRoll);
        }
        else if (Input.IsActionJustPressed("RerollAll") && StageManager.TryRerollAllDice())
        {
            GD.Print("Try to reroll all dice");
            DiceManager.RerollAllDice();
            BuildAndSetScoreText();
            TryProgressStateWithCameraZoom(GameState.UserPerspectiveZoomAnimation, GameState.PreRoll);
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

            TryProgressStateWithCameraZoom(GameState.UserPerspectiveZoomAnimation, GameState.PreRoll);
        }
        else if (Input.IsActionJustPressed("Confirm") && TrySubmitRoundScore())
        {
            var didWeGameOver = false;
            var didWeWin = false;

            if (StageManager.IsStageScoreHigherThanScoreToWin())
            {
                GD.Print("Stage score is higher than score to win.");
                RoundManager.Reset();
                if (!StageManager.TryGoToNextStage())
                {
                    didWeGameOver = true;
                    didWeWin = true;
                }
            }
            else if (!RoundManager.TryToUseScoreAttempt())
            {
                GD.Print("No more score attempts.");
                didWeGameOver = true;
                didWeWin = false;
            }

            if (!didWeGameOver)
            {
                GD.Print("Moving to next round.");
                BuildAndSetScoreText();
                DiceManager.ResetAllDice();
                TryProgressStateWithCameraZoom(GameState.UserPerspectiveZoomAnimation, GameState.PreRoll);
            }
            else
            {
                GameStateManager.TryProgressStateWithCameraZoomBetween(GameState.TableZoomAnimation, GameState.GameOver);
                if (didWeWin)
                {
                    GameOver("You won!");
                }
                else
                {
                    GameOver("You ran out of score attempts and lost this stage.");
                }
            }
        }
    }

    public void HandleSetTableState()
    {

    }

    public void HandleGameOverState()
    {
        if (Input.IsActionJustPressed("Accept"))
        {
            StartGame();
            TryProgressStateWithCameraZoom(GameState.UserPerspectiveZoomAnimation, GameState.PreRoll);
        }
    }

    public void TryProgressState(GameState nextState)
    {
        GameStateManager.TryProgressState(nextState);
        UiManager.SetInstructionLabel(GameStateManager.GetCurrentGameState(), GameStateManager.GetSelectDiceSubstate());
    }

    public void TryProgressStateWithCameraZoom(GameState cameraZoomState, GameState nextState)
    {
        GameStateManager.TryProgressStateWithCameraZoomBetween(cameraZoomState, nextState);
        UiManager.SetInstructionLabel(GameStateManager.GetCurrentGameState(), GameStateManager.GetSelectDiceSubstate());
    }

    #endregion

    #region Mouse
    public void HandleMouseInput(InputEvent inputEvent)
    {
        if (GameStateManager.GetCurrentGameState() == GameState.SelectDice)
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
        else if (GameStateManager.GetCurrentGameState() == GameState.SetTable)
        {
            
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
        BuildAndSetScoreText();
        UiManager.SetInstructionLabel(GameState.PreRoll, GameStateManager.GetSelectDiceSubstate());
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
        UiManager.SetInstructionLabel(GameState.GameOver, GameStateManager.GetSelectDiceSubstate(), extraMessage);
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

    public SelectDiceSubstate Farkle()
    {
        SelectDiceSubstate sdsState;
        if (!RoundManager.TryToFarkle())
        {
            GameStateManager.Farkle(isGameOver: true);
            sdsState = SelectDiceSubstate.FarkledGameOver;
        }
        else
        {
            RoundManager.StartNewRound();
            GameStateManager.Farkle(isGameOver: false);
            sdsState = SelectDiceSubstate.FarkledNotGameOver;
        }
        UiManager.Farkle();
        return sdsState;
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
    }

    public void BuildAndSetScoreText()
    {
        UiManager.BuildAndSetScoreText(
            ScoreManager.TryGetScore(DiceManager.SelectedDiceCollection).Score,
            RoundManager.RoundScore,
            RoundManager.CurrentScoreTries,
            StageManager.StageScore,
            StageManager.GetCurrentStageScoreToWin(),
            StageManager.GetNumberOfStages(),
            StageManager.GetCurrentStageNumber(),
            StageManager.CurrentRerolls);
    }

    public void StartThrowForceBar()
    {
        //ripped from https://gameidea.org/2024/12/13/making-a-health-bar-and-health-system-in-godot/
        var screenPos = cameraController.GetCamera().UnprojectPosition(throwLocationBall.GlobalPosition);
        UiManager.StartThrowForceBar(screenPos);
    }

    #endregion
}
