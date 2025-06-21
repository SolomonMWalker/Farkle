public class PlayerManager
{
    public DiceCollection DiceCollection { get; private set; }
    public int ScoreAttemptsPerStage { get; private set; } = 5;
    public int RerollsPerStage { get; private set; } = 6;

    public PlayerManager()
    {
        CreateDiceCollection();
    }
    
    public void CreateDiceCollection()
    {
        DiceCollection = DiceCollection.InstantiateDiceCollection(Configuration.ConfigValues.NumOfStartingDice);
    }
}