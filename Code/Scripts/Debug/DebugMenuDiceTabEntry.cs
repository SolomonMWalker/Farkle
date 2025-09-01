using Godot;

public partial class DebugMenuDiceTabEntry : MarginContainer
{
    private bool Clickable { get; set; } = true;
    public RootDice Dice { get; private set; }
    private Label DiceLabel { get; set; }
    private LineEdit DiceFaceEdit { get; set; }

    public override void _Ready()
    {
        base._Ready();
        var hBoxContainer = (HBoxContainer)FindChild("HBoxContainer");
        DiceLabel = hBoxContainer.GetChild<Label>(0);
        DiceFaceEdit = hBoxContainer.GetChild<LineEdit>(1);
    }

    public void Initialize(RootDice dice)
    {
        Dice = dice;
        DiceLabel.Text = Dice.GetInstanceId().ToString();
        DiceFaceEdit.Text = Dice.ResultOfRoll?.OriginalNumber.ToString();
    }

    public string GetOverrideDiceFaceValue() => DiceFaceEdit.Text;
    public void SetClickable(bool clickable)
    {
        Clickable = clickable;
        MouseFilterEnum mFilter;
        if (Clickable)
        {
            mFilter = MouseFilterEnum.Pass;
        }
        else
        {
            mFilter = MouseFilterEnum.Ignore;
        }

        MouseFilter = mFilter;
        DiceLabel.MouseFilter = mFilter;
        DiceFaceEdit.MouseFilter = mFilter;
    }
}
