using System.Collections.Generic;
using System.Linq;

public class DiceCollectionScore{
    public ScoreRuleCollection scoreRules = ScoreRuleCollection.GetDefaultRules();
    public ScorableCollection scorableResult;
    public int score = 0;

    public int CalculateScore()
    {
        int calculatedScore = 0;
        foreach(IScoreRule scoreRule in scoreRules.scoreRuleList)
        {
            //need to fix this with new scoring return
            // var newScore = scoreRule.GetScore(scorableResult);
            // if(newScore > calculatedScore)
            // {
            //     calculatedScore = newScore;
            // }
        }
        
        if(calculatedScore > score)
        {
            score = calculatedScore;
        }

        return score;
    }

    public DiceCollectionScore(DiceCollection collection)
    {
        scorableResult = ScorableCollection.NewScorableCollection(collection);
        score = CalculateScore();
    }


    public int RecalculateScore(DiceCollection collection)
    {
        score = 0;
        scorableResult = ScorableCollection.NewScorableCollection(collection);

        if(scorableResult == null) {return 0;}

        score = CalculateScore();
        return score;
    }
}