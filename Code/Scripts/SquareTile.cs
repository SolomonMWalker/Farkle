using Godot;

/// <summary>
/// 5x5 Square Tile with height of 0.1ish for building planes
/// origin at top left
/// </summary>
public partial class SquareTile : Node3D
{   
    public static int SideLength = 1;

    private Mesh mesh = GD.Load<Mesh>("res://Resources/Meshes/SquareTileMesh.tres");
    private Material material = GD.Load<Material>("res://Resources/Materials/SquareTileMaterial.tres");

    public override void _Ready()
    {
        var meshInstance = new MeshInstance3D();
        meshInstance.Mesh = mesh;
        meshInstance.MaterialOverlay = material;

        var staticBody3d = new StaticBody3D();
        
        var meshShape = mesh.CreateTrimeshShape();

        var collider = new CollisionShape3D();
        collider.Shape = meshShape;

        staticBody3d.AddChild(collider);

        AddChild(meshInstance);
        AddChild(staticBody3d);
    }
}
