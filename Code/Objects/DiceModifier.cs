using System;
using System.Collections.Generic;
using System.Linq;

public abstract class DiceModifier
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public RootDice ModifiedDice { get; set; }
    public virtual void AddModifierToDice(RootDice dice)
    {
        ModifiedDice = dice;
        dice.AddModifier(this);
    }
    public virtual void RemoveModifierFromDice()
    {
        ModifiedDice.RemoveModifier(this);
        ModifiedDice = null;
    }
    public abstract void ApplyModifier();
    public abstract void ReleaseModifier();
}

public class Add1ToAllDiceFacesModifier : DiceModifier
{
    public List<DiceFaceModifier> diceFaceModifiers = [];

    public override void AddModifierToDice(RootDice dice)
    {
        base.AddModifierToDice(dice);
        foreach (var face in dice.diceFaceCollection.faces)
        {
            var modifier = new AddToDiceFaceModifier
            {
                Id = Guid.NewGuid(),
                Name = $"{Name}_child",
                Description = $"Adds 1 to dice faces of dice with modifier Add1ToAllDiceFacesModifier",
                NumberToAdd = 1
            };
            diceFaceModifiers.Add(modifier);
            modifier.AddModifierToDiceFace(face);
        }
        ApplyModifier();
    }

    public override void RemoveModifierFromDice()
    {
        ReleaseModifier();
        base.RemoveModifierFromDice();
    }

    public override void ApplyModifier()
    {
        foreach (var modifier in diceFaceModifiers)
        {
            modifier.ApplyModifier();
        }
    }

    public override void ReleaseModifier()
    {
        foreach (var modifier in diceFaceModifiers)
        {
            modifier.ReleaseModifier();
        }
    }
}