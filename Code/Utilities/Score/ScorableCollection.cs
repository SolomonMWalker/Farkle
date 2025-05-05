using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

public class ScorableCollection{
    public FrozenSet<int> fSet;
    public FrozenDictionary<int, int> fDict;
    public readonly List<DiceFace> faces;

    private ScorableCollection(DiceCollection collection)
    {
        faces = collection.GetResultOfRoll();
        fSet = faces.Select(r => r.number).ToFrozenSet();
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
        fDict = dict.ToFrozenDictionary();
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