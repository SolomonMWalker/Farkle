using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Godot;

public class DiceCollection
{
    public ImmutableList<RootDice> diceList;
    public ScoreWithUnusedDice CalculateScoreResult { get => _calculatedScoreResult; }
    private ScoreWithUnusedDice _calculatedScoreResult = null;

    //Constructors
    public DiceCollection()
    {
        diceList = [];
    }

    public DiceCollection(IEnumerable<RootDice> rootDice)
    {
        if (rootDice.Count() == 0) { diceList = []; }
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

    //Scoring methods
    // public ScoreWithUnusedDice CalculateScore()
    // {
    //     if (diceList.IsEmpty) { return new(-1, null); }
    //     _calculatedScoreResult = DiceCollectionScore.CalculateScore(this);
    //     return _calculatedScoreResult;
    // }

    //Get properties of Dice
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

    public void SetDebug(bool isDebug)
    {
        diceList.ForEach(x => x.SetDebug(isDebug));
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