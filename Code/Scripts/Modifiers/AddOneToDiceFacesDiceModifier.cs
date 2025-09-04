using Godot;
using System;
using System.Collections.Generic;

public partial class AddOneToDiceFacesDiceModifier : BaseModifier
{
    public List<IIntModification> DiceFaceValueModifications { get; private set; } = [];
    public RootDice AttachedDice { get; set; }
    private List<DiceFace> diceFaces;

    public override void _EnterTree()
    {
        base._EnterTree();
        // RootDice.ModifierModule.ModifiersParent.Modifier
        AttachedDice = (RootDice)GetParent().GetParent().GetParent();
        diceFaces = AttachedDice.diceFaceCollection.faces;
        ReceiveTrigger("EnterTree");
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ReceiveTrigger("ExitTree");
    }

    public override void ReceiveTrigger(string triggerName)
    {
        base.ReceiveTrigger(triggerName);
        if (triggerName == "EnterTree")
        {
            Activate();
        }
        else if (triggerName == "ExitTree")
        {
            Deactivate();
        }
    }

    public override void Activate()
    {
        base.Activate();
        foreach (var face in diceFaces)
        {
            face.DiceFaceValue.AddModification(
                new AddIntModification(1)
            );
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        foreach (var face in diceFaces)
        {
            face.DiceFaceValue.RemoveModification(MathModificationType.Add, 1);
        }
    }

}
