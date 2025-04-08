using System;
using Godot;

public static class HelperMethods {
    public static Vector3 GetRandomPointInSphere(float radius, Vector3 origin)
    {
        return new Vector3(
            RandomSign() * GD.Randf() * radius + origin.X, 
            RandomSign() * GD.Randf() * radius + origin.Y, 
            RandomSign() * GD.Randf() * radius + origin.Z
        );
    }

    public static int RandomSign() => GD.Randf() > 0.5f ? 1 : -1;

    public static float CenterOfCubeToCorner(float sideLength) => Mathf.Sqrt(sideLength);

    public static Vector3 FuzzyUpVector3(Vector3 vector, float coefficient)
    {
        //Add random values vector axes from -1 to 1
        vector.X += RandomSign()*GD.Randf()*coefficient;        
        vector.Y += RandomSign()*GD.Randf()*coefficient;        
        vector.Z += RandomSign()*GD.Randf()*coefficient;

        return vector;
    }
}