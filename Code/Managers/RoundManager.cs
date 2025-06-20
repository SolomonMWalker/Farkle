public class RoundManager
{
    private const int StartingScoreAttemptsLeft = 1;
    private const int StartingRetriesLeft = 5;

    public int RoundScore { get; private set; } = 0;
    public int ScoreAttemptsLeft { get; private set; } = StartingScoreAttemptsLeft;
    public int RetriesLeft { get; private set; } = StartingRetriesLeft;

    public int AddToRoundScore(int scoreToAdd)
    {
        RoundScore += scoreToAdd;
        return RoundScore;
    }

    public bool TrySubtractScoreAttempt(int amount)
    {
        ScoreAttemptsLeft -= amount;
        if (ScoreAttemptsLeft <= 0)
        {
            return false;
        }
        return true;
    }

    public bool TryToFarkle() => TrySubtractScoreAttempt(1);
    public void Reset()
    {
        RoundScore = 0;
        ScoreAttemptsLeft = StartingScoreAttemptsLeft;
        RetriesLeft = StartingRetriesLeft;
    }
}