    using Godot;
using System;

public partial class ThrowLocationBall : MeshInstance3D
{
    [Export]
    public float width;
    public int dicePerThrow = 6;
    public Node3D throwLocation;
    public Node3D diceHolder;

    private Tween tween;
    private Vector3 startingPosition;

    public override void _Ready()
    {
        base._Ready();
        startingPosition = GlobalPosition;
        throwLocation = this.FindChild<Node3D>("ThrowLocation");
        diceHolder = throwLocation.FindChild<Node3D>("DiceHolder");
    }

    public void Animate()
    {
        tween?.Kill();
        tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(
            this, 
            "position:x", 
            startingPosition.X + width,
            3
         );
         tween.TweenProperty(
            this, 
            "position:x", 
            startingPosition.X,
            3
         );
    }

    public void StopAnimation()
    {
        tween?.Stop();
        Position = startingPosition;
    }
}
