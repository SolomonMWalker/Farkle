using Godot;

public partial class DraggableModule(Control affectedControl) : ClickableModule(affectedControl)
{
    private Vector2 _drag = Vector2.Zero;

    public override void CheckInput(InputEvent @event)
    {
        base.CheckInput(@event);
        _drag = GetDrag(@event);
    }

    public void DragAffectedControl()
    {
        _affectedControl.SetPosition(_affectedControl.Position + _drag);
        _drag = Vector2.Zero;
    }

    public Vector2 GetDrag(InputEvent @event)
    {
        if (Clicked && @event is InputEventMouseMotion mouseMotion)
        {
            var movementSinceLastFrame = mouseMotion.Position - mouseMotion.ScreenRelative;
            return movementSinceLastFrame;
        }
        return Vector2.Zero;
    }
}