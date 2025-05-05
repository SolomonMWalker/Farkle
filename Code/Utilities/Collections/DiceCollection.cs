using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;

public class DiceCollection{
    public ImmutableList<RootDice> diceList;

    public DiceCollection()
    {
        diceList = [];
    }

    public DiceCollection(IEnumerable<RootDice> rootDice)
    {
        diceList = rootDice.ToImmutableList();
    }

    public DiceCollection(IEnumerable<DiceFace> diceFaces)
    {
        diceList = diceFaces.Select(df => df.AssociatedDice).ToImmutableList();
    }

    public DiceCollection AddDice(RootDice dice)
    {
        var tempDiceList = diceList.Count > 0 ? diceList.ToList() : [];
        tempDiceList.Add(dice);
        return new DiceCollection(tempDiceList);
    }

    public DiceCollection RemoveDice(RootDice dice)
    {
        if(!diceList.Contains(dice)){return this;}

        var tempDiceList = diceList.ToList();
        tempDiceList.Remove(dice);
        return new DiceCollection(tempDiceList);
    }
    public DiceCollection RemoveDice(IEnumerable<RootDice> dice){
        return new DiceCollection(diceList.Where(d => !dice.Contains(d)).ToList());
    }
    public DiceCollection RemoveDice(DiceCollection dc) => RemoveDice(dc.diceList);

    public CalculateScoreResult CalculateScore()
    {
        if(diceList.IsEmpty){return new (-1, null);}
        return DiceCollectionScore.CalculateScore(this);
    }

    public bool PointTooClose(Vector3 point, float margin)
    {
        foreach(RootDice dice in diceList)
        {
            if(dice.PointTooClose(point, margin))
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

    public RootDice GetDiceWithInstanceIdEqualTo(ulong objInstanceId)
    {
        foreach(RootDice diceObj in diceList)
        {
            if(diceObj.GetInstanceId() == objInstanceId)
            {
                return diceObj;
            }
        }

        return null;
    }

    public IEnumerable<DiceFace> GetResultOfRoll() => diceList.Select(d => d.GetResultOfRoll()).ToList();
}