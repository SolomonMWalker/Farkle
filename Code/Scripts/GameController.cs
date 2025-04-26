using Godot;

public partial class GameController : Node3D
{
    [Export]
    public int diceOriginSphereRadius = 1;
    [Export]
    public float diceOriginMargin = 0.01f;
    [Export]
    public int diceAmount = 6;

    private DiceCollection diceCollection;
    private Node3D diceHolder;
    private ThrowLocationBall throwLocationBall;
    private Node throwLocationDiceHolder;
    private PackedScene packedRootDice;
    private CameraController cameraController;
    private GameStateManager gameStateManager;
    private Vector2 mousePosition;
    private Label label;

    public override void _Ready()
    {
        base._Ready();
        diceCollection = new DiceCollection();
        diceHolder = this.FindChild<Node3D>("DiceHolder");
        throwLocationBall = FindChild("DiceTable").FindChild<ThrowLocationBall>("ThrowLocationBall");
        throwLocationDiceHolder = throwLocationBall.diceHolder;
        packedRootDice = GD.Load<PackedScene>("res://Scenes/root_dice.tscn");
        cameraController = this.FindChild<CameraController>("CameraController");
        label = this.FindChild<Label>("Label");
        gameStateManager = new GameStateManager();
        mousePosition = Vector2.Zero;

        ReadyDiceForThrow();
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
                ReadyDiceForThrow();
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
            if(diceCollection.IsDoneRolling() && !cameraController.IsAnimationPlaying())
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

        if(label.Text != gameState.ToString())
        {
            label.Text = gameState.ToString();
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
                MouseTools.GetCollisionFromMouseClick(mousePosition, mouseButton, this));

            if(selectedDice is not null) {selectedDice.SelectDice();}
        }
    }

    public RootDice HandleMouseClickOnObject(GodotObject obj)
    {
        var dice =  diceCollection.GetDiceEqualTo(obj);
        return dice is not null ? dice : null; 
    }

    public void HandleDicePhysics()
    {
        if(gameStateManager.GetState() is GameState.RollReady)
        {
            diceCollection.diceList.ForEach(
                x => x.GlobalPosition = throwLocationBall.throwLocation.GlobalPosition);
        }        
    }

    public void SetDicePositionForThrow()
    {
        if(diceCollection.diceList.Count == 0)
        {
            for(int i = 0; i < diceAmount; i++)
            {
                var dice = packedRootDice.Instantiate<RootDice>();
                diceCollection.diceList.Add(dice);
            }
            diceCollection.SetParent(diceHolder);
        }
        else
        {
            diceCollection.DisableCollision();
            diceCollection.FreezeDice();
        }

        foreach(RootDice dice in diceCollection.diceList)
        {
            dice.Rotate(
                new Vector3(GD.Randf(), GD.Randf(), GD.Randf()).Normalized(),
                GD.Randf()*(2*Mathf.Pi)
            );
        }
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

    public void ReadyDiceForThrow()
    {
        SetDicePositionForThrow();
        SetDiceVelocityForThrow();
    }

    public void ThrowDice()
    {
        diceCollection.UnfreezeDice();
        diceCollection.EnableCollision();
        diceCollection.ThrowDice();
    }

}
