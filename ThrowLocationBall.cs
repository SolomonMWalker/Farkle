using Godot;
using System;

public partial class ThrowLocationBall : MeshInstance3D
{
    [Export]
    public int width;
    public int dicePerThrow = 6;
    public ThrowLocationBallState state = ThrowLocationBallState.Inactive;
    public Node3D throwLocation;
    public Node diceHolder;

    private Tween tween;
    private Vector3 startingPosition;

    public override void _Ready()
    {
        base._Ready();
        startingPosition = GlobalPosition;
        throwLocation = this.FindChild<Node3D>("ThrowLocation");
        diceHolder = throwLocation.FindChild<Node>("DiceHolder");
    }

    public void Animate()
    {
        state = ThrowLocationBallState.ReadyToThrow;

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
        state = ThrowLocationBallState.Inactive;
    }
}

public enum ThrowLocationBallState
{
    Inactive,
    ReadyToThrow
}
