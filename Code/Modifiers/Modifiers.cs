public interface IModifier
{
    void ApplyModifier();
    void ReleaseModifier();
}

public abstract class DiceModifier : IModifier
{
    public RootDice Dice { get; set; }
    public abstract void ApplyModifier();
    public abstract void ReleaseModifier();
}

public abstract class DiceFaceModifier : IModifier
{
    public DiceFace DiceFace { get; set; }
    public abstract void ApplyModifier();
    public abstract void ReleaseModifier();
}

public abstract class TableModifier : IModifier
{
    public abstract void ApplyModifier();
    public abstract void ReleaseModifier();
}

public abstract class PlayerModifier : IModifier
{
    public abstract void ApplyModifier();
    public abstract void ReleaseModifier();
}