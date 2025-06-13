using Godot;

public partial class ClickableModule
{
    protected Control _affectedControl;
    public Control AffectedControl { get => _affectedControl; }
    private bool _isMouseInControl = false;
    public bool IsMouseInControl { get => _isMouseInControl; }
    private bool _clicked = false;
    public bool Clicked { get => _clicked; }
    private Vector2 _vectorFromClickedToPosition = Vector2.Zero;
    public Vector2 VectorFromClickedToPosition { get => _vectorFromClickedToPosition; }

    public ClickableModule(Control affectedControl)
    {
        _affectedControl = affectedControl;
        AffectedControl.MouseEntered += MouseEnteredControl;
        AffectedControl.MouseExited += MouseExitedControl;
    }

    public virtual void CheckInput(InputEvent @event)
    {
        CheckControlJustClicked(@event);
        CheckControlStillClicked(@event);
    }

    public bool CheckControlJustClicked(InputEvent @event)
    {
        if (!Clicked && IsMouseInControl && @event is InputEventMouseButton mouseEvent && (int)mouseEvent.ButtonIndex is 1)
        {
            if (Input.IsActionJustPressed("click"))
            {
                _clicked = true;
                _vectorFromClickedToPosition = AffectedControl.GlobalPosition - mouseEvent.GlobalPosition;
                return true;
            }
        }
        return false;
    }

    public bool CheckControlStillClicked(InputEvent @event)
    {
        if (!Clicked)
        {
            ResetVectorFromClickedToPosition();
            return false;
        }
        if (!Input.IsActionPressed("click"))
        {
            _clicked = false;
            ResetVectorFromClickedToPosition();
            return false;
        }
        return true;
    }

    private void MouseEnteredControl() => _isMouseInControl = true;

    private void MouseExitedControl() => _isMouseInControl = false;

    private void ResetVectorFromClickedToPosition()
    {
        if (VectorFromClickedToPosition != Vector2.Zero)
        {
            _vectorFromClickedToPosition = Vector2.Zero;
        }
    }

}