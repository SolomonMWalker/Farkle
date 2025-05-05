using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public interface IScoreRule{
    public (int, List<int>) GetScore(ScorableCollection scorableResult);
}

public class SingleOneScoreRule : IScoreRule{
    public (int, List<int>) GetScore(ScorableCollection scorableResult)
    {
        if(scorableResult.dict == null ||
            !scorableResult.dict.ContainsKey(1))
        {
            return (-1, null);
        }

        var numOfOnes = scorableResult.dict[1];

        if(numOfOnes > 2)
        {
            return (-1, null);
        }
        if(numOfOnes == 1)
        {
            return (100, new(){1});
        }
        if (numOfOnes == 2)
        {
            return (200, new(){1,1});
        }
        
        return (-1, null);
    }
}

public class SingleFiveScoreRule : IScoreRule{
    public (int, List<int>) GetScore(ScorableCollection scorableResult)
    {
        if(scorableResult.dict == null ||
            !scorableResult.dict.ContainsKey(5))
        {
            return (-1, null);
        }

        var numOfFives = scorableResult.dict[5];

        if(numOfFives > 2)
        {
            return (-1, null);
        }
        if(numOfFives == 1)
        {
            return (50, new(){5});
        }
        if (numOfFives == 2)
        {
            return (100, new(){5,5});
        }
        
        return (-1, null);
    }
}

public class ThreeOrMoreOfAKindScoreRule : IScoreRule{
    public (int, List<int>) GetScore(ScorableCollection scorableResult)
    {
        if(scorableResult.dict == null){return (-1, null);}

        var highestMultiple  = scorableResult.dict.Values.Max();

        if(highestMultiple < 3) {return (-1, null);}

        //if mult is 4 or up, only one per collection
        //else, there might be more, so get the highest scoring group first
        int diceNumber = highestMultiple > 3 ? 
            scorableResult.dict
                .Where(x => x.Value == highestMultiple)
                .Select(x => x.Key)
                .Single() :
            scorableResult.dict
                .Where(x => x.Value == 3)
                .Max(x => x.Key);

        //score = score of threeOfAKind + (1000 for each extra past three)
        //comes out to score with 3 + (1000 * (number of dice over three that exists))
        int score = diceNumber is 1 ? 1000 + 1000*(highestMultiple - 3) : diceNumber*100 + 1000*(highestMultiple - 3);
        
        List<int> diceNumbers = new List<int>();
        for(int i = 0; i < highestMultiple; i++)
        {
            diceNumbers.Add(diceNumber);
        }

        return (score, diceNumbers);
    }
}