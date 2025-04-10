using System.Collections.Generic;
using Godot;

/// <summary>
/// Rectangle begins generation at origin in top left
/// Width and Height must be a multiple of 1
/// </summary>
public partial class SquareTilePlane : Node3D
{
    [Export]
    public int width, height;
    [Export]
    public PlaneOrientation planeOrientation;

    private Dictionary<(int, int), SquareTile> squareTilesGraph = new Dictionary<(int, int), SquareTile>();
    private Node squareTilesParent;

    public override void _Ready()
    {
        // Called every time the node is added to the scene.
        // Initialization here.
        squareTilesParent = FindChild("SquareTilesParent");
        BuildPlane();
    }

    public void BuildPlane()
    {
        for(int i = 0; i < width - SquareTile.SideLength; i += SquareTile.SideLength)
        {
            for(int j = 0; j < height - SquareTile.SideLength; j += SquareTile.SideLength)
            {
                var squareTile = new SquareTile();
                squareTile.Name = $"{i}_{j}SquareTile";

                switch (planeOrientation)
                {
                    case PlaneOrientation.Up:
                        squareTile.Position = Position + new Vector3(i, 0, j);
                        break;
                    case PlaneOrientation.Down:
                        squareTile.Position = Position + new Vector3(-i, 0, -j);
                        break;
                    case PlaneOrientation.Left:
                        squareTile.Position = Position + new Vector3(0, i, j);
                        break;
                    case PlaneOrientation.Right:
                        squareTile.Position = Position + new Vector3(0, -i, -j);
                        break;
                    case PlaneOrientation.Backward:
                        squareTile.Position = Position + new Vector3(i, j, 0);
                        break;
                    default: //PlaneOrientation.Forward
                        squareTile.Position = Position + new Vector3(-i, -j, 0);
                        break;
                }

                squareTilesGraph.Add((i, j), squareTile);
                squareTilesParent.AddChild(squareTile);
            }
        }
    }
}

public enum PlaneOrientation{
    Up, //positive y
    Down, //negative y
    Left, //negative x
    Right, //positive x
    Backward, //positive z
    Forward, //negative z
}
