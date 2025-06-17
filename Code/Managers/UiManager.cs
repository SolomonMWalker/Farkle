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

    public void BuildAndSetScoreText(int selectedDiceScore, int currentRoundScore, int currentStageScore)
    {
        ScoreLabel.Text =
        $"""
        Stage score = {currentStageScore}
        Round score = {currentRoundScore}
        Selected dice score = {selectedDiceScore}
        """;
    }

    public void SetInstructionLabel(GameState gameState)
    {
        InstructionLabel.Text = gameState switch
        {
            GameState.PreRoll => "Press space to find rolling position.",
            GameState.RollReady => "Press space to select rolling position and roll dice.",
            GameState.Rolling => "Wait for dice to finish rolling.",
            GameState.SelectDice => "Select dice with the mouse. Press space to enter your score and roll again. Press enter to submit your score. If all six die are scored, you must press space to roll again.",
            GameState.GameOver => "Press space to play a new game.",
            _ => ""
        };
    }

    public void Farkle() => FarkleLabel.Text = "You Farkled";
    public void ClearFarkle() => FarkleLabel.Text = "";
}