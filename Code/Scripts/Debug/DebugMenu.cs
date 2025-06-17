using System.Collections.Generic;
using Godot;

public partial class DebugMenu : MarginContainer
{
    public GameController GameController { get; private set; }
    public TabContainer MenuContainer { get; private set; }
    public DebugMenuDiceTab DiceTab { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        MenuContainer = (TabContainer)FindChild("TabContainer");
        DiceTab = MenuContainer.GetChildByName<DebugMenuDiceTab>("Dice");
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
}