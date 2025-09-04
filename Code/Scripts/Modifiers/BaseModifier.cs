using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class BaseModifier : Node3D
{
    public Guid Id { get; } = Guid.NewGuid();
    public bool Activated { get; private set; }
    public ModifierModule ModifierModule { get; set; }
    protected List<string> Triggers { get; set; } = ["EnterTree", "ExitTree"];
    public virtual void ReceiveTrigger(string triggerName) { }
    public virtual void Activate() { }
    public virtual void Deactivate() { }
}
