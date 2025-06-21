using System.Collections.Generic;
using Godot;

public partial class DebugMenu : MarginContainer
{
    public GameController GameController { get; private set; }
    public PanelContainer MenuContentsContainer { get; private set; }
    public DebugMenuDiceTab DiceTab { get; private set; }
    public Button MinimizeButton { get; private set; }

    private bool Minimized { get; set; } = false;

    public override void _Ready()
    {
        base._Ready();
        MenuContentsContainer = (PanelContainer)FindChild("*MenuContentsContainer");
        GD.Print($"Is MenuContainer null {MenuContentsContainer}");
        DiceTab = (DebugMenuDiceTab)MenuContentsContainer.FindChild("*DiceTab");
        MinimizeButton = (Button)FindChild("*MinimizeButton");
        MinimizeButton.Pressed += MinimizeButtonPressed;
    }

    public void Initialize(GameController gc)
    {
        GameController = gc;
        DiceTab.Initialize(gc);
    }

    public void ResetDiceCollection() => DiceTab.ResetDiceCollection();
    public void SetNewDiceCollection(DiceCollection dc) => DiceTab.SetNewDiceCollection(dc);
    public void AddDice(RootDice dice) => DiceTab.AddDiceEntry(dice);
    public void AddDice(DiceCollection dc) => DiceTab.AddDiceEntries(dc);
    public void AddDice(IEnumerable<RootDice> dice) => DiceTab.AddDiceEntries(dice);
    public void MinimizeButtonPressed()
    {
        if (!Minimized)
        {
            Minimized = true;
            MenuContentsContainer.Visible = false;
            MenuContentsContainer.MouseFilter = MouseFilterEnum.Ignore;
            DiceTab.SetClickable(false);
        }
        else
        {
            Minimized = false;
            MenuContentsContainer.Visible = true;
            MenuContentsContainer.MouseFilter = MouseFilterEnum.Stop;
            DiceTab.SetClickable(true);
        }
    }
}