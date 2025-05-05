using System.Collections.Generic;
using System.Linq;
using Godot;

public static class NodeExtensions
{
    public static IEnumerable<T> GetChildren<T>(this Node node) where T: Node
    {
        return node.GetChildren().OfType<T>().ToList();
    }

    public static T FindChild<T>(this Node node, string name) where T: Node
    {
        return node.GetChildren().OfType<T>().FirstOrDefault(x => x.Name.Equals(name));
    }
}