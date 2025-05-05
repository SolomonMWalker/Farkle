using System.Collections.Generic;
using System.Linq;
using Godot;

public static class DiceCollectionScore{
    public static readonly ScoreRuleCollection scoreRules = ScoreRuleCollection.GetDefaultRules();

    public static CalculateScoreResult CalculateScore(DiceCollection collection)
    {
        var scorableResult = ScorableCollection.NewScorableCollection(collection);
        CalculateScoreResult calculatedScoreResult = new (-1, []);
        foreach(IScoreRule scoreRule in scoreRules.scoreRuleList)
        {
            var newScore = scoreRule.GetScore(scorableResult);
            if(newScore.Score > calculatedScoreResult.Score) 
            {
                calculatedScoreResult = new (newScore.Score, newScore.UnusedDice);
            }
        }

        if(calculatedScoreResult.Score == -1 || !calculatedScoreResult.UnusedDice.Any()) {return calculatedScoreResult;}
        
        var moreScoreResult = CalculateScore(new DiceCollection(calculatedScoreResult.UnusedDice));
        if(moreScoreResult.Score == -1) {return calculatedScoreResult;}
        calculatedScoreResult = new(moreScoreResult.Score + calculatedScoreResult.Score, moreScoreResult.UnusedDice);        

        return calculatedScoreResult;
    }
}

public record CalculateScoreResult(int Score, IEnumerable<RootDice> UnusedDice);