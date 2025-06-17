using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class DiceManager
{
    private const string RootDiceRelPath = "res://Scenes/root_dice.tscn";

    public DiceCollection PersistentDiceCollection { get; private set; } = new();
    public DiceCollection RollableDiceCollection { get; private set; } = new();
    public DiceCollection SelectedDiceCollection { get; private set; } = new();
    public DiceCollection ScoredDiceCollection { get; private set; } = new();

    private Label scoreLabel;
    private Node3D diceHolder, outOfPlayDiceLocation, throwLocationNode;
    private PackedScene packedRootDice;

    public DiceManager(Node3D diceHolder, Node3D outOfPlayDiceLocation, Node3D throwLocationNode, Label scoreLabel)
    {
        this.diceHolder = diceHolder;
        this.outOfPlayDiceLocation = outOfPlayDiceLocation;
        this.throwLocationNode = throwLocationNode;
        this.scoreLabel = scoreLabel;
        packedRootDice = GD.Load<PackedScene>(RootDiceRelPath);
        CreateDiceCollection();
    }

    public void CreateDiceCollection()
    {
        List<RootDice> tempDiceList = [];
        for (int i = 0; i < Configuration.ConfigValues.NumOfStartingDice; i++)
        {
            var dice = packedRootDice.Instantiate<RootDice>();
            tempDiceList.Add(dice);
        }
        PersistentDiceCollection = new DiceCollection(tempDiceList);
        PersistentDiceCollection.SetParent(diceHolder);
        PersistentDiceCollection.SetDebug(Configuration.ConfigValues.IsDebug);
        RollableDiceCollection = new DiceCollection(PersistentDiceCollection);
    }

    public void ResetAllDice()
    {
        PersistentDiceCollection.diceList.ForEach(d => d.UnselectDice());
        RollableDiceCollection = new DiceCollection(PersistentDiceCollection);
        ScoredDiceCollection = new DiceCollection();
        SelectedDiceCollection = new DiceCollection();
        MoveRollableDiceOffCamera();
    }

    public void ResetUnscoredDice()
    {
        RollableDiceCollection = RollableDiceCollection.RemoveDice(SelectedDiceCollection);

        ScoredDiceCollection = ScoredDiceCollection.AddDice(SelectedDiceCollection);
        ScoredDiceCollection.TurnOff();
        ScoredDiceCollection.diceList.ForEach(d => d.UnselectDice());
        MoveScoredDiceOffCamera();
        MoveRollableDiceOffCamera();
        SelectedDiceCollection = new DiceCollection();
    }    

    public void SetDiceRotationForThrow()
    {
        foreach (RootDice dice in RollableDiceCollection.diceList)
        {
            dice.Rotate(HelperMethods.GetRandomVector3().Normalized(), GD.Randf() * (2 * Mathf.Pi));
        }
    }

    public void SetDiceVelocityForThrow()
    {
        var baseVelocity = new Vector3(0, 0, -1) * 6;
        foreach (RootDice dice in RollableDiceCollection.diceList)
        {
            dice.SetVelocityUponThrow(HelperMethods.FuzzyUpVector3(baseVelocity, 0.5f));
        }
    }

    public void ReadyDiceForRoll() //turn off dice, set rotation and velocity
    {
        RollableDiceCollection.TurnOff();
        SetDiceRotationForThrow();
        SetDiceVelocityForThrow();
    }

    public void ThrowDice()
    {
        MoveRollableDiceToThrowLocation();
        RollableDiceCollection.TurnOn();
        RollableDiceCollection.ThrowDice();
        //throwLocationBall.StopAnimation();
        //cameraController.MoveToDiceZoomLocation();
    }

    public bool TryGetSelectedDiceScore(out int score)
    {
        score = 0;
        if (SelectedDiceCollection.Count() == 0)
        {
            //Do nothing, maybe tell the user to select some scoring dice
            return false;
        }

        SelectedDiceCollection.CalculateScore();
        if (SelectedDiceCollection.HasUnusedScoreDice() ||
            SelectedDiceCollection.CalculateScoreResult.Score == -1)
        {
            SelectedDiceCollection.CalculateScoreResult.UnusedDice.ToList()
                .ForEach(d => d.FlashRed());
            return false;
        }
        scoreLabel.Text = "";
        score = SelectedDiceCollection.CalculateScoreResult.Score;
        return true;
    }

    public void MoveRollableDiceToThrowLocation()
    {
        RollableDiceCollection.SetGlobalPosition(throwLocationNode.GlobalPosition);
    }

    public void MoveScoredDiceOffCamera()
    {
        ScoredDiceCollection.SetGlobalPosition(outOfPlayDiceLocation.GlobalPosition);
    }

    public void MoveRollableDiceOffCamera()
    {
        RollableDiceCollection.SetGlobalPosition(outOfPlayDiceLocation.GlobalPosition);
    }

    public bool TryHandleMouseButtonInputForDiceSelect(InputEventMouseButton mouseButtonEvent)
    {
        var clickedOnObjectInstanceId = MouseTools.GetCollisionIdFromMouseClick(mouseButtonEvent.Position, mouseButtonEvent, diceHolder);
        if (clickedOnObjectInstanceId is not null && TryGetRollableDiceWithInstanceId(clickedOnObjectInstanceId, out var clickedDice))
        {
            if (SelectedDiceCollection.diceList.Contains(clickedDice))
            {
                SelectedDiceCollection = SelectedDiceCollection.RemoveDice(clickedDice);
                clickedDice.ToggleSelectDice();
                return true;
            }
            else
            {
                SelectedDiceCollection = SelectedDiceCollection.AddDice(clickedDice);
                clickedDice.ToggleSelectDice();
                return true;
            }
        }
        return false;
    }

    public bool TryGetRollableDiceWithInstanceId(ulong? objInstanceId, out RootDice clickedDice)
    {
        if (objInstanceId != null &&
            RollableDiceCollection.TryGetDiceWithInstanceIdEqualTo(objInstanceId.Value, out var selectedDice))
        {
            clickedDice = selectedDice;
            return true;
        }
        clickedDice = null;
        return false;
    }
    
    #region Debug

    public void RescoreSelectedDice()
    {
        if (Configuration.ConfigValues.IsDebug)
        {
            SelectedDiceCollection.CalculateScore();
        }
    }

    public void EndOverride() => PersistentDiceCollection.EndOverrides();

    #endregion
}