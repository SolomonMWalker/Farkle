using Godot;

public partial class DiceFace() : Node3D
{
    [Export]
    public int numberValue;

    private bool _isDebug = false;
    private DiceFaceValue _diceFaceValue;
    private Label3D _label;
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
            SetLabelText(Number.ToString());
        }
    }

    public void EndOverride()
    {
        if (_isDebug)
        {
            _overridden = false;
            _overrideDiceFaceValue = null;
            SetLabelText(Number.ToString());
        }
    }

    #endregion

    public override void _Ready()
    {
        base._Ready();
        _associatedDice = GetParent<Node3D>().GetParent<RootDice>();
        _label = this.FindChild<Label3D>("Label");
        _diceFaceValue = new DiceFaceValue(numberValue);
        SetLabelText(_diceFaceValue.numberValue.Value.ToString());
    }

    public void SetDebug(bool isDebug)
    {
        _isDebug = isDebug;
    }

    private void SetLabelText(string text) => _label.Text = text;
}
