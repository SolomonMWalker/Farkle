using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RootDice : RigidBody3D
{
    private CollisionShape3D collisionShape;
    private MeshInstance3D meshInstance;
    private Material rootDiceMaterial, rootDiceSelectedMaterial;
    private Node3D corner;
    private Vector3 velocityUponThrow;
    private DiceFaceCollection diceFaceCollection;
    private int colliderId;
    private float edgelength;
    private const string RootDiceMaterialPath = "res://Resources/Materials/RootDiceMaterial.tres";
    private const string RootDiceSelectedMaterialPath = "res://Resources/Materials/RootDiceSelectedMaterial.tres";
    

    public override void _Ready()
    {
        base._Ready();
        collisionShape = this.FindChild<CollisionShape3D>("CollisionShape3D");
        meshInstance = this.FindChild<MeshInstance3D>("MeshInstance3D");
        corner = this.FindChild<Node3D>("Corner");
        edgelength = HelperMethods.GetSideLengthFromHalfDiagonal(Position.DistanceTo(corner.Position));
        rootDiceMaterial = GD.Load<Material>(RootDiceMaterialPath);
        rootDiceSelectedMaterial = GD.Load<Material>(RootDiceSelectedMaterialPath);
        collisionShape.Disabled = true;
        Freeze = true;
        FreezeMode = FreezeModeEnum.Static;
        SetupDiceFaces();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        var mouse = Vector2.Zero;
        if(@event is InputEventMouseMotion eventMouseMotion)
        {
            mouse = eventMouseMotion.Position;
        }
        else if (@event is InputEventMouseButton eventMouseButton)
        {
            if(eventMouseButton.Pressed is false && eventMouseButton.ButtonIndex is MouseButton.Left)
            {
                GetCollisions(mouse);
            }
        }
    }

    public void GetCollisions(Vector2 mouse)
    {
        var space = GetWorld3D().DirectSpaceState;
        var start = GetViewport().GetCamera3D().ProjectRayOrigin(mouse);
        var end = GetViewport().GetCamera3D().ProjectPosition(mouse, 1000);
        var queryParams = new PhysicsRayQueryParameters3D();
        queryParams.From = start;
        queryParams.To = end;

        var result = space.IntersectRay(queryParams);
        GD.Print(result);
    }

    public void SetupDiceFaces()
    {
        var diceFaceParent = FindChild("DiceFaces");
        var diceFaces = diceFaceParent.GetChildren<DiceFaceNumber>();
        diceFaceCollection = new DiceFaceCollection();
        diceFaceCollection.faces = diceFaces;
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

    public bool IsDoneRolling() {
        var velocityIsCloseToZero = Mathf.Abs(LinearVelocity.Length()) < 0.1;
        var lowestFaceHeightIsCloseToTable = 
            Mathf.Abs(diceFaceCollection.GetHeightOfLowestFace()) < (edgelength/2) + 0.1 ;
        return velocityIsCloseToZero && lowestFaceHeightIsCloseToTable;
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
