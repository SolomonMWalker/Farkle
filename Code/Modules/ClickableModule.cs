using Godot;

public partial class ClickableModule
{
    public Control ControlToClick { get; private set; }
    private bool _isMouseInControl = false;
    public bool IsMouseInControl { get => _isMouseInControl; }
    private bool _clicked = false;
    public bool Clicked { get => _clicked; }
    private Vector2 _vectorFromClickedToPosition = Vector2.Zero;
    public Vector2 VectorFromClickedToPosition { get => _vectorFromClickedToPosition; }

    public ClickableModule(Control controlToClick)
    {
        ControlToClick = controlToClick;
        ControlToClick.MouseEntered += MouseEnteredControl;
        ControlToClick.MouseExited += MouseExitedControl;
    }

    public virtual void CheckInput(InputEvent @event)
    {
        CheckControlJustClicked(@event);
        CheckControlStillClicked();
    }

    public bool CheckControlJustClicked(InputEvent @event)
    {
        if (!Clicked && IsMouseInControl && @event is InputEventMouseButton mouseEvent && (int)mouseEvent.ButtonIndex is 1)
        {
            if (Input.IsActionJustPressed("Click"))
            {
                _clicked = true;
                _vectorFromClickedToPosition = ControlToClick.GlobalPosition - mouseEvent.GlobalPosition;
                return true;
            }
        }
        return false;
    }

    public bool CheckControlStillClicked()
    {
        if (!Clicked)
        {
            ResetVectorFromClickedToPosition();
            return false;
        }
        if (!Input.IsActionPressed("Click"))
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