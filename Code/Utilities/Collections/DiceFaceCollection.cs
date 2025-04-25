using System.Collections.Generic;
using System.Linq;

public class DiceFaceCollection
{
    public List<DiceFace> faces;

    //Get the dice face that has the highest y-value
    //might need to change this later
    public DiceFace GetResultOfRoll()
    {
        var maxY =  faces.Max(x => x.Position.Y);
        return faces.First(x => x.Position.Y == maxY);
    }

    public float GetHeightOfLowestFace()
    {
        return faces.Min(x => x.Position.Y);
    }
}