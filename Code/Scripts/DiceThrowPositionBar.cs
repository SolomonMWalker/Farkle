using Godot;
using System;

/// <summary>
/// Gets the left and right extremes of the guidebar and animates the throwSymbol left and right
/// </summary>
public partial class DiceThrowPositionBar : MarginContainer
{
    private Control guideBar, throwSymbol;

    private Tween tween;

    private Vector2 startingLocation;

    public override void _Ready()
    {
        base._Ready();

        guideBar = this.FindChild<ColorRect>("ColorRect");
        throwSymbol = guideBar.FindChild<TextureRect>("TextureRect");

        startingLocation = throwSymbol.Position - (throwSymbol.Size / 2);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(Input.IsKeyPressed(Key.Space))
        {
            Animate();
        }
    }

    public void Animate()
    {
        tween?.Kill();
        tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(
            throwSymbol, 
            "position:x", 
            guideBar.Size.X - throwSymbol.Size.X,
            3
         );
         tween.TweenProperty(
            throwSymbol, 
            "position:x", 
            0,
            3
         );
    }
}
