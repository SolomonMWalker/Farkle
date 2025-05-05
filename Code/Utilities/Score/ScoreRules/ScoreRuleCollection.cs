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
        scoreRuleList = rules.ToImmutableList();
    }

    public static ScoreRuleCollection GetDefaultRules()
    {
        return new ScoreRuleCollection([
            new SingleOneScoreRule(),
            new SingleFiveScoreRule(),
            new ThreeOrMoreOfAKindScoreRule()
        ]);
    }
}