using Godot;

public partial class GraphTile : Node3D
{
    [Export]
    public Sprite3D centerSprite;

    public (int row, int column) GraphPosition { get; set; }
    public Vector3 TopLeftCorner => Position + new Vector3(-1, 0, -1);
    public Vector3 BottomRightCorner => Position + new Vector3(1, 0, 1);
    public bool Selected { get; private set; } = false;
    public bool ClosestToMouse { get; private set; } = false;
    public Vector3 GetCenter => Position;
    private Color _centerSpriteDefaultColor = Colors.Red, _centerSpriteSelectedColor = Colors.White;
    private Color _currentSpriteColor;

    public override void _Ready()
    {
        base._Ready();
        _currentSpriteColor = _centerSpriteDefaultColor;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public Vector2 GetScreenPixelPositionOfGraphTileCenter(Camera3D mainCamera)
    {
        return mainCamera.UnprojectPosition(GlobalPosition);
    }

    public Vector2 GetScreenUnitsPositionOfGraphTileCenter(Camera3D mainCamera)
    {
        return ViewportScaler.GetViewportPositionInUnits(GetScreenPixelPositionOfGraphTileCenter(mainCamera));
    }

    public void SelectTile()
    {
        Selected = true;
        _currentSpriteColor = _centerSpriteSelectedColor;
        centerSprite.Modulate = _currentSpriteColor;
    }

    public void UnselectTile()
    {
        Selected = false;
        _currentSpriteColor = _centerSpriteDefaultColor;
        centerSprite.Modulate = _currentSpriteColor;
    }
}
