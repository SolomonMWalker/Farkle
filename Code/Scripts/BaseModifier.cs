using Godot;
using System;
using System.Collections.Generic;

public partial class BaseModifier : Node3D
{
    public Guid Id { get; } = Guid.NewGuid();
    public bool Activated { get; private set; }
    protected List<string> Triggers { get; set; } = ["Created", "Destroyed"];
    public virtual void ReceiveTrigger(string triggerName) { }
    public virtual void Activate() { }
    public virtual void Deactivate() { }
}
