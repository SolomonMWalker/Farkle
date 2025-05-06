using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

public interface IScoreRule{
    public CalculatedScoreResult GetScore(ScorableCollection scorableResult);
}

public class SingleNumScoreRule(int num, int scoreOfOneNum) : IScoreRule
{
    public int Num { get; } = num;
    public int ScoreOfOneNum { get; } = scoreOfOneNum;

    public static IEnumerable<SingleNumScoreRule> GenerateSingleNumScoreRules(IEnumerable<(int num, int score)> numsAndScores)
    {
        SingleNumScoreRule[] scoreRules = new SingleNumScoreRule[numsAndScores.Count()];

        (int num, int score)[] numsAndScoresDistinct = [.. numsAndScores.Distinct()];
        for(int i = 0; i < numsAndScores.Count(); i++)
        {
            scoreRules[i] = new SingleNumScoreRule(numsAndScoresDistinct[i].num, numsAndScoresDistinct[i].score);
        }
        
        return scoreRules;
    }

    public CalculatedScoreResult GetScore(ScorableCollection scorableResult)
    {
        if(scorableResult.fDict == null || !scorableResult.fDict.ContainsKey(Num)) {return new (-1, []);}

        var diceFaceScored = scorableResult.faces
            .Where(f => f.number == Num);
        var diceFaceScoredCount = diceFaceScored.Count();

        if(diceFaceScoredCount > 2)
        {
            return new (-1, []);
        }
        if(diceFaceScoredCount == 1)
        {
            return new (ScoreOfOneNum, 
                scorableResult.diceCollection.RemoveDice(diceFaceScored.Select(df => df.AssociatedDice)).diceList);
        }
        if (diceFaceScoredCount == 2)
        {
            return new (ScoreOfOneNum * 2, 
                scorableResult.diceCollection.RemoveDice(diceFaceScored.Select(df => df.AssociatedDice)).diceList);
        }
        
        return new (-1, []);
    }
}

public class ThreeOrMoreOfAKindScoreRule : IScoreRule{
    public CalculatedScoreResult GetScore(ScorableCollection scorableResult)
    {
        if(scorableResult.fDict == null){return new (-1, null);}

        var highestMultiple = scorableResult.fDict.Values.Max();

        if(highestMultiple < 3) {return new (-1, null);}

        //if mult is 4 or up, only one per collection
        //else, there might be more, so get the highest scoring group first
        int diceNumber = scorableResult.faces.Count() - highestMultiple >= highestMultiple ? 
            scorableResult.fDict
                .Where(x => x.Value == highestMultiple)
                .Select(x => x.Key)
                .Single() :
            scorableResult.fDict
                .Where(x => x.Value == 3)
                .Max(x => x.Key);

        //score = score of threeOfAKind + (1000 for each extra past three)
        //comes out to score with 3 + (1000 * (number of dice over three that exists))
        int score = diceNumber is 1 ? 1000 + 1000*(highestMultiple - 3) : diceNumber*100 + 1000*(highestMultiple - 3);

        return new (score, 
            scorableResult.faces.Where(f => f.number != diceNumber).Select(f => f.AssociatedDice).ToList());
    }
}