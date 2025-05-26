using Godot;

public partial class RootDice : RigidBody3D
{
    public bool selected;

    private CollisionShape3D collisionShape;
    private MeshInstance3D meshInstance;
    private Material rootDiceMaterial, rootDiceSelectedMaterial, rootDiceFlashRedMaterial;
    private AnimationPlayer animationPlayer;
    private Node3D corner;
    private Vector3 velocityUponThrow;
    private DiceFaceCollectionPerDice diceFaceCollection;
    private int colliderId;
    private float edgelength;
    private const string RootDiceMaterialPath = "res://Resources/Materials/RootDiceMaterial.tres";
    private const string RootDiceSelectedMaterialPath = "res://Resources/Materials/RootDiceSelectedMaterial.tres";
    private const string RootDiceFlashRedMaterialPath = "res://Resources/Materials/RootDiceFlashRedMaterial.tres";

    public override void _Ready()
    {
        base._Ready();
        collisionShape = this.FindChild<CollisionShape3D>("CollisionShape3D");
        meshInstance = this.FindChild<MeshInstance3D>("MeshInstance3D");
        corner = this.FindChild<Node3D>("Corner");
        animationPlayer = this.FindChild<AnimationPlayer>("AnimationPlayer");
        edgelength = HelperMethods.GetSideLengthFromHalfDiagonal(Position.DistanceTo(corner.Position));
        rootDiceMaterial = GD.Load<Material>(RootDiceMaterialPath);
        rootDiceSelectedMaterial = GD.Load<Material>(RootDiceSelectedMaterialPath);
        rootDiceFlashRedMaterial = GD.Load<Material>(RootDiceFlashRedMaterialPath);        
        collisionShape.Disabled = true;
        Freeze = true;
        FreezeMode = FreezeModeEnum.Static;
        selected = false;
        SetupDiceFaces();
    }

    public void SetupDiceFaces()
    {
        var diceFaceParent = FindChild("DiceFaces");
        var diceFaces = diceFaceParent.GetChildren<DiceFace>();
        diceFaceCollection = new ();
        diceFaceCollection.faces = diceFaces;
    }

    public bool PointTooClose(Vector3 point, float margin)
    {
        //if point is sqrt(sidelength) + margin or closer, return true
        //basically a sphere around the cube of the dice
        //should work for other dice sizes as well
        return Position.DistanceTo(point) > ((Mathf.Sqrt2 * edgelength) + margin);
    }

    public void SetVelocityUponThrow(Vector3 velocity)
    {
        velocityUponThrow = velocity;
    }

    public void Throw()
    {
        LinearVelocity = velocityUponThrow;
    }

    public bool IsDoneRolling() {
        var velocityIsCloseToZero = Mathf.Abs(LinearVelocity.Length()) < 0.05;
        var angularVelocityIsClostToZero = Mathf.Abs(AngularVelocity.Length()) < 0.05;
        var lowestFaceHeightIsCloseToTable = 
            Mathf.Abs(diceFaceCollection.GetHeightOfLowestFace()) < (edgelength/2) + 0.05 ;
        return velocityIsCloseToZero && angularVelocityIsClostToZero && lowestFaceHeightIsCloseToTable;
    }

    public void TurnOff()
    {
        DisableCollision();
        Freeze = true;
    }

    public void TurnOn()
    {
        EnableCollision();
        Freeze = false;
    }

    public void DisableCollision() {collisionShape.Disabled = true;}
    public void EnableCollision() {collisionShape.Disabled = false;}
    public DiceFace GetResultOfRoll() {return diceFaceCollection.GetResultOfRoll();}

    public void ToggleSelectDice()
    {
        if(selected)
        {
            meshInstance.SetSurfaceOverrideMaterial(0, rootDiceMaterial);
            selected = false;
        }
        else
        {
            meshInstance.SetSurfaceOverrideMaterial(0, rootDiceSelectedMaterial);
            selected = true;
        }        
    }

    public void SelectDice()
    {
        meshInstance.SetSurfaceOverrideMaterial(0, rootDiceSelectedMaterial);
        selected = true;
    }

    public void UnselectDice()
    {
        meshInstance.SetSurfaceOverrideMaterial(0, rootDiceMaterial);
        selected = false;
    }

    public void FlashRed()
    {
        animationPlayer.Play("SelectedFlashRed");
    }
}
