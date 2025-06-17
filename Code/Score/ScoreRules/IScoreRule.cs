using System.Collections.Generic;
using System.Linq;

public interface IScoreRule{
    public ScoreWithUnusedDice GetScore(ScorableCollection scorableCollection);
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

    public ScoreWithUnusedDice GetScore(ScorableCollection scorableCollection)
    {
        if(scorableCollection.dict == null || !scorableCollection.dict.ContainsKey(Num)) {return new (-1, []);}

        var diceFaceScored = scorableCollection.faces
            .Where(f => f.Number == Num);
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
    public ScoreWithUnusedDice GetScore(ScorableCollection scorableCollection)
    {
        if (scorableCollection.dict == null) { return new(-1, null); }

        var highestMultiple = scorableCollection.dict.Values.Max();

        if (highestMultiple < 3) { return new(-1, null); }

        int diceNumber = scorableCollection.dict
            .Where(x => x.Value == highestMultiple)
            .Max(x => x.Key);            

        //score = score of threeOfAKind + (1000 for each extra past three)
        //comes out to score with 3 + (1000 * (number of dice over three that exists))
        int score = diceNumber is 1 ? 1000 + 1000 * (highestMultiple - 3) : diceNumber * 100 + 1000 * (highestMultiple - 3);

        return new(score,
            [.. scorableCollection.faces.Where(f => f.Number != diceNumber).Select(f => f.AssociatedDice)]);
    }
}

public class StraightScoreRule : IScoreRule
{
    public ScoreWithUnusedDice GetScore(ScorableCollection scorableCollection)
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
                lastNumberInSet = scorableCollection.sSet[i];
            }
            else
            {
                if (straightLength > longestStraightStartAndLength.length)
                {
                    longestStraightStartAndLength = (straightStart, straightLength);
                }
                straightStart = scorableCollection.sSet[i];
                straightLength = 1;
                lastNumberInSet = straightStart;
            }
        }

        //Check in case last in set was part of straight
        if (straightLength > longestStraightStartAndLength.length)
        {
            longestStraightStartAndLength = (straightStart, straightLength);
        }

        if (longestStraightStartAndLength.length < 5)
        {
            return new(-1, []);
        }

        List<int> numbersInStraight = [];
        for (int i = longestStraightStartAndLength.start; i < longestStraightStartAndLength.start + longestStraightStartAndLength.length; i++)
        {
            numbersInStraight.Add(i);
        }

        var score = longestStraightStartAndLength.length > 5 ? 2000 : 1500;
        List<RootDice> usedDice = [];
        foreach (int number in numbersInStraight)
        {
            usedDice.Add(scorableCollection.faces.First(f => f.Number == number).AssociatedDice);
        }
        var unusedDice = scorableCollection.diceCollection.RemoveDice(usedDice).diceList;
        return new(score, [.. unusedDice]);
    }
}

public class ThreePairScoreRule : IScoreRule
{
    public ScoreWithUnusedDice GetScore(ScorableCollection scorableCollection)
    {
        var pairs = scorableCollection.dict.Where(d => d.Value == 2).ToDictionary();

        if (pairs.Keys.Count < 3)
        {
            return new(-1, []);
        }


        List<DiceFace> usedDiceFaces = [];
        foreach (KeyValuePair<int, int> pair in pairs)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                usedDiceFaces.Add(scorableCollection.faces.First(f => f.Number == pair.Key &&
                    !usedDiceFaces.Contains(f)));
            }
        }

        var unusedDice = scorableCollection.diceCollection.RemoveDice(
            usedDiceFaces.Select(f => f.AssociatedDice)).diceList;
        return new(1500, [.. unusedDice]);
    }
}