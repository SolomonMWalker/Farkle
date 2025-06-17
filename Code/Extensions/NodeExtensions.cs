using System.Collections.Generic;
using System.Linq;
using Godot;

public static class NodeExtensions
{
    public static Node GetChildByName(this Node node, string name)
    {
        return node.GetChildren().FirstOrDefault(c => c.Name.Equals(name));
    }

    public static IEnumerable<T> GetChildrenOfType<T>(this Node node) where T : Node
    {
        return [.. node.GetChildren().OfType<T>()];
    }

    public static T GetChildByName<T>(this Node node, string name) where T : Node
    {
        return node.GetChildrenOfType<T>().FirstOrDefault(x => x.Name.Equals(name));
    }
}