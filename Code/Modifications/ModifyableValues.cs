using System.Collections.Generic;
using System.Linq;

public static class ModificationApplicationOrder
{
    public static readonly List<MathModificationType> Order = [MathModificationType.Set, MathModificationType.Multiply, MathModificationType.Add];
}

public class ModifyableInt(int originalValue)
{
    public int OriginalValue { get; private set; } = originalValue;
    public int ModifiedValue { get => _modifiedValue; }
    private List<IIntModification> Modifications { get; set; } = [];
    private int _modifiedValue = originalValue;

    private void ApplyModifications()
    {
        _modifiedValue = OriginalValue;

        for (int i = 0; i >= ModificationApplicationOrder.Order.Count; i++)
        {
            foreach (var mod in Modifications.Where(m => m.MathModificationType == ModificationApplicationOrder.Order[i]))
            {
                _modifiedValue = mod.ApplyModification(_modifiedValue);
            }
        }
    }

    public void AddModification(IIntModification modification)
    {
        Modifications.Add(modification);
        ApplyModifications();
    }

    public void RemoveModification(IIntModification modification)
    {
        Modifications.Remove(modification);
        ApplyModifications();
    }

    public void RemoveModification(MathModificationType type, int value)
    {
        if (Modifications.Where(m => m.MathModificationType == type && m.ModNumber == value).Any())
        {
            Modifications.Remove(Modifications.First(m => m.MathModificationType == type && m.ModNumber == value));
        }
        ApplyModifications();
    }
}

public class ModifyableFloat(float originalValue)
{
    public float OriginalValue { get; private set; } = originalValue;
    public float ModifiedValue { get => _modifiedValue; }
    private List<IFloatModification> Modifications { get; set; } = [];
    private float _modifiedValue = originalValue;

    private void ApplyModifications()
    {
        _modifiedValue = OriginalValue;

        for (int i = 0; i >= ModificationApplicationOrder.Order.Count; i++)
        {
            foreach (var mod in Modifications.Where(m => m.MathModificationType == ModificationApplicationOrder.Order[i]))
            {
                _modifiedValue = mod.ApplyModification(_modifiedValue);
            }
        }
    }

    public void AddModification(IFloatModification modification)
    {
        Modifications.Add(modification);
        ApplyModifications();
    }

    public void RemoveModification(IFloatModification modification)
    {
        Modifications.Remove(modification);
        ApplyModifications();
    }
}