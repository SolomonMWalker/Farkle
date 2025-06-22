using Godot;

public partial class DiceFace() : Node3D
{
    private const string DiceFaceTextureFolder = "res://Resources/Textures/DiceFaces/";

    [Export]
    public int numberValue;

    private bool _isDebug = false;
    private Sprite3D sprite3D;
    private DiceFaceTextureColor color = DiceFaceTextureColor.Black;
    private DiceFaceValue _diceFaceValue;
    private RootDice _associatedDice;
    public RootDice AssociatedDice { get => _associatedDice; }
    public int Number => (_overridden ? _overrideDiceFaceValue.numberValue : _diceFaceValue.numberValue) ?? 0;

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
        _associatedDice = GetParent<Node3D>().GetParent<RootDice>();
        sprite3D = this.GetChildByName<Sprite3D>("Sprite3D");
        _diceFaceValue = new DiceFaceValue(numberValue);
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
