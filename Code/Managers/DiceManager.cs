using Godot;
using System.Collections.Generic;
using System.Linq;

public class DiceManager
{
    private Dictionary<string, DiceCollection> DiceCollections { get; set; } = [];
    public DiceCollection RollableDiceCollection
    {
        get => DiceCollections["Rollable"];
        private set => DiceCollections["Rollable"] = value;
    }
    public DiceCollection SelectedDiceCollection
    {
        get => DiceCollections["Selected"];
        private set => DiceCollections["Selected"] = value;
    }
    public DiceCollection ScoredDiceCollection
    {
        get => DiceCollections["Scored"];
        private set => DiceCollections["Scored"] = value;
    }
    private DiceCollection LeftOnTableDiceCollection
    {
        get => DiceCollections["LeftOnTable"];
        set => DiceCollections["LeftOnTable"] = value;
    }
    private PlayerManager PlayerManager { get; set; }
    private Node3D diceHolder, outOfPlayDiceLocation, throwLocationNode;

    public DiceManager(PlayerManager playerManager, Node3D diceHolder, Node3D outOfPlayDiceLocation, Node3D throwLocationNode)
    {
        DiceCollections.Add("Rollable", new DiceCollection());
        DiceCollections.Add("Selected", new DiceCollection());
        DiceCollections.Add("Scored", new DiceCollection());
        DiceCollections.Add("LeftOnTable", new DiceCollection());
        PlayerManager = playerManager;
        this.diceHolder = diceHolder;
        this.outOfPlayDiceLocation = outOfPlayDiceLocation;
        this.throwLocationNode = throwLocationNode;
        InstantiateRollableDiceFromPlayerDice();
    }

    public void InstantiateRollableDiceFromPlayerDice()
    {
        RollableDiceCollection = DiceCollection.DeepCopyDiceCollection(PlayerManager.DiceCollection);
        RollableDiceCollection.SetParent(diceHolder);
        MoveRollableDiceOffCamera();
    }

    public void ResetAllDice()
    {
        RollableDiceCollection = DeleteTempDiceAndReturnCollectionOfPermanentDice();
        ScoredDiceCollection = new DiceCollection();
        SelectedDiceCollection = new DiceCollection();
        MoveRollableDiceOffCamera();
    }

    public void ResetUnscoredDice()
    {
        RollableDiceCollection = RollableDiceCollection.RemoveDice(SelectedDiceCollection);
        ScoredDiceCollection = ScoredDiceCollection.AddDice(SelectedDiceCollection);
        ScoredDiceCollection.TurnOff();
        ScoredDiceCollection.Unselect();
        MoveScoredDiceOffCamera();
        MoveRollableDiceOffCamera();
        SelectedDiceCollection = new DiceCollection();
    }

    public void RerollSelectedDice()
    {
        UnselectAllDice();
        LeftOnTableDiceCollection = RollableDiceCollection.RemoveDice(SelectedDiceCollection);
        RollableDiceCollection = SelectedDiceCollection;
        SelectedDiceCollection = new DiceCollection();
        MoveRollableDiceOffCamera();
    }

    public void RerollAllDice()
    {
        UnselectAllDice();
        SelectedDiceCollection = new DiceCollection();
        MoveRollableDiceOffCamera();
    }

    public void AddDiceLeftOnTableToRollableDiceCollection()
    {
        RollableDiceCollection = RollableDiceCollection.AddDice(LeftOnTableDiceCollection);
        LeftOnTableDiceCollection = new DiceCollection();
    }

    public void SetDiceRotationForThrow(DiceCollection dc = null)
    {
        dc ??= RollableDiceCollection;
        foreach (RootDice dice in dc.diceList)
        {
            dice.Rotate(HelperMethods.GetRandomVector3().Normalized(), GD.Randf() * (2 * Mathf.Pi));
        }
    }

    public void SetDiceVelocityForThrow(DiceCollection dc = null)
    {
        dc ??= RollableDiceCollection;
        dc.SetVelocityForThrow();
    }

    public void MultiplyVelocityByThrowForceValue(double throwForce, DiceCollection dc = null)
    {
        dc ??= RollableDiceCollection;
        dc.MultiplyVelocityByThrowForceValue(throwForce);
    }

    private void ReadyDiceCollectionForThrow(DiceCollection dc)
    {
        dc.TurnOff();
        SetDiceRotationForThrow(dc);
        SetDiceVelocityForThrow(dc);
    }

    public void ReadyRollableDiceForThrow() //turn off dice, set rotation and velocity
    {
        ReadyDiceCollectionForThrow(RollableDiceCollection);
    }

    public void ThrowDice()
    {
        MoveDiceCollectionToThrowLocation(RollableDiceCollection);
        RollableDiceCollection.TurnOn();
        RollableDiceCollection.ThrowDice();
    }

    private void MoveDiceCollectionToThrowLocation(DiceCollection dc)
    {
        dc.SetGlobalPosition(throwLocationNode.GlobalPosition);
    }

    public void MoveRollableDiceToThrowLocation()
    {
        MoveDiceCollectionToThrowLocation(RollableDiceCollection);
    }

    private void MoveDiceCollectionOffCamera(DiceCollection dc)
    {
        dc.SetGlobalPosition(outOfPlayDiceLocation.GlobalPosition);
    }

    public void MoveScoredDiceOffCamera()
    {
        MoveDiceCollectionOffCamera(ScoredDiceCollection);
    }

    public void MoveRollableDiceOffCamera()
    {
        MoveDiceCollectionOffCamera(RollableDiceCollection);
    }

    public bool TryHandleMouseButtonInputForDiceSelect(InputEventMouseButton mouseButtonEvent)
    {
        var clickedOnObjectInstanceId = HelperMethods.GetCollisionIdFromMouseClick(mouseButtonEvent, diceHolder);
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

    private static bool TryGetDiceInCollectionWithInstanceId(ulong? objInstanceId, out RootDice clickedDice, DiceCollection dc)
    {
        if (objInstanceId != null &&
            dc.TryGetDiceWithInstanceIdEqualTo(objInstanceId.Value, out var selectedDice))
        {
            clickedDice = selectedDice;
            return true;
        }
        clickedDice = null;
        return false;
    }

    public bool TryGetRollableDiceWithInstanceId(ulong? objInstanceId, out RootDice clickedDice)
    {
        bool x = TryGetDiceInCollectionWithInstanceId(objInstanceId, out var dice, RollableDiceCollection);
        clickedDice = dice;
        return x;
    }

    public bool AreAllDiceBeingSubmittedForScore()
    {
        if (ScoredDiceCollection.Count() + SelectedDiceCollection.Count() >= PlayerManager.DiceCollection.Count())
        {
            return true;
        }
        return false;
    }

    private void DeleteAllTemporaryDice() => DiceCollections.Values.ToList().ForEach(dc => dc.DeleteAllTemporaryDice());

    private DiceCollection DeleteTempDiceAndReturnCollectionOfPermanentDice()
    {
        DeleteAllTemporaryDice();
        List<RootDice> permanentDice = [];
        foreach (var pair in DiceCollections)
        {
            foreach (var d in pair.Value.diceList)
            {
                permanentDice.Add(d);
            }
        }
        return new DiceCollection(permanentDice);
    }

    public void UnselectAllDice() => DiceCollections.Values.ToList().ForEach(dc => dc.Unselect());

    #region Debug

    public void EndOverride() => DiceCollections.Values.ToList().ForEach(dc => dc.EndOverrides());

    #endregion
}