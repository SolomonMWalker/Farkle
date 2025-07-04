using Godot;

public partial class DiceFace() : Node3D
{
    private const string DiceFaceTextureFolder = "res://Resources/Textures/DiceFaces/";

    [Export]
    public int numberValue;

    public ModifyableValue<DiceFaceValue> DiceFaceValue { get; private set; }
    public RootDice AssociatedDice { get; private set; }
    public int Number => (_overridden ? _overrideDiceFaceValue.numberValue : DiceFaceValue.Value.numberValue) ?? 0;

    private bool _isDebug = false;
    private Sprite3D sprite3D;
    private DiceFaceTextureColor color = DiceFaceTextureColor.Black;

    #region Debug

    private bool _overridden = false;
    private DiceFaceValue _overrideDiceFaceValue;
    public void Override(DiceFaceValue dfValue)
    {
        if (_isDebug)
        {
            _overrideDiceFaceValue = dfValue;
            _overridden = true;
            LoadTexture();
        }
    }

    public void EndOverride()
    {
        if (_isDebug)
        {
            _overridden = false;
            _overrideDiceFaceValue = null;
            LoadTexture();
        }
    }

    #endregion

    public override void _Ready()
    {
        base._Ready();
        AssociatedDice = GetParent<Node3D>().GetParent<RootDice>();
        sprite3D = this.GetChildByName<Sprite3D>("Sprite3D");
        DiceFaceValue = new ModifyableValue<DiceFaceValue>(new DiceFaceValue(numberValue));
        _isDebug = Configuration.ConfigValues.IsDebug;
        LoadTexture();
    }

    public void LoadTexture()
    {
        sprite3D.Texture = GD.Load<Texture2D>(DiceFaceTextureFolder + $"PixelOldSchool/{Number}.png");
        sprite3D.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        sprite3D.Modulate = Colors.Black;
    }
}

public enum DiceFaceTextureColor
{
    White,
    Black
}
