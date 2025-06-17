using System.Collections.Generic;
using Godot;

public partial class GameController : Node3D
{
    #region Properties

    private const string DebugMenuRelPath = "res://Scenes/Debug/DebugMenu.tscn";

    public DebugMenu DebugMenu { get; private set; }
    private CameraController cameraController;
    public GameStateManager GameStateManager { get; private set; }
    public PlayerManager activePlayerManager, regularPlayerManager, lastRoundPlayerManager;
    private ThrowLocationBall throwLocationBall;
    public DiceManager DiceManager { get; private set; }
    public UiManager UiManager { get; private set; }
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
        cameraController = this.GetChildByName<CameraController>("CameraController");
        throwLocationBall = FindChild("DiceTable").GetChildByName<ThrowLocationBall>("ThrowLocationBall");
        UiManager = new UiManager(this.GetChildByName<Control>("ControlParent"));
        GameStateManager = new GameStateManager();
        DiceManager = new DiceManager(this.GetChildByName<Node3D>("DiceHolder"),
            this.GetChildByName<Node3D>("OutOfPlayDiceLocation"),
            throwLocationBall.throwLocation,
            UiManager.ScoreLabel);

        UiManager.PlayerInputLineEdit.TextSubmitted += HandlePlayerNameSubmitted;
        UiManager.PlayerInputLineEdit.KeepEditingOnTextSubmit = true;

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
            DiceManager.ReadyDiceForThrow,
        ]);
        GameStateManager.AddOnStateEnterOrExitAction(enter: true, GameState.RollReady, [
            throwLocationBall.Animate,
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
                    List<PlayerScore> playerScores = [];
                    foreach (var playerScore in activePlayerManager.playerScores)
                    {
                        playerScores.Add(new PlayerScore(playerScore.Key, playerScore.Value));
                    }
                    var currentPlayer = activePlayerManager.players[activePlayerManager.currentPlayerTurnIndex];
                    UiManager.BuildAndSetScoreText(playerScores, currentPlayer, roundScore);
                    UiManager.SetPlayerTurnLabel(currentPlayer);
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
                    List<PlayerScore> playerScores = [];
                    foreach (var playerScore in activePlayerManager.playerScores)
                    {
                        playerScores.Add(new PlayerScore(playerScore.Key, playerScore.Value));
                    }
                    var currentPlayer = activePlayerManager.players[activePlayerManager.currentPlayerTurnIndex];
                    UiManager.BuildAndSetScoreText(playerScores, currentPlayer, roundScore);
                    UiManager.SetPlayerTurnLabel(currentPlayer);
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
            UiManager.SetInstructionLabel(GameStateManager.GameState);
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
                    UiManager.ScoreLabel.Text = DiceManager.SelectedDiceCollection.CalculateScore().Score.ToString();

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
            UiManager.PlayerInputLineEdit.Editable = true;
            UiManager.PlayerInputLineEdit.Visible = true;
            UiManager.SetInstructionLabel(GameState.PlayerSetup);
            UiManager.PlayerTurnLabel.Text = "Enter player 1 name";
            UiManager.PlayerInputLineEdit.Edit();
        }
    }
    public void HandlePlayerNameSubmitted(string newText)
    {
        if (regularPlayerManager is null)
        {
            regularPlayerManager = new PlayerManager(newText);
            UiManager.PlayerInputLineEdit.Clear();
            UiManager.PlayerTurnLabel.Text = "Enter player 2 name";
        }
        else
        {
            regularPlayerManager.AddPlayer(newText);
            UiManager.PlayerInputLineEdit.Clear();
            StartGame();
        }
    }

    public void StartGame()
    {
        UiManager.PlayerInputLineEdit.Editable = false;
        UiManager.PlayerInputLineEdit.Visible = false;
        regularPlayerManager.StartGame();
        activePlayerManager = regularPlayerManager;
        List<PlayerScore> playerScores = [];
        foreach (var playerScore in activePlayerManager.playerScores)
        {
            playerScores.Add(new PlayerScore(playerScore.Key, playerScore.Value));
        }
        var currentPlayer = activePlayerManager.players[activePlayerManager.currentPlayerTurnIndex];
        UiManager.BuildAndSetScoreText(playerScores, currentPlayer, roundScore);
        UiManager.SetPlayerTurnLabel(currentPlayer);
        UiManager.SetInstructionLabel(GameState.PreRoll);
        GameStateManager.StartGame();
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
            UiManager.ScoreLabel.Text = "";
            roundScore += tempScore;
            List<PlayerScore> playerScores = [];
            foreach (var playerScore in activePlayerManager.playerScores)
            {
                playerScores.Add(new PlayerScore(playerScore.Key, playerScore.Value));
            }
            var currentPlayer = activePlayerManager.players[activePlayerManager.currentPlayerTurnIndex];
            UiManager.BuildAndSetScoreText(playerScores, currentPlayer, roundScore);
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
            UiManager.LastRoundLabel.Text = $"On last round with {lastRoundPlayerScore.Player} leading!";
            lastRoundPlayerManager = regularPlayerManager.GetLastRoundPlayerManager(playerScoreAtMaxScore.Player);
            activePlayerManager = lastRoundPlayerManager;
            return true;
        }
        return false;
    }

    public void GameOver()
    {
        GameStateManager.GameOver();
        List<PlayerScore> playerScores = [];
        foreach (var playerScore in activePlayerManager.playerScores)
        {
            playerScores.Add(new PlayerScore(playerScore.Key, playerScore.Value));
        }
        var currentPlayer = activePlayerManager.players[activePlayerManager.currentPlayerTurnIndex];
        UiManager.BuildAndSetScoreText(playerScores, currentPlayer, roundScore);
        UiManager.SetPlayerTurnLabel(currentPlayer, gameOver: true);
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
        UiManager.FarkleLabel.Text = "You Farkled";
    }

    public void ClearFarkle()
    {
        GameStateManager.ClearFarkle();
        UiManager.FarkleLabel.Text = "";
    }

    #endregion

    #region Dice
    public void ThrowDice()
    {
        DiceManager.ThrowDice();
        throwLocationBall.StopAnimation();
        cameraController.MoveToDiceZoomLocation();
    }

    public void RescoreSelectedDice()
    {
        DiceManager.RescoreSelectedDice();
        UiManager.ScoreLabel.Text = DiceManager.SelectedDiceCollection.CalculateScore().Score.ToString();
    }

    #endregion
}
