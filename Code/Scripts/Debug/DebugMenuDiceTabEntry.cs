using Godot;

public partial class DebugMenuDiceTabEntry : MarginContainer
{
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
        DiceFaceEdit.Text = Dice.ResultOfRoll?.Number.ToString();
    }

    public string GetOverrideDiceFaceValue() => DiceFaceEdit.Text;
}
