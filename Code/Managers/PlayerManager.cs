using System.Collections.Generic;

public class PlayerManager
{
    public DiceCollection DiceCollection { get; private set; }
    public List<BaseModifier> MasterModifierList { get; private set; } = [];
    public ModifyableInt ScoreTriesPerRound { get; private set; }
    public ModifyableInt RerollsPerStage { get; private set; }
    public ModifyableInt NumberOfPersistentDice { get; private set; }
    public PlayerManager()
    {
        ScoreTriesPerRound = new ModifyableInt(Configuration.ConfigValues.ScoreTriesPerRound);
        RerollsPerStage = new ModifyableInt(Configuration.ConfigValues.RerollsPerStage);
        NumberOfPersistentDice = new ModifyableInt(Configuration.ConfigValues.NumOfStartingDice);
        CreateDiceCollection();
    }
    
    public void CreateDiceCollection()
    {
        DiceCollection = DiceCollection.InstantiateDiceCollection(NumberOfPersistentDice.ModifiedValue);
    }
}