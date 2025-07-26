using Godot;

public class TableManager
{
    private TableGraph _tableGraph;
    private Camera3D _mainCamera;
    private int _maximumObstacleSnapDistanceInScreenUnits = 50;

    public TableManager(TableGraph tableGraph, Camera3D mainCamera)
    {
        _tableGraph = tableGraph;
        _tableGraph.SetCamera3D(mainCamera);
    }

    public bool TrySnapObjectToTile(Vector2 mousePosition)
    {
        if (!_tableGraph.TryGetGraphTileClosestToScreenPosition(mousePosition, out var closestTile)) { return false; }
        var distance = ViewportScaler.GetDistanceInUnits(
            mousePosition, closestTile.GetScreenPixelPositionOfGraphTileCenter(_mainCamera));
        if (distance > _maximumObstacleSnapDistanceInScreenUnits) { return false; }
        //snap object to closest tile
        return true;
    }
}