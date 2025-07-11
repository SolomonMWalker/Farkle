using Godot;

public partial class GraphTile : Node3D
{
    public (int row, int column) GraphPosition { get; set; }
    public Vector3 TopLeftCorner => Position + new Vector3(-1, 0, -1);
    public Vector3 BottomRightCorner => Position + new Vector3(1, 0, 1);
    public bool Selected { get; private set; } = false;
    public bool ClosestToMouse { get; private set; } = false;
    public Vector3 GetCenter => Position;

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

}
