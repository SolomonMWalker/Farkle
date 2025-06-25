public class RoundManager
{
    private const int StartingScoreAttemptsLeft = 1;
    public PlayerManager PlayerManager { get; private set; }
    public int CurrentScoreTries { get; private set; }
    public int RoundScore { get; private set; } = 0;

    public RoundManager(PlayerManager playerManager)
    {
        PlayerManager = playerManager;
        Reset();
    }

    public int AddToRoundScore(int scoreToAdd)
    {
        RoundScore += scoreToAdd;
        return RoundScore;
    }

    public void Reset()
    {
        RoundScore = 0;
        CurrentScoreTries = PlayerManager.ScoreTriesPerRound;
    }

    private bool TrySubtractScoreAttempt(int amount)
    {
        CurrentScoreTries -= amount;
        if (CurrentScoreTries <= 0)
        {
            return false;
        }
        return true;
    }

    public void StartNewRound() => Reset();
    public bool TryToUseScoreAttempt() => TrySubtractScoreAttempt(1);
    public bool TryToFarkle() => TrySubtractScoreAttempt(1);
}