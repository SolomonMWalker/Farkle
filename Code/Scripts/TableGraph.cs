using Godot;
using System;
using System.Collections.Generic;

public partial class TableGraph : Node3D
{
    private const string GraphTilePath = "res://Scenes/graph_tile.tscn";
    private PackedScene GraphTilePackedScene { get; set; }
    private Node3D TilesParent { get; set; }

    [Export]
    public int width = 0;
    [Export]
    public int height = 0;

    public List<GraphTile> GraphTiles { get; private set; } = [];

    public override void _Ready()
    {
        base._Ready();
        GD.Print("Starting");
        if (width % 2 != 0 || height % 2 != 0)
        {
            GD.PrintErr("Width or height of graph is not divisible by 2.");
            throw new Exception();
        }

        TilesParent = (Node3D) FindChild("Tiles");

        GraphTilePackedScene = GD.Load<PackedScene>(GraphTilePath);
        GraphInitialSetup();
    }

    public void GraphInitialSetup()
    {
        //rows
        for (int row = 0; row < width / 2; row++)
        {
            //columns
            for (int col = 0; col < height / 2; col++)
            {
                var tilePosition = new Vector3((row * 2) + 1, 0, (col * 2) + 1);
                var tile = GraphTilePackedScene.Instantiate<GraphTile>();
                TilesParent.AddChild(tile);
                tile.Position = tilePosition;
                tile.GraphPosition = (row, col);
                GraphTiles.Add(tile);
            }
        }
    }

}
