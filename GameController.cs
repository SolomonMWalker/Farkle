using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class GameController : Node
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


    public override void _Ready()
    {
        base._Ready();
        diceCollection = new DiceCollection(FindChild("DiceHolder").GetChildren<RootDice>());
        diceHolder = this.FindChild<Node>("DiceHolder");
        throwLocationBall = this.FindChild<ThrowLocationBall>("ThrowLocationBall");
        throwLocationDiceHolder = throwLocationBall.diceHolder;
        packedRootDice = GD.Load<PackedScene>("res://root_dice.tscn");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        HandleDice();
    }


    public void HandleDice()
    {
        if(Input.IsActionJustPressed("space"))
        {
            if(throwLocationBall.state == ThrowLocationBallState.Inactive)
            {
                SetDicePositionForThrow();
                SetDiceVelocityForThrow();
                throwLocationBall.Animate();
            }
            else {
                throwLocationBall.StopAnimation();
                //ThrowDice();
            }
            
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
                throwLocationDiceHolder.AddChild(dice);
            }
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
            
            dice.GlobalPosition = dicePosition;
            diceInPosition.diceList.Add(dice);
        }

        diceCollection = diceInPosition;
    }

    public void SetDiceVelocityForThrow()
    {
        //base velocity is 0,0,-1
        var baseVelocity = new Vector3(0, 0, -1);
        foreach(RootDice dice in diceCollection.diceList)
        {
            dice.SetVelocityUponThrow(HelperMethods.FuzzyUpVector3(baseVelocity, 0.5f));
        }
    }

    public void ThrowDice()
    {
        diceCollection.ChangeParent(diceHolder, false);
        diceCollection.EnableCollision();
        diceCollection.ThrowDice();
    }

}
