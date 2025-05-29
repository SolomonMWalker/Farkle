using Godot;

public partial class RootDiceFace(DiceFaceValue diceFaceValue, Transform3D transform) : Node3D
{
    protected DiceFaceValue diceFaceValue = diceFaceValue;
    protected Label3D _label;
    protected RootDice _associatedDice;
    protected const DiceFaceType diceFaceType = DiceFaceType.Root;
    public RootDice AssociatedDice { get => _associatedDice; }
    public virtual int Number => diceFaceValue.numberValue ?? 0;

    public override void _Ready()
    {
        base._Ready();
        Transform = transform;
        _label = this.FindChild<Label3D>("Label");
        _associatedDice = GetParent<Node3D>().GetParent<RootDice>();
    }

    public virtual void ChangeNumber(int newNumber)
    {
        diceFaceValue.numberValue = newNumber;
        _label.Text = Number.ToString();
    }

    protected void SetLabelText(string text) => _label.Text = text;
}
