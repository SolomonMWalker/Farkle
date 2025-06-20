using System.Collections.Generic;
using System.Linq;

public class ScoreManager
{
    public readonly ScoreRuleCollection scoreRules = ScoreRuleCollection.GetDefaultRules();
    private ScoreWithUnusedDice CalculateScore(DiceCollection collection)
    {
        var scorableResult = ScorableCollection.NewScorableCollection(collection);
        ScoreWithUnusedDice scoreWithUnusedDice = new(-1, []);
        foreach (IScoreRule scoreRule in scoreRules.scoreRuleList)
        {
            var newScore = scoreRule.GetScore(scorableResult);
            if (newScore.Score > scoreWithUnusedDice.Score)
            {
                scoreWithUnusedDice = new(newScore.Score, newScore.UnusedDice);
            }
        }

        if (scoreWithUnusedDice.Score == -1 || !scoreWithUnusedDice.UnusedDice.Any()) { return scoreWithUnusedDice; }

        var moreScoreResult = CalculateScore(new DiceCollection(scoreWithUnusedDice.UnusedDice));
        if (moreScoreResult.Score == -1) { return scoreWithUnusedDice; }
        scoreWithUnusedDice = new(moreScoreResult.Score + scoreWithUnusedDice.Score, moreScoreResult.UnusedDice);

        return scoreWithUnusedDice;
    }

    public CalculateScoreResult TryGetScore(DiceCollection dc)
    {
        if (dc is not null && dc.diceList.IsEmpty)
        {
            return new CalculateScoreResult(false, -1, CalculateScoreResultType.NoDiceInDiceCollection, null);
        }
        var calcScore = CalculateScore(dc);
        if (calcScore.Score == -1)
        {
            return new CalculateScoreResult(false, -1, CalculateScoreResultType.NoScorableDice,
                new DiceCollection(calcScore.UnusedDice));
        }
        return calcScore.UnusedDice.Any() ?
        new CalculateScoreResult(true, calcScore.Score, CalculateScoreResultType.HasUnusedScoreDice,
            new DiceCollection(calcScore.UnusedDice)) :
        new CalculateScoreResult(true, calcScore.Score, CalculateScoreResultType.Success, null);
    }
}

public record CalculateScoreResult(
    bool Scored, int Score, CalculateScoreResultType? ResultType, DiceCollection UnusedDice
);

public record ScoreWithUnusedDice(int Score, IEnumerable<RootDice> UnusedDice);

public enum CalculateScoreResultType {
    Success,
    HasUnusedScoreDice,
    NoScorableDice,
    NoDiceInDiceCollection
}