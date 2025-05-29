using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;

public enum DiceFaceType
{
    Root,
    Test
}

public class DiceFaceFactory
{
    public static List<RootDiceFace> CreateDiceFaces(
        IEnumerable<(Node3D node, DiceFaceValue dfValue)> nodesAndValues,
        DiceFaceType dfType)
    {
        List<RootDiceFace> rdf = [];
        foreach ((Node3D n, DiceFaceValue value) tdfv in nodesAndValues)
        {
            var face = dfType switch
            {
                DiceFaceType.Root => new RootDiceFace(tdfv.value, tdfv.n.Transform),
                DiceFaceType.Test => new TestDiceFace(tdfv.value, tdfv.n.Transform),
                _ => new RootDiceFace(tdfv.value, tdfv.n.Transform)
            };
            rdf.Add(face);
        }
        return rdf;
    }

    public static List<RootDiceFace> CreateDiceFaces(IEnumerable<DiceFacePlaceholder> dfPs,
        DiceFaceType type)
    {
        return CreateDiceFaces(
            dfPs.Select(x => ( (Node3D) x, new DiceFaceValue(x.diceFaceNumber))),
            type
        );
    }
}