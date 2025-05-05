using System.Collections.Generic;
using System.Linq;
using Godot;

public static class DiceCollectionScore{
    public static readonly ScoreRuleCollection scoreRules = ScoreRuleCollection.GetDefaultRules();

    public static int CalculateScore(DiceCollection collection)
    {
        var scorableResult = ScorableCollection.NewScorableCollection(collection);
        int calculatedScore = -1;
        HashSet<RootDice> diceUsed = [];
        foreach(IScoreRule scoreRule in scoreRules.scoreRuleList)
        {
            var newScore = scoreRule.GetScore(scorableResult);
            if(newScore.Item1 > calculatedScore)
            {
                calculatedScore = newScore.Item1;
                diceUsed = newScore.Item2.ToHashSet();
            }
        }

        if(calculatedScore == -1) {return -1;}

        if(diceUsed.Count < collection.diceList.Count)
        {
            var moreScore = CalculateScore(collection.RemoveDice(diceUsed));
            if(moreScore == -1) {return calculatedScore;}
            calculatedScore += moreScore;
        }

        return calculatedScore;
    }
}