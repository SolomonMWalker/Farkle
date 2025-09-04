using System.Collections.Generic;
using Godot;

public record ModifierFactoryEntry(string path, PackedScene pScene)
{
    public string Path { get; set; } = path;
    public PackedScene PackedScene { get; set; } = pScene;
}

public class ModifierFactory
{
    private Dictionary<string, ModifierFactoryEntry> ModifierEntries = new Dictionary<string, ModifierFactoryEntry>()
    {
        ["AddOneToDiceFacesDiceModifier"] = new("res://Scenes/Modifiers/add_one_to_dice_faces_dice_modifier.tscn", null)
    };

    public BaseModifier InstantiateModifier(string name)
    {
        if (!ModifierEntries.ContainsKey(name)) { return null; }
        if (ModifierEntries[name].PackedScene is null)
        {
            ModifierEntries[name].PackedScene = GD.Load<PackedScene>(ModifierEntries[name].Path);
        }
        return ModifierEntries[name].PackedScene.Instantiate<BaseModifier>();
    }
}

