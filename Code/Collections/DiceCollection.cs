using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;

public class DiceCollection
{
    public ImmutableList<RootDice> diceList;
    public ScoreWithUnusedDice CalculateScoreResult { get => _calculatedScoreResult; }
    private ScoreWithUnusedDice _calculatedScoreResult = null;

    public static DiceCollection InstantiateDiceCollection(int size)
    {
        List<RootDice> tempList = [];
        for (int i = 0; i < size; i++)
        {
            tempList.Add(RootDice.InstantiateRootDice());
        }
        return new DiceCollection(tempList);
    }

    public static DiceCollection DeepCopyDiceCollection(DiceCollection dc) => DeepCopyDiceCollection(dc.diceList);

    public static DiceCollection DeepCopyDiceCollection(IEnumerable<RootDice> dice)
    {
        List<RootDice> tempList = [];
        foreach (var d in dice)
        {
            tempList.Add(d.DeepCopy());
        }
        return new DiceCollection(tempList);
    }

    public static List<RootDice> DeepCopyRootDiceEnumerable(IEnumerable<RootDice> dice)
    {
        List<RootDice> tempList = [];
        foreach (var d in dice)
        {
            tempList.Add(d.DeepCopy());
        }
        return tempList;
    }

    //Constructors
    public DiceCollection()
    {
        diceList = [];
    }

    public DiceCollection(IEnumerable<RootDice> rootDice)
    {
        if (!rootDice.Any()) { diceList = []; }
        else { diceList = [.. rootDice]; }
    }

    public DiceCollection(IEnumerable<DiceFace> diceFaces)
    {
        diceList = [.. diceFaces.Select(df => df.AssociatedDice)];
    }

    public DiceCollection(DiceCollection collection) => diceList = collection.diceList;


    //Immutable modifiers, return new DiceCollection
    public DiceCollection AddDice(RootDice dice)
    {
        List<RootDice> tempDiceList = !diceList.IsEmpty ? [.. diceList] : [];
        tempDiceList.Add(dice);
        return new DiceCollection(tempDiceList);
    }

    public DiceCollection AddDice(IEnumerable<RootDice> dice)
    {
        var tempDiceList = diceList.Concat(dice);
        return new DiceCollection(tempDiceList);
    }

    public DiceCollection AddDice(DiceCollection collection) => AddDice(collection.diceList);

    public DiceCollection RemoveDice(RootDice dice)
    {
        if (!diceList.Contains(dice)) { return this; }

        List<RootDice> tempDiceList = [.. diceList];
        tempDiceList.Remove(dice);
        return new DiceCollection(tempDiceList);
    }

    public DiceCollection RemoveDice(IEnumerable<RootDice> dice) => new([.. diceList.Where(d => !dice.Contains(d))]);
    public DiceCollection RemoveDice(DiceCollection dc) => RemoveDice(dc.diceList);
    private void DeleteAllDice()
    {
        diceList.ForEach(d => d.QueueFree());
        diceList = [];
    }

    public void DeleteAllDiceAndReplaceWithDeepCopy(DiceCollection dc)
    {
        DeleteAllDice();
        _calculatedScoreResult = null;
        diceList = [.. DeepCopyRootDiceEnumerable(dc.diceList)];
    }

    public void DeleteAllTemporaryDice()
    {
        if(!diceList.Any(d => d.temporary)) { return; }
        List<RootDice> temporaryDice = [.. diceList.Where(d => d.temporary == true)];
        diceList = [.. diceList.Where(d => d.temporary == false)];
        temporaryDice.ForEach(d => d.QueueFree());        
    }


    public bool HasUnusedScoreDice() => _calculatedScoreResult.UnusedDice.Count() > 0;

    public int Count()
    {
        if (diceList == null || diceList.IsEmpty)
        {
            return 0;
        }
        else
        {
            return diceList.Count;
        }
    }

    public bool PointTooClose(Vector3 point, float margin)
    {
        foreach (RootDice dice in diceList)
        {
            if (dice.PointTooClose(point, margin))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsDoneRolling()
    {
        return diceList.All(x => x.IsDoneRolling() == true);
    }

    public void SetGlobalPosition(Vector3 globalPosition)
    {
        diceList.ForEach(x => x.GlobalPosition = globalPosition);
    }

    public void ChangeParent(Node newParent, bool changeGlobalTransform)
    {
        diceList.ForEach(x => x.Reparent(newParent, changeGlobalTransform));
    }

    public void SetParent(Node newParent)
    {
        diceList.ForEach(x => newParent.AddChild(x));
    }

    public void SetVelocityForThrow()
    {
        var baseVelocity = new Vector3(0, 0, -1) * 5;
        foreach (RootDice dice in diceList)
        {
            dice.SetVelocityUponThrow(HelperMethods.FuzzyUpVector3(baseVelocity, 0.5f));
        }
    }

    public void MultiplyVelocityByThrowForceValue(double throwForceValue)
    {
        foreach (RootDice dice in diceList)
        {
            dice.MultiplyVelocityByThrowForceValue(throwForceValue);
        }
    }

    public void ThrowDice()
    {
        diceList.ForEach(x => x.Throw());
    }

    public void TurnOn()
    {
        diceList.ForEach(x => x.TurnOn());
    }

    public void TurnOff()
    {
        diceList.ForEach(x => x.TurnOff());
    }

    public void FreezeDice()
    {
        diceList.ForEach(x => x.Freeze = true);
    }

    public void UnfreezeDice()
    {
        diceList.ForEach(x => x.Freeze = false);
    }

    public void EnableCollision() => diceList.ForEach(x => x.EnableCollision());

    public void DisableCollision() => diceList.ForEach(x => x.DisableCollision());

    public void ResetPosition() => diceList.ForEach(x => x.Position = Vector3.Zero);

    public bool TryGetDiceWithInstanceIdEqualTo(ulong objInstanceId, out RootDice selectedDice)
    {
        foreach (RootDice diceObj in diceList)
        {
            if (diceObj.GetInstanceId() == objInstanceId
                || diceObj.GetDiceFaceInstanceIds().Contains(objInstanceId))
            {
                selectedDice = diceObj;
                return true;
            }
        }

        selectedDice = null;
        return false;
    }

    public IEnumerable<DiceFace> GetResultOfRoll() => diceList.Select(d => d.GetResultOfRoll()).ToList();
    public void FlashRed() => diceList.ForEach(d => d.FlashRed());
    public void EndOverrides() => diceList.ForEach(d => d.EndOverride());
    public void Unselect() => diceList.ForEach(d => d.UnselectDice());
}