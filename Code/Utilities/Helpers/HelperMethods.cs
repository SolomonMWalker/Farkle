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

    // get side length of cube from half its diagonal
    // we have origin of a cube to a corner which is half its diagonal
    // with this we can get the side length
    public static float GetSideLengthFromHalfDiagonal(float halfDiagonal) => halfDiagonal / Mathf.Pow(3, 1/3);

    public static Vector3 GetRandomVector3(float coefficient = 1f)
    {
        return new Vector3(GD.Randf()*coefficient, GD.Randf()*coefficient, GD.Randf()*coefficient);
    }
}