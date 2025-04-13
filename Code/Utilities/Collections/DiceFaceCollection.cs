using System.Collections.Generic;
using System.Linq;

public class DiceFaceCollection
{
    public List<DiceFaceNumber> faces;

    //Get the dice face that has the highest y-value
    //might need to change this later
    public int GetResultOfRoll()
    {
        var maxY =  faces.Max(x => x.Position.Y);
        return faces.First(x => x.Position.Y == maxY).number;
    }
}