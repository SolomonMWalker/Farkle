public enum MathModificationType
{
    Add,
    Multiply,
    Set
}

public interface IIntModification
{
    public MathModificationType MathModificationType { get; }
    public int ModNumber { get; }
    public int ApplyModification(int originalValue);
    public int RemoveModification(int modifiedValue);
}

public class AddIntModification(int NumberToAdd) : IIntModification
{
    public MathModificationType MathModificationType { get; } = MathModificationType.Add;
    public int ModNumber { get; } = NumberToAdd;
    public int ApplyModification(int originalValue) => originalValue + ModNumber;
    public int RemoveModification(int modifiedValue) => modifiedValue - ModNumber;
}

public class MultiplyIntModification(int NumberToMultiply) : IIntModification
{
    public MathModificationType MathModificationType { get; } = MathModificationType.Multiply;
    public int ModNumber { get; } = NumberToMultiply;
    public int ApplyModification(int originalValue) => originalValue * ModNumber;
    public int RemoveModification(int modifiedValue) => modifiedValue / ModNumber;
}

public class SetIntModification(int NewNumber) : IIntModification
{
    public MathModificationType MathModificationType { get; } = MathModificationType.Set;
    public int ModNumber { get; } = NewNumber;
    private int OldNumber { get; set; }
    public int ApplyModification(int originalValue)
    {
        OldNumber = originalValue;
        return ModNumber;
    }
    public int RemoveModification(int modifiedValue) => OldNumber;
}

public interface IFloatModification
{
    public MathModificationType MathModificationType { get; }
    public float ModNumber { get; }
    public float ApplyModification(float originalValue);
    public float RemoveModification(float modifiedValue);
}

public class AddFloatModification(float NumberToAdd) : IFloatModification
{
    public MathModificationType MathModificationType { get; } = MathModificationType.Add;
    public float ModNumber { get; } = NumberToAdd;
    public float ApplyModification(float originalValue) => originalValue + ModNumber;
    public float RemoveModification(float modifiedValue) => modifiedValue - ModNumber;
}

public class MultiplyFloatModification(float NumberToMultiply) : IFloatModification
{
    public MathModificationType MathModificationType { get; } = MathModificationType.Multiply;
    public float ModNumber { get; } = NumberToMultiply;
    public float ApplyModification(float originalValue) => originalValue * ModNumber;
    public float RemoveModification(float modifiedValue) => modifiedValue / ModNumber;
}

public class SetFloatModification(float NewNumber) : IFloatModification
{
    public MathModificationType MathModificationType { get; } = MathModificationType.Set;
    public float ModNumber { get; } = NewNumber;
    private float OldNumber { get; set; }
    public float ApplyModification(float originalValue)
    {
        OldNumber = originalValue;
        return ModNumber;
    }
    public float RemoveModification(float modifiedValue) => OldNumber;
}