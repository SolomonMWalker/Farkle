using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class ScorableCollection{
    public ImmutableSortedSet<int> sSet;
    public Dictionary<int, int> dict;
    public readonly IEnumerable<DiceFace> faces;
    public readonly DiceCollection diceCollection;

    private ScorableCollection(DiceCollection collection)
    {
        diceCollection = collection;
        faces = diceCollection.GetResultOfRoll();
        sSet = [.. faces.Select(r => r.number)];
        var dict = new Dictionary<int, int>();
        foreach(DiceFace face in faces)
        {
            if(dict.Keys.Contains(face.number))
            {
                dict[face.number] += 1;
            }
            else
            {
                dict[face.number] = 1;
            }
        }
        this.dict = dict.ToDictionary();
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