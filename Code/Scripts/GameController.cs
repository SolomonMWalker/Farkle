using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum GameState{
    PreRoll,
    Rolling,
    //PickDice,
    PostRoll
}

public partial class GameController : Node3D
{
    [Export]
    public int diceOriginSphereRadius = 5;
    [Export]
    public float diceOriginMargin = 0.01f;
    [Export]
    public int diceAmount = 6;

    private DiceCollection diceCollection;
    private Node diceHolder;
    private ThrowLocationBall throwLocationBall;
    private Node throwLocationDiceHolder;
    private PackedScene packedRootDice;
    private CameraController cameraController;
    private GameState gameState;

    private Vector2 mousePosition;

    public override void _Ready()
    {
        base._Ready();
        diceCollection = new DiceCollection();
        diceHolder = this.FindChild<Node>("DiceHolder");
        throwLocationBall = FindChild("DiceTable").FindChild<ThrowLocationBall>("ThrowLocationBall");
        throwLocationDiceHolder = throwLocationBall.diceHolder;
        packedRootDice = GD.Load<PackedScene>("res://Scenes/root_dice.tscn");
        cameraController = this.FindChild<CameraController>("CameraController");
        gameState = GameState.PostRoll;
        mousePosition = Vector2.Zero;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        HandleGameState();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        HandleDicePhysics();
    }

    public void HandleGameState()
    {
        if(gameState == GameState.PreRoll)
        {
            if(Input.IsActionJustPressed("space"))
            {
                throwLocationBall.StopAnimation();
                ThrowDice();
                cameraController.MoveToDicePickLocation();
                gameState = GameState.Rolling;
            }
        }
        else if(gameState == GameState.Rolling)
        {
            if(diceCollection.IsDoneRolling())
            {
                if(Input.IsActionJustPressed("space"))
                {
                    cameraController.MoveToThrowDiceLocation();
                    gameState = GameState.PostRoll;
                }
            }
        }
        else //gameState == GameState.PostRoll
        {
            if(Input.IsActionJustPressed("space"))
            {
                if(throwLocationBall.state == ThrowLocationBallState.Inactive)
                {
                    SetDicePositionForThrow();
                    SetDiceVelocityForThrow();
                    throwLocationBall.Animate();
                }

                gameState = GameState.PreRoll;
            }
        }
    }

    public void HandleDicePhysics()
    {
        if(throwLocationBall.state == ThrowLocationBallState.ReadyToThrow)
        {
            diceCollection.diceList.ForEach(x => x.GlobalPosition = throwLocationBall.throwLocation.GlobalPosition);
        }        
    }

    public void SetDicePositionForThrow()
    {
        var diceInPosition = new DiceCollection();

        if(diceCollection.diceList.Count == 0)
        {
            for(int i = 0; i < diceAmount; i++)
            {
                var dice = packedRootDice.Instantiate<RootDice>();
                diceCollection.diceList.Add(dice);
            }
            diceCollection.SetParent(throwLocationDiceHolder);
        }
        else
        {
            diceCollection.DisableCollision();
            diceCollection.FreezeDice();
            diceCollection.ChangeParent(throwLocationDiceHolder, false);
        }

        foreach(RootDice dice in diceCollection.diceList)
        {
            var dicePosition = HelperMethods.GetRandomPointInSphere(
                diceOriginSphereRadius, throwLocationBall.throwLocation.GlobalPosition
            );

            if(diceInPosition.diceList.Count > 0 )
            {
                while(diceInPosition.PointTooClose(dicePosition, diceOriginMargin))
                {
                    dicePosition = HelperMethods.GetRandomPointInSphere(
                        diceOriginSphereRadius, throwLocationBall.throwLocation.GlobalPosition
                    );
                }
            }


            dice.Rotate(
                new Vector3(GD.Randf(), GD.Randf(), GD.Randf()).Normalized(),
                GD.Randf()*(2*Mathf.Pi)
            );
            dice.GlobalPosition = dicePosition;
            diceInPosition.diceList.Add(dice);
        }

        diceCollection = diceInPosition;
    }

    public void SetDiceVelocityForThrow()
    {
        //base velocity is 0,0,-1
        var baseVelocity = new Vector3(0, 0, -1) * 6;
        foreach(RootDice dice in diceCollection.diceList)
        {
            dice.SetVelocityUponThrow(HelperMethods.FuzzyUpVector3(baseVelocity, 0.5f));
        }
    }

    public void ThrowDice()
    {
        diceCollection.ChangeParent(diceHolder, true);
        diceCollection.UnfreezeDice();
        diceCollection.EnableCollision();
        diceCollection.ThrowDice();
    }

}
