using System.Collections.Generic;

public class StageManager
{
    private const int NumOfStages = 1;

    public int StageScore { get; private set; } = 0;
    public int CurrentStageIndex { get; private set; } = 0;
    public List<Stage> Stages { get; private set; } = [];

    public StageManager()
    {
        SetUpTestStages();
    }

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
    }

    public Stage GetCurrentStage() => Stages[CurrentStageIndex];
    public int GetCurrentStageScoreToWin() => Stages[CurrentStageIndex].ScoreToWin;
    public int GetCurrentStageNumber() => CurrentStageIndex + 1;
    public int GetNumberOfStages() => Stages.Count;
    public bool IsStageScoreHigherThanScoreToWin() => StageScore >= Stages[CurrentStageIndex].ScoreToWin;
}