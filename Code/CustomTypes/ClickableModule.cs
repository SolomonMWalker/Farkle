using Godot;

public partial class ClickableModule
{
    protected Control _affectedControl;
    private bool _isMouseInControl = false;
    public bool IsMouseInControl { get => _isMouseInControl; }
    private bool _clicked = false;
    public bool Clicked { get => _clicked; }

    public ClickableModule(Control affectedControl)
    {
        _affectedControl = affectedControl;
        _affectedControl.MouseEntered += MouseEnteredControl;
        _affectedControl.MouseExited += MouseExitedControl;
    }

    public virtual void CheckInput(InputEvent @event)
    {
        CheckControlJustClicked(@event);
        CheckControlStillClicked(@event);
    }

    public bool CheckControlJustClicked(InputEvent @event)
    {
        if (IsMouseInControl && @event is InputEventMouseButton mouseEvent && (int)mouseEvent.ButtonIndex is 1)
        {
            if (Input.IsActionJustPressed("click"))
            {
                _clicked = true;
                return true;
            }
        }
        return false;
    }

    public bool CheckControlStillClicked(InputEvent @event)
    {
        if (!Clicked) { return false; }
        if (!Input.IsActionPressed("click"))
        {
            _clicked = false;
            return false;
        }
        return true;
    }

    private void MouseEnteredControl() => _isMouseInControl = true;
    
    private void MouseExitedControl() => _isMouseInControl = false;

}