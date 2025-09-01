using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RootDice : RigidBody3D
{
    private const string RootDiceMaterialPath = "res://Resources/Materials/RootDiceMaterial.tres";
    private const string RootDiceSelectedMaterialPath = "res://Resources/Materials/RootDiceSelectedMaterial.tres";
    private const string RootDiceFlashRedMaterialPath = "res://Resources/Materials/RootDiceFlashRedMaterial.tres";
    private const string RootDiceRelPath = "res://Scenes/root_dice.tscn";
    private static PackedScene packedRootDice = null;

    public bool selected;
    public bool temporary;
    public RootDice AssociatedPermanentDice { get; private set; } = null;
    public List<DiceModifier> Modifiers { get; private set; } = [];
    public DiceFaceCollection diceFaceCollection;

    private CollisionShape3D collisionShape;
    private MeshInstance3D meshInstance;
    private Material rootDiceMaterial, rootDiceSelectedMaterial, rootDiceFlashRedMaterial;
    private AnimationPlayer animationPlayer;
    private Node3D corner, diceFacesParent;
    public Node3D DiceFacesParent { get; }
    private Vector3 velocityUponThrow;
    public DiceFace ResultOfRoll { get; private set; }
    private bool _isDebug = false, _isInitialized = false;
    public bool IsDebug { get; }
    private int colliderId;
    private float edgelength;

    public static RootDice InstantiateRootDice()
    {
        packedRootDice ??= GD.Load<PackedScene>(RootDiceRelPath);
        return packedRootDice.Instantiate<RootDice>();
    }

    public virtual RootDice DeepCopy()
    {
        return InstantiateRootDice();
    }

    public override void _Ready()
    {
        base._Ready();
        collisionShape = this.GetChildByName<CollisionShape3D>("CollisionShape3D");
        meshInstance = this.GetChildByName<MeshInstance3D>("MeshInstance3D");
        corner = this.GetChildByName<Node3D>("Corner");
        diceFacesParent = this.GetChildByName<Node3D>("DiceFaces");
        animationPlayer = this.GetChildByName<AnimationPlayer>("AnimationPlayer");
        edgelength = HelperMethods.GetSideLengthFromHalfDiagonal(Position.DistanceTo(corner.Position));
        rootDiceMaterial = GD.Load<Material>(RootDiceMaterialPath);
        rootDiceSelectedMaterial = GD.Load<Material>(RootDiceSelectedMaterialPath);
        rootDiceFlashRedMaterial = GD.Load<Material>(RootDiceFlashRedMaterialPath);
        _isDebug = Configuration.ConfigValues.IsDebug;
        temporary = false;
        collisionShape.Disabled = true;
        Freeze = true;
        FreezeMode = FreezeModeEnum.Static;
        selected = false;
        diceFaceCollection = new DiceFaceCollection
        {
            faces = [.. diceFacesParent.GetChildrenOfType<DiceFace>()]
        };
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

    public void MultiplyVelocityByThrowForceValue(double throwForceValue) => velocityUponThrow = velocityUponThrow * (float)throwForceValue;

    public void Throw()
    {
        LinearVelocity = velocityUponThrow;
    }

    public bool IsDoneRolling()
    {
        var velocityIsCloseToZero = Mathf.Abs(LinearVelocity.Length()) < 0.05;
        var angularVelocityIsClostToZero = Mathf.Abs(AngularVelocity.Length()) < 0.05;
        var lowestFaceHeightIsCloseToTable =
            Mathf.Abs(diceFaceCollection.GetHeightOfLowestFace()) < (edgelength / 2) + 0.05;
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

    public void DisableCollision() { collisionShape.Disabled = true; }
    public void EnableCollision() { collisionShape.Disabled = false; }
    public DiceFace GetResultOfRoll()
    {
        ResultOfRoll = diceFaceCollection.GetResultOfRoll();
        return ResultOfRoll;
    }

    public void ToggleSelectDice()
    {
        if (selected)
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

    public List<ulong> GetDiceFaceInstanceIds() => diceFaceCollection.GetDiceFaceInstanceIds();
    public void EndOverride() => diceFaceCollection.EndOverrides();

    public void AddModifier(DiceModifier diceModifier) => Modifiers.Add(diceModifier);
    public void RemoveModifier(DiceModifier diceModifier) => Modifiers.Remove(Modifiers.FirstOrDefault(m => m.Id == diceModifier.Id));
}
