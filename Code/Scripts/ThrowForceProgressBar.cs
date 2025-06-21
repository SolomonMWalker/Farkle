using Godot;

public partial class ThrowForceProgressBar : ProgressBar
{
    private const double TimeToFillUpBarInSec = 0.8;
    private readonly Vector2 OffsetFromThrowLocation = new(50f, -100f);

    private bool isIncreasing = true;
    private bool isStarted = false;

    public override void _Ready()
    {
        base._Ready();
        End();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (isStarted)
        {
            BarProcess(delta);
        }
    }

    public void Start(Vector2 newPosition)
    {
        Position = newPosition + OffsetFromThrowLocation;
        Visible = true;
        Value = 0;
        isStarted = true;
    }

    public void End()
    {
        Visible = false;
        Position = Vector2.Zero;
        isStarted = false;
    }

    public void BarProcess(double delta)
    {
        var amountToChangeInSec = MaxValue / TimeToFillUpBarInSec;
        double newValue;
        if (isIncreasing)
        {
            newValue = Value + (amountToChangeInSec * delta);
            if (newValue > MaxValue)
            {
                isIncreasing = false;
                newValue = MaxValue;
            }
        }
        else //implicitly decreasing
        {
            newValue = Value - (amountToChangeInSec * delta);
            if (newValue < MinValue)
            {
                isIncreasing = true;
                newValue = MinValue;
            }
        }

        Value = newValue;
    }
}
