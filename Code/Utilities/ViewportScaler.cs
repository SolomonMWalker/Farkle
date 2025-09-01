using Godot;

public class ViewportScaler
{
    public const int UnitsPerAxis = 300;
    public static float XAxisPixelsPerUnit, YAxisPixelsPerUnit;
    public static Viewport Viewport;

    public static void SetViewport(Viewport viewport) => Viewport = viewport;

    public static void SetPixelsPerAxis()
    {
        var viewportRect = Viewport.GetVisibleRect();
        XAxisPixelsPerUnit = viewportRect.Size.X / UnitsPerAxis;
        YAxisPixelsPerUnit = viewportRect.Size.Y / UnitsPerAxis;
    }

    public static Vector2 GetViewportPositionInUnits(Vector2 screenPosition)
    {
        return new Vector2(screenPosition.X / XAxisPixelsPerUnit, screenPosition.Y / YAxisPixelsPerUnit);
    }

    public static float GetViewportPositionXAxisInUnis(float xScreenPosition) => xScreenPosition / XAxisPixelsPerUnit;
    public static float GetViewportPositionYAxisInUnis(float yScreenPosition) => yScreenPosition / YAxisPixelsPerUnit;

    public static float GetDistanceInUnits(Vector2 fromScreenPosition, Vector2 toScreenPosition) =>
        Mathf.Abs(GetViewportPositionInUnits(fromScreenPosition).DistanceTo(GetViewportPositionInUnits(toScreenPosition)));

}