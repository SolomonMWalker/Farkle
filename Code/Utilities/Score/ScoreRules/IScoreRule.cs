using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

public interface IScoreRule{
    public CalculatedScoreResult GetScore(ScorableCollection scorableCollection);
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

    public CalculatedScoreResult GetScore(ScorableCollection scorableCollection)
    {
        if(scorableCollection.dict == null || !scorableCollection.dict.ContainsKey(Num)) {return new (-1, []);}

        var diceFaceScored = scorableCollection.faces
            .Where(f => f.number == Num);
        var diceFaceScoredCount = diceFaceScored.Count();

        if(diceFaceScoredCount > 2)
        {
            return new (-1, []);
        }
        if(diceFaceScoredCount == 1)
        {
            return new (ScoreOfOneNum, 
                scorableCollection.diceCollection.RemoveDice(diceFaceScored.Select(df => df.AssociatedDice)).diceList);
        }
        if (diceFaceScoredCount == 2)
        {
            return new (ScoreOfOneNum * 2, 
                scorableCollection.diceCollection.RemoveDice(diceFaceScored.Select(df => df.AssociatedDice)).diceList);
        }
        
        return new (-1, []);
    }
}

public class ThreeOrMoreOfAKindScoreRule : IScoreRule
{
    public CalculatedScoreResult GetScore(ScorableCollection scorableCollection)
    {
        if (scorableCollection.dict == null) { return new(-1, null); }

        var highestMultiple = scorableCollection.dict.Values.Max();

        if (highestMultiple < 3) { return new(-1, null); }

        //if mult is 4 or up, only one per collection
        //else, there might be more, so get the highest scoring group first
        int diceNumber = scorableCollection.faces.Count() - highestMultiple >= highestMultiple ?
            scorableCollection.dict
                .Where(x => x.Value == highestMultiple)
                .Select(x => x.Key)
                .Single() :
            scorableCollection.dict
                .Where(x => x.Value == 3)
                .Max(x => x.Key);

        //score = score of threeOfAKind + (1000 for each extra past three)
        //comes out to score with 3 + (1000 * (number of dice over three that exists))
        int score = diceNumber is 1 ? 1000 + 1000 * (highestMultiple - 3) : diceNumber * 100 + 1000 * (highestMultiple - 3);

        return new(score,
            [.. scorableCollection.faces.Where(f => f.number != diceNumber).Select(f => f.AssociatedDice)]);
    }
}

public class StraightScoreRule : IScoreRule
{
    public CalculatedScoreResult GetScore(ScorableCollection scorableCollection)
    {
        (int start, int length) longestStraightStartAndLength = (0, 0);

        int straightStart = 0;
        int straightLength = 0;
        int lastNumberInSet = 0;
        for (int i = 0; i < scorableCollection.sSet.Count; i++)
        {
            if (straightStart == 0)
            {
                straightStart = scorableCollection.sSet[i];
                straightLength = 1;
                lastNumberInSet = straightStart;
            }
            else if (scorableCollection.sSet[i] == lastNumberInSet + 1)
            {
                straightLength++;
            }
            else
            {
                if (longestStraightStartAndLength.length > straightLength)
                {
                    longestStraightStartAndLength = (straightStart, straightLength);
                    straightStart = scorableCollection.sSet[i];
                    straightLength = 1;
                    lastNumberInSet = straightStart;
                }
            }
        }

        if (longestStraightStartAndLength.length < 5)
        {
            return new(-1, []);
        }

        var score = longestStraightStartAndLength.length > 5 ? 2000 : 1500;
        var unusedDice = scorableCollection.faces.Where(f => f.number < longestStraightStartAndLength.start &&
            f.number >= longestStraightStartAndLength.start + longestStraightStartAndLength.length)
            .Select(f => f.AssociatedDice);
        return new(score, [.. unusedDice]);
    }
}