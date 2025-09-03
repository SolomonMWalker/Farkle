using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ModifierModule : Node3D
{
    public PlayerManager playerManager;

    private List<string> ModifierTriggers { get; set; } = [];
    private List<BaseModifier> Modifiers { get; set; } = [];
    private Node ModifiersParent { get; set; }
    private ModifierFactory ModifierFactory { get; set; } = new ModifierFactory();

    public override void _Ready()
    {
        base._Ready();
        ModifiersParent = FindChild("Modifiers");
    }

    public bool TryAddModifierTrigger(string triggerName)
    {
        if (ModifierTriggers.Contains(triggerName)) { return false; }
        ModifierTriggers.Add(triggerName);
        return true;
    }

    public void FireModifierTrigger(string triggerName)
    {
        if (!ModifierTriggers.Contains(triggerName)) { return; }
        foreach (var modifier in Modifiers)
        {
            modifier.ReceiveTrigger(triggerName);
        }
    }

    public void AddModifier(string modifierName)
    {
        var mod = ModifierFactory.InstantiateModifier(modifierName);
        Modifiers.Add(mod);
        ModifiersParent.AddChild(mod);
        //Add mod to playerManager master modList
    }

    public void RemoveModifier(Guid modifierId)
    {
        var mod = Modifiers.FirstOrDefault(mod => mod.Id == modifierId);
        if (mod is null) { return; }
        if (mod.Activated) { mod.Deactivate(); }
        Modifiers.Remove(mod);
        mod.QueueFree();
        //Remove mod from playerManager master modList
    }
}
