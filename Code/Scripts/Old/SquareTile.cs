using Godot;

/// <summary>
/// 1x1 Square Tile with height of 0.1ish for building planes
/// origin at top left
/// </summary>
public partial class SquareTile : Node3D
{   
    public const int SideLength = 1;

    private const string SquareTileMeshRelPath = "res://Resources/Meshes/SquareTileMesh.tres", 
        SquareTileMaterialRelPath = "res://Resources/Materials/SquareTileMaterial.tres";

    private Mesh mesh = GD.Load<Mesh>(SquareTileMeshRelPath);
    private Material material = GD.Load<Material>(SquareTileMaterialRelPath);

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
