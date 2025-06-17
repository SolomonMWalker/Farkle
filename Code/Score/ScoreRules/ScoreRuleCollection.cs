using System.Collections.Generic;
using System.Collections.Immutable;

public class ScoreRuleCollection
{
    public ImmutableList<IScoreRule> scoreRuleList;

    public ScoreRuleCollection()
    {
        scoreRuleList = [];
    }

    public ScoreRuleCollection(IEnumerable<IScoreRule> rules)
    {
        scoreRuleList = [.. rules];
    }

    public static ScoreRuleCollection GetDefaultRules()
    {
        List<(int num, int score)> singleNumScores = [(1, 100), (5, 50)];

        List<IScoreRule> ruleList = [];
        foreach(SingleNumScoreRule rule in SingleNumScoreRule.GenerateSingleNumScoreRules(singleNumScores))
        {
            ruleList.Add(rule);
        }
        ruleList.Add(new ThreeOrMoreOfAKindScoreRule());
        ruleList.Add(new StraightScoreRule());
        ruleList.Add(new ThreePairScoreRule());

        return new ScoreRuleCollection(ruleList);
    }
}