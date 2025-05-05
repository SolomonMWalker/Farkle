using System.Collections.Generic;

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