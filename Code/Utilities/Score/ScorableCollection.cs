using System.Collections.Generic;
using System.Linq;

public class ScorableCollection{
    public HashSet<int> set;
    public Dictionary<int, int> dict;
    public List<int> rollResults;

    private ScorableCollection(DiceCollection diceCollection)
    {
        rollResults = diceCollection.GetResultOfRoll();
        set = rollResults.ToHashSet();
        dict = new Dictionary<int, int>();
        foreach(int result in rollResults)
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

    public static ScorableCollection NewScorableCollection(DiceCollection diceCollection)
    {
        if(diceCollection == null || diceCollection.diceList.Count == 0)
        {
            return null;
        }
        else
        {
            return new ScorableCollection(diceCollection);
        }
    }
}