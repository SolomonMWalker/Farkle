using Godot;

public partial class ThrowLocationBall : MeshInstance3D
{
    [Export]
    public float width;
    public int dicePerThrow = 6;
    public Node3D throwLocation;
    public Node3D diceHolder;
    public Control control;

    private Tween tween;
    private Vector3 startingPosition;

    public override void _Ready()
    {
        base._Ready();
        startingPosition = GlobalPosition;
        throwLocation = this.GetChildByName<Node3D>("ThrowLocation");
        diceHolder = throwLocation.GetChildByName<Node3D>("DiceHolder");
        control = this.GetChildByName<Control>("Control");
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
            2.5
         );
        tween.TweenProperty(
           this,
           "position:x",
           startingPosition.X,
           2.5
        );
    }

    public void StopAnimation()
    {
        tween?.Stop();        
    }

    public void ResetPositon()
    {
        Position = startingPosition;
    }
}
