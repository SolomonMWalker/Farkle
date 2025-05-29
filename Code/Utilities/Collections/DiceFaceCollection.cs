using System.Collections.Generic;
using System.Linq;
using Godot;

public class DiceFaceCollection
{
    public IEnumerable<RootDiceFace> faces = [];
    public DiceFaceType diceFaceType;

    public DiceFaceCollection(IEnumerable<DiceFacePlaceholder> dfPlaceholders, DiceFaceType dfType)
    {
        diceFaceType = dfType;
        faces = DiceFaceFactory.CreateDiceFaces(dfPlaceholders, dfType);
    }

    public void ReparentDiceFaces(Node3D parent)
    {
        foreach (var face in faces)
        {
            face.Reparent(face);
        }
    }

    //Get the dice face that has the highest y-value
    //might need to change this later
    public RootDiceFace GetResultOfRoll()
    {
        RootDiceFace faceOnTop = null;
        foreach (RootDiceFace face in faces)
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