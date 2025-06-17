using Godot;
using System;
using System.Collections.Generic;

public class UiManager
{
    public DebugMenu DebugMenu { get; private set; }
    public Control UiParent { get; private set; }
    public Label InstructionLabel { get; private set; }
    public Label ScoreLabel { get; private set; }
    public Label FarkleLabel { get; private set; }
    public Label PlayerTurnLabel { get; private set; }
    public Label LastRoundLabel { get; private set; }
    public RichTextLabel ScorePerRollLabel { get; private set; }
    public LineEdit PlayerInputLineEdit { get; private set; }

    public UiManager(Control uiParent)
    {
        UiParent = uiParent;
        InstructionLabel = UiParent.GetChildByName<Label>("Instructions");
        ScoreLabel = UiParent.GetChildByName<Label>("Score");
        FarkleLabel = UiParent.GetChildByName<Label>("FarkleLabel");
        PlayerTurnLabel = UiParent.GetChildByName<Label>("PlayerTurn");
        LastRoundLabel = UiParent.GetChildByName<Label>("OnLastRound");
        ScorePerRollLabel = UiParent.GetChildByName<RichTextLabel>("ScorePerRoll");
        PlayerInputLineEdit = UiParent.GetChildByName<LineEdit>("PlayerInput");
    }

    public void SetPlayerTurnLabel(string thisTurnsPlayer, bool gameOver = false)
    {
        PlayerTurnLabel.Text = gameOver ? "GameOver, press space to play again"
            : $"{thisTurnsPlayer}'s turn";
    }

    public void BuildAndSetScoreText(List<PlayerScore> playerScores, string currentPlayer, int currentRoundScore)
    {
        var scoreString = "";
        foreach (var playerScore in playerScores)
        {
            scoreString += $"{playerScore.Player} total score = {playerScore.Score}\n";
        }
        scoreString += $"{currentPlayer} is playing the current round.\n";
        scoreString += $"Round score = {currentRoundScore}";

        ScorePerRollLabel.Text = scoreString;
    }

    public void SetInstructionLabel(GameState gameState)
    {
        InstructionLabel.Text = gameState switch
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
}