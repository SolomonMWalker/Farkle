using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public partial class GameController : Node3D
{
    #region Properties

    private const string RootDiceRelPath = "res://Scenes/root_dice.tscn";
    private const string DebugMenuRelPath = "res://Scenes/DebugMenu.tscn";

    public DebugMenu DebugMenu { get; private set; }
    private DiceCollection persistentDiceCollection, rollableDiceCollection, selectedDiceCollection,
        scoredDiceCollection;
    private CameraController cameraController;
    public GameStateManager GameStateManager { get; private set; }
    public PlayerManager activePlayerManager, regularPlayerManager, lastRoundPlayerManager;
    private Node3D diceHolder, outOfPlayDiceLocation;
    private ThrowLocationBall throwLocationBall;
    private PackedScene packedRootDice;
    private Vector2 mousePosition = Vector2.Zero;
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
        packedRootDice = GD.Load<PackedScene>(RootDiceRelPath);
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
        persistentDiceCollection = new DiceCollection();
        rollableDiceCollection = new DiceCollection();
        selectedDiceCollection = new DiceCollection();
        scoredDiceCollection = new DiceCollection();

        playerInputLineEdit.TextSubmitted += HandlePlayerNameSubmitted;
        playerInputLineEdit.KeepEditingOnTextSubmit = true;

        SetupGameStateEnterExitActions();

        CreateDiceCollection();

        if (Configuration.ConfigValues.IsDebug)
        {
            DebugMenu = GD.Load<PackedScene>(DebugMenuRelPath).Instantiate<DebugMenu>();
            this.GetChildByName<Control>("ControlParent").AddChild(DebugMenu);
            DebugMenu.Initialize(this);
        }

        GameStateManager.StartPlayerSetup();
    }

    public void SetupGameStateEnterExitActions()
    {
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.PlayerSetup, [
            StartPlayerSetup,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.PreRoll, [
            ReadyDiceForRoll,
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
                EndOverride,
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
                if (rollableDiceCollection.IsDoneRolling() && !cameraController.IsAnimationPlaying())
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
                ResetAllDice();
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
            var scoredDiceCount = scoredDiceCollection.Count() + selectedDiceCollection.Count();
            if (scoredDiceCount == persistentDiceCollection.Count())
            {
                ResetAllDice();
            }
            else
            {
                ResetUnscoredDice();
            }

            cameraController.MoveToUserPerspectiveLocation();
            TryProgressState();
        }
        else if (Input.IsActionJustPressed("enter"))
        {
            var scoredDiceCount = scoredDiceCollection.Count() + selectedDiceCollection.Count();
            if (scoredDiceCount == persistentDiceCollection.Count())
            {
                selectedDiceCollection.FlashRed();
            }
            else if (TryRecordScore())
            {
                activePlayerManager.AddToPlayerScore(activePlayerManager.GetWhoseTurnItIs(), roundScore);
                roundScore = 0;
                ResetAllDice();

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
            if (inputEvent is InputEventMouseMotion mouseMotion)
            {
                mousePosition = mouseMotion.Position;
            }
            else if (inputEvent is InputEventMouseButton mouseButton)
            {
                var clickedOnObject = MouseTools.GetCollisionIdFromMouseClick(mousePosition, mouseButton, this);
                if (TryHandleMouseClickOnObject(clickedOnObject, out var selectedDice))
                {
                    if (selectedDiceCollection.diceList.Contains(selectedDice))
                    {
                        selectedDiceCollection = selectedDiceCollection.RemoveDice(selectedDice);
                        scoreLabel.Text = selectedDiceCollection.CalculateScore().Score.ToString();
                        selectedDice.ToggleSelectDice();
                    }
                    else
                    {
                        selectedDiceCollection = selectedDiceCollection.AddDice(selectedDice);
                        scoreLabel.Text = selectedDiceCollection.CalculateScore().Score.ToString();
                        selectedDice.ToggleSelectDice();
                    }

                    if (Configuration.ConfigValues.IsDebug)
                    {
                        DebugMenu.AddDice(selectedDice);
                    }
                }
            }
        }
    }

    public bool TryHandleMouseClickOnObject(ulong? objInstanceId, out RootDice clickedDice)
    {
        if (objInstanceId != null && rollableDiceCollection.TryGetDiceWithInstanceIdEqualTo(objInstanceId.Value, out var selectedDice))
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
        if (selectedDiceCollection.Count() == 0)
        {
            //Do nothing, maybe tell the user to select some scoring dice
            return false;
        }

        selectedDiceCollection.CalculateScore();
        if (selectedDiceCollection.HasUnusedScoreDice() ||
            selectedDiceCollection.CalculateScoreResult.Score == -1)
        {
            selectedDiceCollection.CalculateScoreResult.UnusedDice.ToList()
                .ForEach(d => d.FlashRed());
            return false;
        }
        scoreLabel.Text = "";
        roundScore += selectedDiceCollection.CalculateScoreResult.Score;
        BuildAndSetScoreText();
        return true;
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

    public bool DidRolledDiceFarkle() => rollableDiceCollection.CalculateScore().Score == -1;

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
    public void ResetAllDice()
    {
        persistentDiceCollection.diceList.ForEach(d => d.UnselectDice());
        rollableDiceCollection = new DiceCollection(persistentDiceCollection);
        scoredDiceCollection = new DiceCollection();
        selectedDiceCollection = new DiceCollection();
        MoveRollableDiceOffCamera();
    }

    public void ResetUnscoredDice()
    {
        rollableDiceCollection = rollableDiceCollection.RemoveDice(selectedDiceCollection);

        scoredDiceCollection = scoredDiceCollection.AddDice(selectedDiceCollection);
        scoredDiceCollection.TurnOff();
        scoredDiceCollection.diceList.ForEach(d => d.UnselectDice());
        MoveScoredDiceOffCamera();

        selectedDiceCollection = new DiceCollection();
    }

    public void CreateDiceCollection()
    {
        List<RootDice> tempDiceList = [];
        for (int i = 0; i < Configuration.ConfigValues.NumOfStartingDice; i++)
        {
            var dice = packedRootDice.Instantiate<RootDice>();
            tempDiceList.Add(dice);
        }
        persistentDiceCollection = new DiceCollection(tempDiceList);
        persistentDiceCollection.SetParent(diceHolder);
        persistentDiceCollection.SetDebug(Configuration.ConfigValues.IsDebug);
        rollableDiceCollection = new DiceCollection(persistentDiceCollection);
    }

    public void SetDiceRotationForThrow()
    {
        foreach (RootDice dice in rollableDiceCollection.diceList)
        {
            dice.Rotate(HelperMethods.GetRandomVector3().Normalized(), GD.Randf() * (2 * Mathf.Pi));
        }
    }

    public void SetDiceVelocityForThrow()
    {
        var baseVelocity = new Vector3(0, 0, -1) * 6;
        foreach (RootDice dice in rollableDiceCollection.diceList)
        {
            dice.SetVelocityUponThrow(HelperMethods.FuzzyUpVector3(baseVelocity, 0.5f));
        }
    }

    public void ReadyDiceForRoll() //turn off dice, set rotation and velocity
    {
        rollableDiceCollection.TurnOff();
        SetDiceRotationForThrow();
        SetDiceVelocityForThrow();
    }

    public void AnimateDiceThrowerBall()
    {
        throwLocationBall.Animate();
    }

    public void ThrowDice()
    {
        MoveRollableDiceToThrowLocation();
        rollableDiceCollection.TurnOn();
        rollableDiceCollection.ThrowDice();
        throwLocationBall.StopAnimation();
        cameraController.MoveToDiceZoomLocation();
    }

    public void MoveRollableDiceToThrowLocation()
    {
        rollableDiceCollection.SetGlobalPosition(throwLocationBall.throwLocation.GlobalPosition);
    }

    public void MoveScoredDiceOffCamera()
    {
        rollableDiceCollection.SetGlobalPosition(outOfPlayDiceLocation.GlobalPosition);
    }

    public void MoveRollableDiceOffCamera()
    {
        rollableDiceCollection.SetGlobalPosition(outOfPlayDiceLocation.GlobalPosition);
    }

    #endregion

    #region Debug

    public void RescoreSelectedDice()
    {
        if (Configuration.ConfigValues.IsDebug)
        {
            selectedDiceCollection.CalculateScore();
            scoreLabel.Text = selectedDiceCollection.CalculateScore().Score.ToString();
        }
    }

    public void EndOverride() => persistentDiceCollection.EndOverrides();

    #endregion
}
