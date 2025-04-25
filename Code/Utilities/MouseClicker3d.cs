using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MouseClicker3d()
{
    public Variant? GetCollisionFromMouseClick(InputEvent e, Vector2 mousePosition, Node3D node)
    {
        if(e is InputEventMouseMotion eventMouseMotion)
        {
            mousePosition = eventMouseMotion.Position;
        }
        else if (e is InputEventMouseButton eventMouseButton)
        {
            if(eventMouseButton.Pressed is false && eventMouseButton.ButtonIndex is MouseButton.Left)
            {
                return GetCollisions(node, mousePosition);
            }
        }
        return null;
    }

    public Variant? GetCollisions(Node3D node, Vector2 mouse)
    {
        var space = node.GetWorld3D().DirectSpaceState;
        var start = node.GetViewport().GetCamera3D().ProjectRayOrigin(mouse);
        var end = node.GetViewport().GetCamera3D().ProjectPosition(mouse, 1000);
        var queryParams = new PhysicsRayQueryParameters3D();
        queryParams.From = start;
        queryParams.To = end;

        //https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html
        var result = space.IntersectRay(queryParams);
        if(result.Count > 0)
        {
            return (GodotObject)result["collider"];
        }

        return null;
    }
}