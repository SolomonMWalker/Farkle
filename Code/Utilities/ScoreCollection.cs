using System.Collections.Generic;
using System.Linq;

public class ScoreCollection{
    public List<IScoreRule> scoreRules;
    public ScorableResult scorableResult;
    public int score;

    private int CalculateScore()
    {
        int score = 0;
        foreach(IScoreRule scoreRule in scoreRules)
        {
            score += scoreRule.GetScore(scorableResult);
        }
        return score;
    }

    public ScoreCollection(DiceCollection collection)
    {
        scorableResult = new ScorableResult(collection);
        score = CalculateScore();
    }

    public int GetScore()
    {
        return score;
    }
}

public class ScorableResult{
    public HashSet<int> set;
    public Dictionary<int, int> dict;

    public ScorableResult(DiceCollection diceCollection)
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
}

public interface IScoreRule{
    public int GetScore(ScorableResult scorableResult);
}

public class SingleOneScoreRule : IScoreRule{
    public int GetScore(ScorableResult scorableResult)
    {
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