using Godot;
using System;

public partial class CameraController : Node3D
{
    private Node3D throwDiceCameraLocation, dicePickCameraLocation;
    private Camera3D camera;
    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        base._Ready();
        throwDiceCameraLocation = this.FindChild<Node3D>("ThrowDiceCameraLocation");
        dicePickCameraLocation = this.FindChild<Node3D>("DicePickCameraLocation");
        animationPlayer = this.FindChild<AnimationPlayer>("AnimationPlayer");
        camera = this.FindChild<Camera3D>("Camera3D");

        camera.Position = throwDiceCameraLocation.Position;
        camera.RotationDegrees = throwDiceCameraLocation.RotationDegrees;
    }

    public void MoveToThrowDiceLocation()
    {
        animationPlayer.Play("Camera_Move_To_ThrowDice");
    }

    public void MoveToDicePickLocation()
    {
        animationPlayer.Play("Camera_Move_To_DicePick");
    }
}
