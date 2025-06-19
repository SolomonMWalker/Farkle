using System.Collections.Generic;

public class StageManager
{
    public int StageScore { get; private set; } = 0;
    public int CurrentStageIndex { get; private set; } = 0;
    public List<Stage> Stages { get; private set; } = [];

    public StageManager()
    {
        SetUpTestStages();
    }

    public void SetUpTestStages()
    {
        for (int i = 1; i <= 2; i++)
        {
            Stages.Add(new Stage(i, 1000));
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
            return true;
        }
        return false;
    }

    public Stage GetCurrentStage() => Stages[CurrentStageIndex];
    public int GetCurrentStageScoreToWin() => Stages[CurrentStageIndex].ScoreToWin;
    public int GetCurrentStageNumber() => CurrentStageIndex + 1;
    public int GetNumberOfStages() => Stages.Count;
    public bool IsStageScoreHigherThanScoreToWin() => StageScore >= Stages[CurrentStageIndex].ScoreToWin;
}