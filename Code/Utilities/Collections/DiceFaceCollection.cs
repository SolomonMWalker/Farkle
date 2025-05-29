using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class DiceFaceCollection
{
    public List<DiceFace> faces = [];

    public void SetDebug(bool isDebug)
    {
        faces.ForEach(f => f.SetDebug(isDebug));
    }

    //Get the dice face that has the highest y-value
    //might need to change this later
    public DiceFace GetResultOfRoll()
    {
        DiceFace faceOnTop = null;
        foreach (DiceFace face in faces)
        {
            if (faceOnTop == null)
            {
                faceOnTop = face;
            }
            else if (face.GlobalPosition.Y > faceOnTop.GlobalPosition.Y)
            {
                faceOnTop = face;
            }
        }
        return faceOnTop;
    }

    public float GetHeightOfLowestFace()
    {
        return faces.Min(x => x.Position.Y);
    }

    public List<ulong> GetDiceFaceInstanceIds() => [.. faces.Select(f => f.GetInstanceId())];
}