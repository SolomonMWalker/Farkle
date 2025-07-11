using System;
using System.Collections.Generic;
using Godot;

public static class HelperMethods
{
    public static Vector3 GetRandomPointInSphere(float radius, Vector3 origin)
    {
        return new Vector3(
            RandomSign() * GD.Randf() * radius + origin.X,
            RandomSign() * GD.Randf() * radius + origin.Y,
            RandomSign() * GD.Randf() * radius + origin.Z
        );
    }

    public static int RandomSign() => GD.Randf() > 0.5f ? 1 : -1;

    public static float CenterOfCubeToCorner(float sideLength) => Mathf.Sqrt(sideLength);

    public static Vector3 FuzzyUpVector3(Vector3 vector, float coefficient)
    {
        //Add random values vector axes from -1 to 1
        vector.X += RandomSign() * GD.Randf() * coefficient;
        vector.Y += RandomSign() * GD.Randf() * coefficient;
        vector.Z += RandomSign() * GD.Randf() * coefficient;

        return vector;
    }

    // get side length of cube from half its diagonal
    // we have origin of a cube to a corner which is half its diagonal
    // with this we can get the side length
    public static float GetSideLengthFromHalfDiagonal(float halfDiagonal) => halfDiagonal / Mathf.Pow(3, 1 / 3);

    public static Vector3 GetRandomVector3(float coefficient = 1f)
    {
        return new Vector3(GD.Randf() * coefficient, GD.Randf() * coefficient, GD.Randf() * coefficient);
    }

    public static ulong? GetCollisionIdFromMouseClick(InputEventMouseButton mouseButtonEvent, Node3D node)
    {
        if (mouseButtonEvent.Pressed is false && mouseButtonEvent.ButtonIndex is MouseButton.Left)
        {
            return GetCollidingObjectFromOriginRay(node, mouseButtonEvent.Position);
        }
        return null;
    }

    private static ulong? GetCollidingObjectFromOriginRay(Node3D node, Vector2 rayOrigin)
    {
        //Ripped from https://github.com/Chevifier/ChevifierTutorials/blob/main/Mouse%20Interaction%203D%20Tutorial/main.gd
        var space = node.GetWorld3D().DirectSpaceState;
        var start = node.GetViewport().GetCamera3D().ProjectRayOrigin(rayOrigin);
        var end = node.GetViewport().GetCamera3D().ProjectPosition(rayOrigin, 1000);
        var queryParams = new PhysicsRayQueryParameters3D();
        queryParams.From = start;
        queryParams.To = end;

        //https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html
        var result = space.IntersectRay(queryParams);
        if (result.Count > 0)
        {
            return (ulong)result["collider_id"];
        }

        return null;
    }

    public static Vector2 GetScreenPositionOfGlobalPosition3D(Vector3 globalPosition, Camera3D mainCamera) => mainCamera.UnprojectPosition(globalPosition);
}