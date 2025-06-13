using Godot;

public partial class DraggableModule(Control affectedControl) : ClickableModule(affectedControl)
{
    private Vector2 _drag = Vector2.Zero;

    public override void CheckInput(InputEvent @event)
    {
        base.CheckInput(@event);
        if (TryGetDrag(@event, out var newDrag)) { _drag = newDrag; }
        else { _drag = Vector2.Zero; }
    }

    public void DragAffectedControl()
    {
        if (_drag == Vector2.Zero) { return; }
        AffectedControl.SetPosition(AffectedControl.Position + VectorFromClickedToPosition + _drag);
        _drag = Vector2.Zero;
    }

    public bool TryGetDrag(InputEvent @event, out Vector2 drag)
    {
        if (Clicked && @event is InputEventMouseMotion mouseMotion)
        {
            drag = mouseMotion.Position - mouseMotion.ScreenRelative;
            return true;
        }
        drag = Vector2.Zero;
        return false;
    }
}