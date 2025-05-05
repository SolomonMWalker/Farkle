using System.Collections.Generic;
using Godot;

public partial class GameController : Node3D
{
    [Export]
    public int diceOriginSphereRadius = 1;
    [Export]
    public float diceOriginMargin = 0.01f;
    [Export]
    public int diceAmount = 6;

    private DiceCollection rollableDiceCollection, scoringDiceCollection;
    private Node3D diceHolder;
    private ThrowLocationBall throwLocationBall;
    private Node throwLocationDiceHolder;
    private PackedScene packedRootDice;
    private CameraController cameraController;
    private GameStateManager gameStateManager;
    private Vector2 mousePosition;
    private Label gameStateLabel, scoreLabel;

    public override void _Ready()
    {
        base._Ready();
        rollableDiceCollection = new DiceCollection();
        diceHolder = this.FindChild<Node3D>("DiceHolder");
        throwLocationBall = FindChild("DiceTable").FindChild<ThrowLocationBall>("ThrowLocationBall");
        throwLocationDiceHolder = throwLocationBall.diceHolder;
        packedRootDice = GD.Load<PackedScene>("res://Scenes/root_dice.tscn");
        cameraController = this.FindChild<CameraController>("CameraController");
        gameStateLabel = this.FindChild<Label>("GameState");
        scoreLabel = this.FindChild<Label>("Score");
        gameStateManager = new GameStateManager();
        mousePosition = Vector2.Zero;
        scoringDiceCollection = new DiceCollection();

        CreateDiceCollection();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        RunGame();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        HandleDicePhysics();
    }

    public void RunGame()
    {
        var gameState = gameStateManager.GetState();

        if(gameState == GameState.PreRoll)
        {
            if(Input.IsActionJustPressed("space"))
            {
                ReadyDiceForRoll();
                throwLocationBall.Animate();
                gameStateManager.ProgressState();
            }
        }
        else if(gameState == GameState.RollReady)
        {
            if(Input.IsActionJustPressed("space"))
            {
                ThrowDice();
                throwLocationBall.StopAnimation();
                cameraController.MoveToDiceZoomLocation();
                gameStateManager.ProgressState();
            }
        }
        else if(gameState == GameState.Rolling)
        {
            if(rollableDiceCollection.IsDoneRolling() && !cameraController.IsAnimationPlaying())
            {
                gameStateManager.ProgressState();
            }
        }
        else if(gameState == GameState.SelectDice)
        {
            if(Input.IsActionJustPressed("space"))
            {
                cameraController.MoveToUserPerspectiveLocation();
                gameStateManager.ProgressState();
            }
        }
        else if (gameState == GameState.ExitDiceZoomAnimation)
        {
            if(!cameraController.IsAnimationPlaying())
            {
                gameStateManager.ProgressState();
            }
        }

        if(gameStateLabel.Text != gameState.ToString())
        {
            gameStateLabel.Text = gameState.ToString();
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        HandleMouseInput(@event);
    }

    public void HandleMouseInput(InputEvent inputEvent)
    {
        if(inputEvent is InputEventMouseMotion mouseMotion)
        {
            mousePosition = mouseMotion.Position;
        }
        else if(inputEvent is InputEventMouseButton mouseButton)
        {
            var selectedDice = HandleMouseClickOnObject(
                MouseTools.GetCollisionIdFromMouseClick(mousePosition, mouseButton, this));

            if(selectedDice is not null) 
            {
                if(scoringDiceCollection.diceList.Contains(selectedDice))
                {
                    scoringDiceCollection = scoringDiceCollection.RemoveDice(selectedDice);
                    scoreLabel.Text = scoringDiceCollection.CalculateScore().Score.ToString();
                    selectedDice.SelectDice();
                }
                else
                {
                    scoringDiceCollection = scoringDiceCollection.AddDice(selectedDice);
                    scoreLabel.Text = scoringDiceCollection.CalculateScore().Score.ToString();
                    selectedDice.SelectDice();
                }
            }
        }
    }

    public RootDice HandleMouseClickOnObject(ulong? objInstanceId)
    {
        if(objInstanceId == null)
        {
            return null;
        }
        var dice =  rollableDiceCollection.GetDiceWithInstanceIdEqualTo(objInstanceId.Value);
        return dice; 
    }

    public void HandleDicePhysics()
    {
        if(gameStateManager.GetState() is GameState.RollReady)
        {
            rollableDiceCollection.diceList.ForEach(
                x => x.GlobalPosition = throwLocationBall.throwLocation.GlobalPosition);
        }        
    }

    public void CreateDiceCollection()
    {
        var tempDiceList = new List<RootDice>();
        for(int i = 0; i < diceAmount; i++)
        {
            var dice = packedRootDice.Instantiate<RootDice>();
            tempDiceList.Add(dice);
        }
        rollableDiceCollection = new DiceCollection(tempDiceList);
        rollableDiceCollection.SetParent(diceHolder);
    }

    public void SetDiceRotationForThrow()
    {
        foreach(RootDice dice in rollableDiceCollection.diceList)
        {
            dice.Rotate(HelperMethods.GetRandomVector3().Normalized(), GD.Randf()*(2*Mathf.Pi));
        }
    }

    public void SetDiceVelocityForThrow()
    {
        var baseVelocity = new Vector3(0, 0, -1) * 6;
        foreach(RootDice dice in rollableDiceCollection.diceList)
        {
            dice.SetVelocityUponThrow(HelperMethods.FuzzyUpVector3(baseVelocity, 0.5f));
        }
    }

    public void ReadyDiceForRoll() //turn off dice, set rotation and velocity
    {
        rollableDiceCollection.TurnOff();
        SetDiceRotationForThrow();
        SetDiceVelocityForThrow();
    }

    public void ThrowDice()
    {
        rollableDiceCollection.TurnOn();
        rollableDiceCollection.ThrowDice();
    }

}
