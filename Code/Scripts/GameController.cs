using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public partial class GameController : Node3D
{
    public const int ScoreToWin = 10000;
    public const int DiceAmount = 6;

    private DiceCollection persistentDiceCollection, rollableDiceCollection, selectedDiceCollection,
        scoredDiceCollection;
    private CameraController cameraController;
    private GameStateManager gameStateManager;
    public PlayerManager activePlayerManager, regularPlayerManager, lastRoundPlayerManager;
    private Node3D diceHolder, outOfPlayDiceLocation;
    private ThrowLocationBall throwLocationBall;
    private PackedScene packedRootDice;
    private Vector2 mousePosition;
    private Label gameStateLabel, scoreLabel, farkleLabel, playerTurnLabel, lastRoundLabel;
    private RichTextLabel scorePerRollLabel;
    private LineEdit playerInputLineEdit;
    private bool arePlayersReady = false;
    private bool onLastRound = false;
    private PlayerScore lastRoundPlayerScore;
    private int roundScore = 0;

    public override void _Ready()
    {
        base._Ready();
        rollableDiceCollection = new DiceCollection();
        diceHolder = this.FindChild<Node3D>("DiceHolder");
        throwLocationBall = FindChild("DiceTable").FindChild<ThrowLocationBall>("ThrowLocationBall");
        outOfPlayDiceLocation = this.FindChild<Node3D>("OutOfPlayDiceLocation");
        packedRootDice = GD.Load<PackedScene>("res://Scenes/root_dice.tscn");
        cameraController = this.FindChild<CameraController>("CameraController");
        gameStateLabel = this.FindChild<Control>("ControlParent").FindChild<Label>("GameState");
        scoreLabel = this.FindChild<Control>("ControlParent").FindChild<Label>("Score");
        farkleLabel = this.FindChild<Control>("ControlParent").FindChild<Label>("FarkleLabel");
        playerTurnLabel = this.FindChild<Control>("ControlParent").FindChild<Label>("PlayerTurn");
        lastRoundLabel = this.FindChild<Control>("ControlParent").FindChild<Label>("OnLastRound");
        scorePerRollLabel = this.FindChild<Control>("ControlParent").FindChild<RichTextLabel>("ScorePerRoll");
        playerInputLineEdit = this.FindChild<Control>("ControlParent").FindChild<LineEdit>("PlayerInput");
        gameStateManager = new GameStateManager();
        mousePosition = Vector2.Zero;
        selectedDiceCollection = new DiceCollection();
        scoredDiceCollection = new DiceCollection();

        playerInputLineEdit.TextSubmitted += HandlePlayerNameSubmitted;
        playerInputLineEdit.KeepEditingOnTextSubmit = true;

        StartPlayerSetup();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        RunGame();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        HandleDicePhysics();
    }

    public void RunGame()
    {
        var gameState = gameStateManager.GetState;

        switch (gameState)
        {
            case GameState.PlayerSetup:
                //do nothing here
                break;
            case GameState.PreRoll:
                if (Input.IsActionJustPressed("space"))
                {
                    ReadyDiceForRoll();
                    throwLocationBall.Animate();
                    TryProgressState();
                }
                break;
            case GameState.RollReady:
                if (Input.IsActionJustPressed("space"))
                {
                    ThrowDice();
                    throwLocationBall.StopAnimation();
                    cameraController.MoveToDiceZoomLocation();
                    TryProgressState();
                }
                break;
            case GameState.Rolling:
                if (rollableDiceCollection.IsDoneRolling() && !cameraController.IsAnimationPlaying())
                {
                    if (rollableDiceCollection.Count() != 0 && rollableDiceCollection.CalculateScore()?.Score is -1)
                    {
                        Farkle();
                    }
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

        if (gameStateLabel.Text != gameState.ToString())
        {
            gameStateLabel.Text = gameState.ToString();
        }
    }

    public void SetPlayerTurnLabel(bool gameOver = false)
    {
        playerTurnLabel.Text = gameOver ? "GameOver" : $"{activePlayerManager.GetWhoseTurnItIs()}'s turn";
    }

    public void StartGame()
    {
        playerInputLineEdit.Editable = false;
        playerInputLineEdit.Visible = false;
        gameStateManager.StartGame();
        regularPlayerManager.StartGame();
        activePlayerManager = regularPlayerManager;
        SetPlayerTurnLabel();
        BuildScoreText();
    }

    public void StartPlayerSetup()
    {
        gameStateManager.StartPlayerSetup();
        ClearFarkle();
        CreateDiceCollection();
        playerTurnLabel.Text = "Enter player 1 name";
        playerInputLineEdit.Edit();
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

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        HandleMouseInput(@event);
    }

    public void HandleMouseInput(InputEvent inputEvent)
    {
        if (gameStateManager.GetState == GameState.SelectDice)
        {
            if (inputEvent is InputEventMouseMotion mouseMotion)
            {
                mousePosition = mouseMotion.Position;
            }
            else if (inputEvent is InputEventMouseButton mouseButton)
            {
                var selectedDice = HandleMouseClickOnObject(
                    MouseTools.GetCollisionIdFromMouseClick(mousePosition, mouseButton, this));

                if (selectedDice is not null)
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
                }
            }
        }
    }

    public RootDice HandleMouseClickOnObject(ulong? objInstanceId)
    {
        if (objInstanceId == null)
        {
            return null;
        }
        var dice = rollableDiceCollection.GetDiceWithInstanceIdEqualTo(objInstanceId.Value);
        return dice;
    }

    public void HandleDicePhysics()
    {
        if (gameStateManager.GetState is GameState.RollReady)
        {
            rollableDiceCollection.diceList.ForEach(
                x => x.GlobalPosition = throwLocationBall.throwLocation.GlobalPosition);
        }
    }

    public void HandleSelectDiceState()
    {
        if (gameStateManager.GetSelectDiceSubstate is SelectDiceSubstate.Farkled)
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
                    BuildScoreText();
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
        else if (Input.IsActionJustPressed("enter") && TryRecordScore())
        {
            activePlayerManager.AddToPlayerScore(activePlayerManager.GetWhoseTurnItIs(), roundScore);
            roundScore = 0;
            ResetAllDice();
            if (TryAdvanceTurn())
            {
                BuildScoreText(onLastRound);
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

    public void HandleGameOverState()
    {
        if (Input.IsActionJustPressed("space"))
        {
            StartPlayerSetup();
        }
    }

    public void GameOver()
    {
        gameStateManager.GameOver();
        BuildScoreText(gameOver: true);
        SetPlayerTurnLabel(gameOver: true);
    }

    public bool TryAdvanceTurn()
    {
        if (onLastRound && !activePlayerManager.TryAdvanceTurnOnLastRound(out _))
        {
            return false;
        }
        if (BeginLastRoundIfPlayerAtMaxScore())
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
        if (selectedDiceCollection.HasUnusedScoreDice())
        {
            selectedDiceCollection.CalculateScoreResult.UnusedDice.ToList()
                .ForEach(d => d.FlashRed());
            return false;
        }
        scoreLabel.Text = "";
        roundScore += selectedDiceCollection.CalculateScoreResult.Score;
        BuildScoreText();
        return true;
    }

    public bool BeginLastRoundIfPlayerAtMaxScore()
    {
        if (!onLastRound && activePlayerManager.TryGetPlayerAtScore(ScoreToWin, out var playerScoreAtMaxScore))
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

    public bool TryProgressState()
    {
        if (gameStateManager.GetState is not GameState.GameOver)
        {
            gameStateManager.ProgressState();
            return true;
        }
        return false;
    }

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

    public void Farkle()
    {
        roundScore = 0;
        gameStateManager.Farkle();
        farkleLabel.Text = "You Farkled";
    }

    public void ClearFarkle()
    {
        gameStateManager.ClearFarkle();
        farkleLabel.Text = "";
    }

    public void CreateDiceCollection()
    {
        List<RootDice> tempDiceList = [];
        for(int i = 0; i < DiceAmount; i++)
        {
            var dice = packedRootDice.Instantiate<RootDice>();
            tempDiceList.Add(dice);
        }
        rollableDiceCollection = new DiceCollection(tempDiceList);
        rollableDiceCollection.SetParent(diceHolder);
        persistentDiceCollection = new DiceCollection(rollableDiceCollection.diceList);
    }

    public void SetDiceRotationForThrow()
    {
        foreach(RootDice dice in rollableDiceCollection.diceList)
        {
            dice.Rotate(HelperMethods.GetRandomVector3().Normalized(), GD.Randf()*(2*Mathf.Pi));
        }
    }

    public void SetDiceVelocityForThrow()
    {
        var baseVelocity = new Vector3(0, 0, -1) * 6;
        foreach(RootDice dice in rollableDiceCollection.diceList)
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

    public void ThrowDice()
    {
        rollableDiceCollection.TurnOn();
        rollableDiceCollection.ThrowDice();
    }

    public void MoveScoredDiceOffCamera()
    {
        scoredDiceCollection.diceList.ForEach(d => d.GlobalPosition = outOfPlayDiceLocation.GlobalPosition);
    }

    public void MoveRollableDiceOffCamera()
    {
        rollableDiceCollection.diceList.ForEach(d => d.GlobalPosition = outOfPlayDiceLocation.GlobalPosition);
    }

    public void BuildScoreText(bool gameOver = false)
    {
        var scoreString = "";
        foreach(var player in activePlayerManager.players)
        {
            scoreString += $"{player} total score = {activePlayerManager.playerScores[player]}\n";
        }
        if (gameOver)
        {
            scoreString += $"{lastRoundPlayerScore.Player} total score = {lastRoundPlayerScore.Score}\n";
        }
        scoreString += $"{activePlayerManager.GetWhoseTurnItIs()} is playing the current round.\n";
        scoreString += $"Round score = {roundScore}";

        scorePerRollLabel.Text = scoreString;
    }
}
