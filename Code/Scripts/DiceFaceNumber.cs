using Godot;
using System;

public partial class DiceFaceNumber : Label3D
{
    public int number;

    public override void _Ready()
    {
        base._Ready();
        number = int.Parse(Text);
    }

    public void ChangeNumber(int newNumber)
    {
        number = newNumber;
        Text = number.ToString();
    }

}
