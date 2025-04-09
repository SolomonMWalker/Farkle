using System.Collections.Generic;
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
}