public class StageManager
{
    public int StageScore { get; private set; }

    public int AddToStageScore(int scoreToAdd)
    {
        StageScore += scoreToAdd;
        return StageScore;
    }
}