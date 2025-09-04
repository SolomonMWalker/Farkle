using System.Collections.Generic;

public class StageManager
{
    private const int NumOfStages = 1;
    private const int RetriesUsedToRerollOneDice = 1;
    private const int RetriesUsedToRerollAllDice = 3;

    public int StageScore { get; private set; } = 0;
    public int CurrentStageIndex { get; private set; } = 0;
    public int CurrentRerolls { get; private set; }
    public List<Stage> Stages { get; private set; } = [];
    public PlayerManager PlayerManager { get; private set; }

    public StageManager(PlayerManager playerManager)
    {
        PlayerManager = playerManager;
        CurrentRerolls = PlayerManager.RerollsPerStage.ModifiedValue;
        SetUpTestStages();
    }


    private bool TrySubtractRerolls(int amount)
    {
        if (CurrentRerolls - amount < 0)
        {
            return false;
        }
        CurrentRerolls = CurrentRerolls - amount;
        return true;
    }

    public void SetUpTestStages()
    {
        for (int i = 1; i <= NumOfStages; i++)
        {
            Stages.Add(new Stage(i, Configuration.ConfigValues.ScoreToWin));
        }
    }

    public int AddToStageScore(int scoreToAdd)
    {
        StageScore += scoreToAdd;
        return StageScore;
    }

    public bool TryGoToNextStage()
    {
        if (CurrentStageIndex + 1 <= (Stages.Count - 1))
        {
            CurrentStageIndex += 1;
            StageScore = 0;
            CurrentRerolls = PlayerManager.NumberOfPersistentDice.ModifiedValue;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        Stages = [];
        SetUpTestStages();
        StageScore = 0;
        CurrentStageIndex = 0;
        CurrentRerolls = PlayerManager.NumberOfPersistentDice.ModifiedValue;
    }

    public bool TryRerollSingleDice(int numberOfDice) => TrySubtractRerolls(numberOfDice);
    public bool TryRerollAllDice() => TrySubtractRerolls(3);
    public Stage GetCurrentStage() => Stages[CurrentStageIndex];
    public int GetCurrentStageScoreToWin() => Stages[CurrentStageIndex].ScoreToWin;
    public int GetCurrentStageNumber() => CurrentStageIndex + 1;
    public int GetNumberOfStages() => Stages.Count;
    public bool IsStageScoreHigherThanScoreToWin() => StageScore >= Stages[CurrentStageIndex].ScoreToWin;
}