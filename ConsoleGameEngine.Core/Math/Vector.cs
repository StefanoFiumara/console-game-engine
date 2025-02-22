using System;

namespace ConsoleGameEngine.Core.Math;

using static System.Math;
    
public struct Vector : IEquatable<Vector>
{
    private const float ToleranceEpsilon = 0.01f;
        
    public static Vector Zero { get; } = new(0, 0);
        
    public static Vector Left { get; } = new(-1, 0);
    public static Vector Right { get; } = new(1, 0);
    public static Vector Up { get; } = new(0, -1);
    public static Vector Down { get; } = new(0, 1);
        
    public float X { get; set; }
    public float Y { get; set; }

    public float Magnitude => (float) Sqrt(X * X + Y * Y);
    
    public Vector Normalized => Magnitude == 0f ? Zero : this / Magnitude;
    public Vector Rounded => new((int) X, (int) Y);

    public Vector(double x, double y)
    {
        X = (float) x;
        Y = (float) y;
    }

    public Vector(float x, float y)
    {
        X = x;
        Y = y;
    }
        
    public Vector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => $"(X: {X}, Y: {Y})";

    public static Vector operator *(Vector v, float scalar) => new(v.X * scalar, v.Y * scalar);
    public static Vector operator *(Vector v, int scalar) => new(v.X * scalar, v.Y * scalar);
    public static Vector operator *(float scalar, Vector v) => new(v.X * scalar, v.Y * scalar);
    public static Vector operator *(int scalar, Vector v) => new(v.X * scalar, v.Y * scalar);
    public static Vector operator /(Vector v, float scalar) => new(v.X / scalar, v.Y / scalar);
    public static Vector operator /(float scalar, Vector v) => new(scalar / v.X, scalar / v.Y);
    public static Vector operator /(Vector v, int scalar) => new(v.X / scalar, v.Y / scalar);
    public static Vector operator +(Vector l, Vector r) => new(l.X + r.X, l.Y + r.Y);
    public static Vector operator -(Vector l, Vector r) => new(l.X - r.X, l.Y - r.Y);

    public static bool operator ==(Vector l, Vector r) =>
        Abs(l.X - r.X) < ToleranceEpsilon && 
        Abs(l.Y - r.Y) < ToleranceEpsilon;

    public static bool operator !=(Vector l, Vector r) => !(l == r);
    public static Vector operator -(Vector v) => new(-v.X, -v.Y);

    public bool Equals(Vector other) => this == other;
    public override bool Equals(object? obj) => obj is Vector other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}