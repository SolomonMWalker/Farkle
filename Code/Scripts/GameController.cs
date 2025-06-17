using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public partial class GameController : Node3D
{
    #region Properties

    private const string DebugMenuRelPath = "res://Scenes/Debug/DebugMenu.tscn";

    public DebugMenu DebugMenu { get; private set; }
    private CameraController cameraController;
    public GameStateManager GameStateManager { get; private set; }
    public PlayerManager activePlayerManager, regularPlayerManager, lastRoundPlayerManager;
    private Node3D diceHolder, outOfPlayDiceLocation;
    private ThrowLocationBall throwLocationBall;
    public DiceManager DiceManager { get; private set; }
    private Label instructionLabel, scoreLabel, farkleLabel, playerTurnLabel, lastRoundLabel;
    private RichTextLabel scorePerRollLabel;
    private LineEdit playerInputLineEdit;
    private bool arePlayersReady = false;
    private bool onLastRound = false;
    private PlayerScore lastRoundPlayerScore;
    private int roundScore = 0;

    #endregion

    #region Initialization
    public override void _Ready()
    {
        base._Ready();
        Configuration.SetUpConfiguration();
        diceHolder = this.GetChildByName<Node3D>("DiceHolder");
        throwLocationBall = FindChild("DiceTable").GetChildByName<ThrowLocationBall>("ThrowLocationBall");
        outOfPlayDiceLocation = this.GetChildByName<Node3D>("OutOfPlayDiceLocation");
        cameraController = this.GetChildByName<CameraController>("CameraController");
        instructionLabel = this.GetChildByName<Control>("ControlParent").GetChildByName<Label>("Instructions");
        scoreLabel = this.GetChildByName<Control>("ControlParent").GetChildByName<Label>("Score");
        farkleLabel = this.GetChildByName<Control>("ControlParent").GetChildByName<Label>("FarkleLabel");
        playerTurnLabel = this.GetChildByName<Control>("ControlParent").GetChildByName<Label>("PlayerTurn");
        lastRoundLabel = this.GetChildByName<Control>("ControlParent").GetChildByName<Label>("OnLastRound");
        scorePerRollLabel = this.GetChildByName<Control>("ControlParent").GetChildByName<RichTextLabel>("ScorePerRoll");
        playerInputLineEdit = this.GetChildByName<Control>("ControlParent").GetChildByName<LineEdit>("PlayerInput");
        GameStateManager = new GameStateManager();
        DiceManager = new DiceManager(diceHolder, outOfPlayDiceLocation, throwLocationBall.throwLocation, scoreLabel);

        playerInputLineEdit.TextSubmitted += HandlePlayerNameSubmitted;
        playerInputLineEdit.KeepEditingOnTextSubmit = true;

        if (Configuration.ConfigValues.IsDebug)
        {
            DebugMenu = GD.Load<PackedScene>(DebugMenuRelPath).Instantiate<DebugMenu>();
            this.GetChildByName<Control>("ControlParent").AddChild(DebugMenu);
            DebugMenu.Initialize(this);
        }

        SetupGameStateEnterExitActions();

        GameStateManager.StartPlayerSetup();
    }

    public void SetupGameStateEnterExitActions()
    {
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.PlayerSetup, [
            StartPlayerSetup,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.PreRoll, [
            DiceManager.ReadyDiceForRoll,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.RollReady, [
            AnimateDiceThrowerBall,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.Rolling, [
            ThrowDice,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.SelectDice, [
            PreSelectDiceFarkleCheck,
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
            case GameState.PlayerSetup:
                //do nothing here
                break;
            case GameState.PreRoll:
                if (Input.IsActionJustPressed("space"))
                {
                    TryProgressState();
                }
                break;
            case GameState.RollReady:
                if (Input.IsActionJustPressed("space"))
                {
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
        if (GameStateManager.GetSelectDiceSubstate is SelectDiceSubstate.Farkled)
        {
            if (onLastRound)
            {
                GameOver();
            }
            else if (Input.IsActionJustPressed("space"))
            {
                ClearFarkle();
                DiceManager.ResetAllDice();
                if (TryAdvanceTurn())
                {
                    BuildAndSetScoreText();
                    SetPlayerTurnLabel();
                }

                cameraController.MoveToUserPerspectiveLocation();
                TryProgressState();
            }
        }
        else if (Input.IsActionJustPressed("space") && TryRecordScore())
        {
            var scoredDiceCount = DiceManager.ScoredDiceCollection.Count() + DiceManager.SelectedDiceCollection.Count();
            if (scoredDiceCount == DiceManager.PersistentDiceCollection.Count())
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
        else if (Input.IsActionJustPressed("enter"))
        {
            var scoredDiceCount = DiceManager.ScoredDiceCollection.Count() + DiceManager.SelectedDiceCollection.Count();
            if (scoredDiceCount == DiceManager.PersistentDiceCollection.Count())
            {
                DiceManager.SelectedDiceCollection.FlashRed();
            }
            else if (TryRecordScore())
            {
                activePlayerManager.AddToPlayerScore(activePlayerManager.GetWhoseTurnItIs(), roundScore);
                roundScore = 0;
                DiceManager.ResetAllDice();

                if (TryAdvanceTurn())
                {
                    BuildAndSetScoreText();
                    SetPlayerTurnLabel();
                }
                else
                {
                    GameOver();
                }
                cameraController.MoveToUserPerspectiveLocation();
                TryProgressState();
            }
        }
    }

    public void HandleGameOverState()
    {
        if (Input.IsActionJustPressed("space"))
        {
            TryProgressState();
        }
    }

    public bool TryProgressState()
    {
        if (GameStateManager.GameState is not GameState.GameOver)
        {
            GameStateManager.ProgressState();
            SetInstructionLabel(GameStateManager.GameState);
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
                    scoreLabel.Text = DiceManager.SelectedDiceCollection.CalculateScore().Score.ToString();

                    if (Configuration.ConfigValues.IsDebug)
                    {
                        DebugMenu.SetNewDiceCollection(DiceManager.SelectedDiceCollection);
                    }
                }
            }
        }
    }

    public bool TryHandleMouseClickOnObject(ulong? objInstanceId, out RootDice clickedDice)
    {
        if (objInstanceId != null && DiceManager.RollableDiceCollection.TryGetDiceWithInstanceIdEqualTo(objInstanceId.Value, out var selectedDice))
        {
            clickedDice = selectedDice;
            return true;
        }
        clickedDice = null;
        return false;
    }

    #endregion

    #region PlayerSetup
    public void StartPlayerSetup()
    {
        ClearFarkle();
        if (Configuration.ConfigValues.IsDebug)
        {
            regularPlayerManager = new PlayerManager(["Player1", "Player2"]);
            StartGame();
        }
        else
        {
            playerInputLineEdit.Editable = true;
            playerInputLineEdit.Visible = true;
            SetInstructionLabel(GameState.PlayerSetup);
            playerTurnLabel.Text = "Enter player 1 name";
            playerInputLineEdit.Edit();
        }
    }
    public void HandlePlayerNameSubmitted(string newText)
    {
        if (regularPlayerManager is null)
        {
            regularPlayerManager = new PlayerManager(newText);
            playerInputLineEdit.Clear();
            playerTurnLabel.Text = "Enter player 2 name";
        }
        else
        {
            regularPlayerManager.AddPlayer(newText);
            playerInputLineEdit.Clear();
            StartGame();
        }
    }

    public void StartGame()
    {
        playerInputLineEdit.Editable = false;
        playerInputLineEdit.Visible = false;
        regularPlayerManager.StartGame();
        activePlayerManager = regularPlayerManager;
        SetPlayerTurnLabel();
        BuildAndSetScoreText();
        SetInstructionLabel(GameState.PreRoll);
        GameStateManager.StartGame();
    }

    #endregion

    #region UI
    public void SetPlayerTurnLabel(bool gameOver = false)
    {
        playerTurnLabel.Text = gameOver ? "GameOver, press space to play again"
            : $"{activePlayerManager.GetWhoseTurnItIs()}'s turn";
    }

    public void BuildAndSetScoreText()
    {
        var scoreString = "";
        foreach (var player in activePlayerManager.players)
        {
            scoreString += $"{player} total score = {activePlayerManager.playerScores[player]}\n";
        }
        if (onLastRound)
        {
            scoreString += $"{lastRoundPlayerScore.Player} total score = {lastRoundPlayerScore.Score}\n";
        }
        scoreString += $"{activePlayerManager.GetWhoseTurnItIs()} is playing the current round.\n";
        scoreString += $"Round score = {roundScore}";

        scorePerRollLabel.Text = scoreString;
    }

    public void SetInstructionLabel(GameState gameState)
    {
        instructionLabel.Text = gameState switch
        {
            GameState.PlayerSetup => "Type in player name and press enter to continue.",
            GameState.PreRoll => "Press space to find rolling position.",
            GameState.RollReady => "Press space to select rolling position and roll dice.",
            GameState.Rolling => "Wait for dice to finish rolling.",
            GameState.SelectDice => "Select dice with the mouse. Press space to enter your score and roll again. Press enter to submit your score. If all six die are scored, you must press space to roll again.",
            GameState.GameOver => "Press space to play a new game.",
            _ => ""
        };
    }

    #endregion

    #region PlayersTurnsAndScore

    public bool TryAdvanceTurn()
    {
        if (onLastRound && !activePlayerManager.CanAdvanceTurnOnLastRound())
        {
            return false;
        }
        if (TryBeginLastRoundAndAdvanceTurn())
        {
            return true;
        }
        activePlayerManager.AdvanceTurn();
        return true;
    }

    public bool TryRecordScore()
    {
        if (DiceManager.TryGetSelectedDiceScore(out var tempScore))
        {
            scoreLabel.Text = "";
            roundScore += tempScore;
            BuildAndSetScoreText();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a player is at max score, and if they are, begin the last round and advance the turn to the next player
    /// </summary>
    /// <returns></returns>
    public bool TryBeginLastRoundAndAdvanceTurn()
    {
        if (!onLastRound && activePlayerManager.TryGetPlayerAtScore(Configuration.ConfigValues.ScoreToWin, out var playerScoreAtMaxScore))
        {
            onLastRound = true;
            lastRoundPlayerScore = playerScoreAtMaxScore;
            lastRoundLabel.Text = $"On last round with {lastRoundPlayerScore.Player} leading!";
            lastRoundPlayerManager = regularPlayerManager.GetLastRoundPlayerManager(playerScoreAtMaxScore.Player);
            activePlayerManager = lastRoundPlayerManager;
            return true;
        }
        return false;
    }

    public void GameOver()
    {
        GameStateManager.GameOver();
        BuildAndSetScoreText();
        SetPlayerTurnLabel(gameOver: true);
    }

    public void PreSelectDiceFarkleCheck()
    {
        if (DidRolledDiceFarkle())
        {
            Farkle();
        }
    }

    public bool DidRolledDiceFarkle() => DiceManager.RollableDiceCollection.CalculateScore().Score == -1;

    public void Farkle()
    {
        roundScore = 0;
        GameStateManager.Farkle();
        farkleLabel.Text = "You Farkled";
    }

    public void ClearFarkle()
    {
        GameStateManager.ClearFarkle();
        farkleLabel.Text = "";
    }

    #endregion

    #region Dice
    public void AnimateDiceThrowerBall()
    {
        throwLocationBall.Animate();
    }

    public void ThrowDice()
    {
        DiceManager.ThrowDice();
        throwLocationBall.StopAnimation();
        cameraController.MoveToDiceZoomLocation();
    }

    public void RescoreSelectedDice()
    {
        DiceManager.RescoreSelectedDice();
        scoreLabel.Text = DiceManager.SelectedDiceCollection.CalculateScore().Score.ToString();
    }

    #endregion
}
