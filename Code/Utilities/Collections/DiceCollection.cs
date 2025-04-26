using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;

public class DiceCollection{
    public List<RootDice> diceList;

    public DiceCollection()
    {
        diceList = new List<RootDice>();
    }

    public DiceCollection(List<RootDice> rootDice)
    {
        diceList = rootDice;
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

    public RootDice GetDiceEqualTo(GodotObject obj)
    {
        foreach(RootDice diceObj in diceList)
        {
            if(diceObj.Equals(obj))
            {
                return diceObj;
            }
        }

        return null;
    }

    public List<int> GetResultOfRoll()
    {
        var returnList = new List<int>();
        diceList.ForEach(x => returnList.Add(x.GetResultOfRoll().number));
        return returnList;
    }
}

public enum DiceCollectionState{
    NotRolling,
    Rolling
}