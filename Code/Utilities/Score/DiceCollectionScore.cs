using System.Collections.Generic;
using System.Linq;
using Godot;

public class DiceCollectionScore{
    public ScoreRuleCollection scoreRules = ScoreRuleCollection.GetDefaultRules();
    public int score = 0;

    public int CalculateScore(DiceCollection collection)
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
                diceUsed = newScore.Item2;
            }
        }

        if(calculatedScore == -1) {return -1;}

        if(diceUsed.Count < collection.diceList.Count)
        {
            var moreScore = CalculateScore(collection.RemoveDice(diceUsed));
            if(moreScore == -1) {return score;}
            calculatedScore += moreScore;
        }
        
        if(calculatedScore > score)
        {
            score = calculatedScore;
        }

        return score;
    }

    public DiceCollectionScore(DiceCollection collection)
    {
        score = CalculateScore(collection);
    }


    public int RecalculateScore(DiceCollection collection)
    {
        var scorableResult = ScorableCollection.NewScorableCollection(collection);

        if(scorableResult == null) {return 0;}

        score = CalculateScore(collection);
        return score;
    }
}