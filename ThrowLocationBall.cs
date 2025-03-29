using Godot;
using System;

public partial class ThrowLocationBall : MeshInstance3D
{
    [Export]
    public int width;

    private Tween tween;
    private Vector3 startingPosition;

    public override void _Ready()
    {
        base._Ready();
        startingPosition = GlobalPosition;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(Input.IsKeyPressed(Key.Space))
        {
            Animate();
        }
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
}
