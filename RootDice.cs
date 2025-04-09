using Godot;
using System;
using System.Linq;

public partial class RootDice : RigidBody3D
{
    private static float longestSideLength = 1.5f;
    private CollisionShape3D collisionShape;
    private Vector3 velocityUponThrow;

    public override void _Ready()
    {
        base._Ready();
        collisionShape = this.FindChild<CollisionShape3D>("CollisionShape3D");
        collisionShape.Disabled = true;
        Freeze = true;
        FreezeMode = FreezeModeEnum.Static;
    }

    public bool PointTooClose(Vector3 point, float margin)
    {
        //if point is sqrt(sidelength) + margin or closer, return true
        //basically a sphere around the cube of the dice
        //should work for other dice sizes as well
        return Position.DistanceTo(point) > ((Mathf.Sqrt2 * longestSideLength) + margin);
    }

    public void SetVelocityUponThrow(Vector3 velocity)
    {
        velocityUponThrow = velocity;
    }

    public void Throw()
    {
        LinearVelocity = velocityUponThrow;
    }

    public void DisableCollision()
    {
        collisionShape.Disabled = true;
    }

    public void EnableCollision()
    {
        collisionShape.Disabled = false;
    }
}
