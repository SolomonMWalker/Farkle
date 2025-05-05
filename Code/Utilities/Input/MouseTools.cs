using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MouseTools()
{

    //MAKE MOUSEPOSITION REFERENCE OR SEPARATE OUT TO ITS OWN METHOD
    public static ulong? GetCollisionIdFromMouseClick(Vector2 mousePosition, InputEventMouseButton mouseButtonEvent, Node3D node)
    {
        if(mouseButtonEvent.Pressed is false && mouseButtonEvent.ButtonIndex is MouseButton.Left)
        {
            return GetCollidingObject(node, mousePosition);
        }
        return null;
    }

    private static ulong? GetCollidingObject(Node3D node, Vector2 mouse)
    {
        //Ripped from https://github.com/Chevifier/ChevifierTutorials/blob/main/Mouse%20Interaction%203D%20Tutorial/main.gd
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
            return (ulong)result["collider_id"];
        }

        return null;
    }
}