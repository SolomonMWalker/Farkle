public class PlayerManager
{
    public DiceCollection DiceCollection { get; private set; }
    private ModifyableNumericalValue<int> _scoreTriesPerRound { get; set; }
    public int ScoreTriesPerRound
    {
        get => _scoreTriesPerRound.Value;
        set => _scoreTriesPerRound.ModifyValue(value);
    }
    private ModifyableNumericalValue<int> _rerollsPerStage { get; set; }
    public int RerollsPerStage
    {
        get => _rerollsPerStage.Value;
        set => _rerollsPerStage.ModifyValue(value);
    }
    private ModifyableNumericalValue<int> _numberOfPersistentDice { get; set; }
    public int NumberOfPersistentDice
    {
        get => _numberOfPersistentDice.Value;
        set => _numberOfPersistentDice.ModifyValue(value);
    }
    public PlayerManager()
    {
        _scoreTriesPerRound = new ModifyableNumericalValue<int>(Configuration.ConfigValues.ScoreTriesPerRound);
        _rerollsPerStage = new ModifyableNumericalValue<int>(Configuration.ConfigValues.RerollsPerStage);
        _numberOfPersistentDice = new ModifyableNumericalValue<int>(Configuration.ConfigValues.NumOfStartingDice);
        CreateDiceCollection();
    }
    
    public void CreateDiceCollection()
    {
        DiceCollection = DiceCollection.InstantiateDiceCollection(NumberOfPersistentDice);
    }
}