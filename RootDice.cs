using Godot;
using System;
using System.Linq;

public partial class RootDice : RigidBody3D
{
    private CollisionShape3D collisionShape;
    private Node3D corner;
    private Vector3 velocityUponThrow;
    private float edgelength;
    //diagonal of cube formula (3^(1/3))*sideLength = diagonal
    //sidelength = diagonal / ((3^(1/3)*sidelength))

    public override void _Ready()
    {
        base._Ready();
        collisionShape = this.FindChild<CollisionShape3D>("CollisionShape3D");
        corner = this.FindChild<Node3D>("Corner");
        edgelength = HelperMethods.GetSideLengthFromHalfDiagonal(Position.DistanceTo(corner.Position));
        collisionShape.Disabled = true;
        Freeze = true;
        FreezeMode = FreezeModeEnum.Static;
    }

    public bool PointTooClose(Vector3 point, float margin)
    {
        //if point is sqrt(sidelength) + margin or closer, return true
        //basically a sphere around the cube of the dice
        //should work for other dice sizes as well
        return Position.DistanceTo(point) > ((Mathf.Sqrt2 * edgelength) + margin);
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
