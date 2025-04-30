using System.Collections.Generic;
using System.Linq;

public class DiceCollectionScore{
    public ScoreRuleCollection scoreRules = ScoreRuleCollection.GetDefaultRules();
    public ScorableResult scorableResult;
    public int score = 0;

    public int CalculateScore()
    {
        int calculatedScore = 0;
        foreach(IScoreRule scoreRule in scoreRules.scoreRuleList)
        {
            var newScore = scoreRule.GetScore(scorableResult);
            if(newScore > calculatedScore)
            {
                calculatedScore = newScore;
            }
        }
        
        if(calculatedScore > score)
        {
            score = calculatedScore;
        }

        return score;
    }

    public DiceCollectionScore(DiceCollection collection)
    {
        scorableResult = ScorableResult.NewScorableResult(collection);
        score = CalculateScore();
    }


    public int RecalculateScore(DiceCollection collection)
    {
        score = 0;
        scorableResult = ScorableResult.NewScorableResult(collection);

        if(scorableResult == null) {return 0;}

        score = CalculateScore();
        return score;
    }
}

public class ScorableResult{
    public HashSet<int> set;
    public Dictionary<int, int> dict;

    private ScorableResult(DiceCollection diceCollection)
    {
        var results = diceCollection.GetResultOfRoll();
        set = results.ToHashSet();
        dict = new Dictionary<int, int>();
        foreach(int result in results)
        {
            if(dict.Keys.Contains(result))
            {
                dict[result] += 1;
            }
            else
            {
                dict[result] = 1;
            }
        }
    }

    public static ScorableResult NewScorableResult(DiceCollection diceCollection)
    {
        if(diceCollection == null || diceCollection.diceList.Count == 0)
        {
            return null;
        }
        else
        {
            return new ScorableResult(diceCollection);
        }
    }
}

public class ScoreRuleCollection
{
    public List<IScoreRule> scoreRuleList;

    public ScoreRuleCollection()
    {
        scoreRuleList = new List<IScoreRule>();
    }

    public ScoreRuleCollection(List<IScoreRule> rules)
    {
        scoreRuleList = rules;
    }

    public static ScoreRuleCollection GetDefaultRules()
    {
        return new ScoreRuleCollection([
            new SingleOneScoreRule()
        ]);
    }
}

public interface IScoreRule{
    public int GetScore(ScorableResult scorableResult);
}

public class SingleOneScoreRule : IScoreRule{
    public int GetScore(ScorableResult scorableResult)
    {
        if(scorableResult.dict == null ||
            !scorableResult.dict.ContainsKey(1))
        {
            return 0;
        }

        var numOfOnes = scorableResult.dict[1];
        if(numOfOnes == 1)
        {
            return 100;
        }
        else if (numOfOnes == 2)
        {
            return 200;
        }
        else if (numOfOnes == 3)
        {
            return 1000;
        }
        else if (numOfOnes > 3)
        {
            return 1000 * (numOfOnes - 2);
        }
        else
        {
            return 0;
        }
    }
}