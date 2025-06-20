public class RoundManager
{
    private const int StartingScoreAttemptsLeft = 1;

    public int RoundScore { get; private set; } = 0;
    
    public int AddToRoundScore(int scoreToAdd)
    {
        RoundScore += scoreToAdd;
        return RoundScore;
    }

    public void Reset()
    {
        RoundScore = 0;
    }
}