using Godot;
using System;

public partial class DraggablePanel : PanelContainer
{
    private DraggableModule _draggableModule = null;

    public override void _Ready()
    {
        base._Ready();
        _draggableModule = new DraggableModule(this);
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
