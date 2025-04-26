using Godot;
using System;

public partial class CameraController : Node3D
{
    private Node3D userPerspectiveCameraLocation, diceZoomCameraLocation;
    private Camera3D camera;
    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        base._Ready();
        userPerspectiveCameraLocation = this.FindChild<Node3D>("UserPerspectiveCameraLocation");
        diceZoomCameraLocation = this.FindChild<Node3D>("DiceZoomCameraLocation");
        animationPlayer = this.FindChild<AnimationPlayer>("AnimationPlayer");
        camera = this.FindChild<Camera3D>("Camera3D");

        camera.Position = userPerspectiveCameraLocation.Position;
        camera.RotationDegrees = userPerspectiveCameraLocation.RotationDegrees;
    }

    public void MoveToUserPerspectiveLocation()
    {
        animationPlayer.Play("Camera_MoveTo_UserPerspective");
    }

    public void MoveToDiceZoomLocation()
    {
        animationPlayer.Play("Camera_MoveTo_DiceZoom");
    }

    public bool IsAnimationPlaying()
    {
        return animationPlayer.IsPlaying();
    }
}
