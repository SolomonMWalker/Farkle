using Godot;
using System;

public partial class DraggablePanel : PanelContainer
{
    [Export]
    public Control controlToDrag = null;
    private DraggableModule _draggableModule = null;

    public override void _Ready()
    {
        base._Ready();
        if (controlToDrag is null)
        {
            _draggableModule = new DraggableModule(this);
        }
        else
        {
            _draggableModule = new DraggableModule(controlToDrag);
        }        
    }

    public void SetDraggableControl(Control control)
    {
        _draggableModule = new DraggableModule(control);
    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        _draggableModule?.CheckInput(@event);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _draggableModule?.DragAffectedControl();
    }

}
