public class RoundManager
{
    public int RoundScore { get; private set; } = 0;
    public int ScoreAttemptsLeft { get; private set; } = 3;
    public int RetriesLeft { get; private set; } = 5;

    public int AddToRoundScore(int scoreToAdd)
    {
        RoundScore += scoreToAdd;
        return RoundScore;
    }

    public int SubtractScoreAttempt(int amount)
    {
        ScoreAttemptsLeft -= amount;
        return ScoreAttemptsLeft;
    }

    public void Farkle()
    {
        SubtractScoreAttempt(1);
    }
}