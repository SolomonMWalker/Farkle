using Godot;
using System;

public partial class DiceFace : Label3D
{
    [Export]
    public int number;
    public RootDice AssociatedDice { get => _associatedDice; }
    private RootDice _associatedDice;

    public override void _Ready()
    {
        base._Ready();
        number = int.Parse(Text);
        _associatedDice = GetParent<Node3D>().GetParent<RootDice>();
    }

    public void ChangeNumber(int newNumber)
    {
        number = newNumber;
        Text = number.ToString();
    }

}
