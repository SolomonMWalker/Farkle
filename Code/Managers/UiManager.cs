using Godot;
using System;
using System.Collections.Generic;

public class UiManager
{
    public DebugMenu DebugMenu { get; private set; }
    public Control UiParent { get; private set; }
    public Label InstructionLabel { get; private set; }
    public Label FarkleLabel { get; private set; }
    public Label LastRoundLabel { get; private set; }
    public RichTextLabel ScoreLabel { get; private set; }
    public UiManager(Control uiParent)
    {
        UiParent = uiParent;
        InstructionLabel = UiParent.GetChildByName<Label>("Instructions");
        FarkleLabel = UiParent.GetChildByName<Label>("FarkleLabel");
        LastRoundLabel = UiParent.GetChildByName<Label>("OnLastRound");
        ScoreLabel = UiParent.GetChildByName<RichTextLabel>("Score");
    }

    public void BuildAndSetScoreText(int selectedDiceScore, int currentRoundScore, int roundAttemptsLeft, int currentStageScore, int currentStageScoreToWin,
    int totalStages, int stageNumber)
    {
        ScoreLabel.Text =
        $"""
        Total Stages = {totalStages}
        Stage number = {stageNumber}
        Stage score to win = {currentStageScoreToWin}
        Stage score = {currentStageScore}
        Round Attempts Left = {roundAttemptsLeft}
        Round score = {currentRoundScore}
        Selected dice score = {selectedDiceScore}
        """;
    }

    public void SetInstructionLabel(GameState gameState, SelectDiceSubstate selectDiceSubstate, string extraMessage = null)
    {
        InstructionLabel.Text = (gameState, selectDiceSubstate) switch
        {
            (GameState.Instantiated, _) => "Press space to start game.",
            (GameState.PreRoll, _) => "Press space to find rolling position.",
            (GameState.RollReady, _) => "Press space to select rolling position and roll dice.",
            (GameState.Rolling, _) => "Wait for dice to finish rolling.",
            (GameState.SelectDice, SelectDiceSubstate.SelectingDice) => "Select dice with the mouse. Press space to enter your score and roll again. Press enter to submit your score. If all six die are scored, you must press space to roll again.",
            (GameState.SelectDice, SelectDiceSubstate.Farkled) => "You Farkled. Press Space to Continue.",
            (GameState.GameOver, _) => "Game Over! Press space to play a new game.",
            _ => ""
        };

        if (extraMessage != null)
        {
            InstructionLabel.Text = InstructionLabel.Text + $"\n{extraMessage}";
        }
    }

    public void Farkle() => FarkleLabel.Text = "You Farkled";
    public void ClearFarkle() => FarkleLabel.Text = "";
}