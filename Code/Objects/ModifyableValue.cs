using System;
using System.Collections.Generic;

public class ModifyableValue<T>(T mValue)
{
    private T _value = mValue;
    private T _originalValue = mValue;
    public T Value { get => _value; }
    public List<T> OldValues { get; private set; } = [];
    public List<Action> OnValueChangeActions { get; private set; } = [];

    public void ModifyValue(T newValue)
    {
        OldValues.Add(_value);
        _value = newValue;
        OnValueChangeActions.ForEach(a => a());
    }

    public T GetOriginalValue() => _originalValue;
}