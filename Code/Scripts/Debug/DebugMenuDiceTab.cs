using Godot;
using System.Collections.Generic;

public partial class DebugMenuDiceTab : MarginContainer
{
    private const string _diceEntrySceneRelPath = "res://Scenes/Debug/debug_menu_dice_tab_entry.tscn";
    private PackedScene _diceEntryPScene;
    private GameController gController;
    private List<DebugMenuDiceTabEntry> DiceEntries { get; set; } = [];
    public DiceCollection DiceCollection { get; private set; } = new DiceCollection();
    private bool Clickable { get; set; } = true;

    [Export]
    public VBoxContainer diceMenuVBox;
    [Export]
    public Button overrideButton;
    [Export]
    public Button endOverrideButton;

    public override void _Ready()
    {
        base._Ready();
        _diceEntryPScene = GD.Load<PackedScene>(_diceEntrySceneRelPath);
        overrideButton.ButtonDown += OverrideResultOfRollDiceFaces;
        endOverrideButton.ButtonDown += EndOverride;
    }

    public void Initialize(GameController gameController)
    {
        gController = gameController;
    }

    public void ResetDiceCollection()
    {
        DiceCollection = new DiceCollection();
        DeleteDiceEntries();
    }

    public void SetNewDiceCollection(DiceCollection diceCollection)
    {
        DeleteDiceEntries();
        DiceCollection = new DiceCollection(diceCollection);
        foreach (RootDice d in DiceCollection.diceList)
        {
            var diceEntryScene = _diceEntryPScene.Instantiate<DebugMenuDiceTabEntry>();
            DiceEntries.Add(diceEntryScene);
            diceMenuVBox.AddChild(diceEntryScene);
            diceEntryScene.Initialize(d);
        }
    }

    public void DeleteDiceEntries()
    {
        if (DiceEntries is null || DiceEntries.Count == 0) { return; }
        foreach (var child in DiceEntries)
        {
            child.QueueFree();
        }
        DiceEntries = [];
    }

    public void AddDiceEntries(DiceCollection diceCollection)
    {
        AddDiceEntries(diceCollection.diceList);
    }

    public void AddDiceEntries(IEnumerable<RootDice> dice)
    {
        foreach (RootDice d in dice)
        {
            AddDiceEntry(d);
        }
    }

    public void AddDiceEntry(RootDice dice)
    {
        DiceCollection = DiceCollection.AddDice(dice);
        var diceEntryScene = _diceEntryPScene.Instantiate<DebugMenuDiceTabEntry>();
        GD.Print(diceEntryScene.ToString());
        DiceEntries.Add(diceEntryScene);
        diceMenuVBox.AddChild(diceEntryScene);
        diceEntryScene.Initialize(dice);
    }

    public void OverrideResultOfRollDiceFaces()
    {
        if (gController.GameStateManager.GameState != GameState.SelectDice) { return; }
        foreach (var entry in DiceEntries)
        {
            var topDiceFace = entry.Dice.ResultOfRoll;
            topDiceFace.EndOverride();
            var overrideScore = int.Parse(entry.GetOverrideDiceFaceValue());
            topDiceFace.Override(new DiceFaceValue(overrideScore));
        }
        gController.BuildAndSetScoreText();
    }

    public void EndOverride()
    {
        foreach (var entry in DiceEntries)
        {
            entry.Dice.EndOverride();
        }
        if (gController.GameStateManager.GameState is GameState.SelectDice)
        {
            gController.BuildAndSetScoreText();
        }
    }

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
    }
}
