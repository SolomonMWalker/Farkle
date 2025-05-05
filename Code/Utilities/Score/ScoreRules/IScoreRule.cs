using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public interface IScoreRule{
    public (int, HashSet<RootDice>) GetScore(ScorableCollection scorableResult);
}

public class SingleOneScoreRule : IScoreRule{
    public (int, HashSet<RootDice>) GetScore(ScorableCollection scorableResult)
    {
        if(scorableResult.fDict == null ||
            !scorableResult.fDict.ContainsKey(1))
        {
            return (-1, null);
        }

        var diceFaceScoredOne = scorableResult.faces
            .Where(f => f.number == 1);
        var diceFaceScoredOneCount = diceFaceScoredOne.Count();

        if(diceFaceScoredOneCount > 2)
        {
            return (-1, null);
        }
        if(diceFaceScoredOneCount == 1)
        {
            return (100, diceFaceScoredOne.Select(df => df.AssociatedDice).ToHashSet());
        }
        if (diceFaceScoredOneCount == 2)
        {
            return (200, diceFaceScoredOne.Select(df => df.AssociatedDice).ToHashSet());
        }
        
        return (-1, null);
    }
}

public class SingleFiveScoreRule : IScoreRule{
    public (int, HashSet<RootDice>) GetScore(ScorableCollection scorableResult)
    {
        if(scorableResult.fDict == null ||
            !scorableResult.fDict.ContainsKey(1))
        {
            return (-1, null);
        }

        var diceFaceScoredFive = scorableResult.faces
            .Where(f => f.number == 5);
        var diceFaceScoredFiveCount = diceFaceScoredFive.Count();

        if(diceFaceScoredFiveCount > 2)
        {
            return (-1, null);
        }
        if(diceFaceScoredFiveCount == 1)
        {
            return (50, diceFaceScoredFive.Select(df => df.AssociatedDice).ToHashSet());
        }
        if (diceFaceScoredFiveCount == 2)
        {
            return (100, diceFaceScoredFive.Select(df => df.AssociatedDice).ToHashSet());
        }
        
        return (-1, null);
    }
}

public class ThreeOrMoreOfAKindScoreRule : IScoreRule{
    public (int, HashSet<RootDice>) GetScore(ScorableCollection scorableResult)
    {
        if(scorableResult.fDict == null){return (-1, null);}

        var highestMultiple = scorableResult.fDict.Values.Max();

        if(highestMultiple < 3) {return (-1, null);}

        //if mult is 4 or up, only one per collection
        //else, there might be more, so get the highest scoring group first
        int diceNumber = highestMultiple > 3 ? 
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

        return (score, scorableResult.faces.Where(f => f.number == diceNumber).Select(f => f.AssociatedDice).ToHashSet());
    }
}