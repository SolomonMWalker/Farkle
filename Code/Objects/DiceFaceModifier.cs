using System;

public abstract class DiceFaceModifier
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DiceFace ModifiedDiceFace { get; set; }
    public virtual void AddModifierToDiceFace(DiceFace diceFace)
    {
        diceFace.AddModifier(this);
    }
    public virtual void RemoveModifierFromDiceFace()
    {
        ModifiedDiceFace.RemoveModifier(this);
        ModifiedDiceFace = null;
    }
    public abstract void ApplyModifier();
    public abstract void ReleaseModifier();
}

public class AddToDiceFaceModifier : DiceFaceModifier
{
    public int NumberToAdd { get; set; }

    public override void ApplyModifier()
    {
        ModifiedDiceFace.ModifiedDiceFaceValue += NumberToAdd;
    }

    public override void ReleaseModifier()
    {
        ModifiedDiceFace.ModifiedDiceFaceValue -= NumberToAdd;
    }
}