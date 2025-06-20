using System.Collections.Generic;

public class StageManager
{
    private const int NumOfStages = 1;
    private const int StartingScoreAttemptsLeft = 4;
    private const int StartingRetriesLeft = 6;
    private const int RetriesUsedToRerollOneDice = 1;
    private const int RetriesUsedToRerollAllDice = 3;

    public int RetriesLeft { get; private set; } = StartingRetriesLeft;
    public int ScoreAttemptsLeft { get; private set; } = StartingScoreAttemptsLeft;
    public int StageScore { get; private set; } = 0;
    public int CurrentStageIndex { get; private set; } = 0;
    public List<Stage> Stages { get; private set; } = [];

    public StageManager()
    {
        SetUpTestStages();
    }

    private bool TrySubtractScoreAttempt(int amount)
    {
        ScoreAttemptsLeft -= amount;
        if (ScoreAttemptsLeft <= 0)
        {
            return false;
        }
        return true;
    }

    private bool TrySubtractRetries(int amount)
    {
        if (RetriesLeft - amount < 0)
        {
            return false;
        }
        RetriesLeft -= amount;
        return true;
    }

    public bool TryToUseScoreAttempt() => TrySubtractScoreAttempt(1);
    public bool TryToFarkle() => TrySubtractScoreAttempt(1);
    public bool TryRerollSingleDice(int numberOfDice) => TrySubtractRetries(numberOfDice);
    public bool TryRerollAllDice => TrySubtractRetries(3);

    public Stage GetCurrentStage() => Stages[CurrentStageIndex];
    public int GetCurrentStageScoreToWin() => Stages[CurrentStageIndex].ScoreToWin;
    public int GetCurrentStageNumber() => CurrentStageIndex + 1;
    public int GetNumberOfStages() => Stages.Count;
    public bool IsStageScoreHigherThanScoreToWin() => StageScore >= Stages[CurrentStageIndex].ScoreToWin;

    public void SetUpTestStages()
    {
        for (int i = 1; i <= NumOfStages; i++)
        {
            Stages.Add(new Stage(i, 20000));
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
            RetriesLeft = StartingRetriesLeft;
            ScoreAttemptsLeft = StartingScoreAttemptsLeft;
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
        RetriesLeft = StartingRetriesLeft;
        ScoreAttemptsLeft = StartingScoreAttemptsLeft;
    }    
}