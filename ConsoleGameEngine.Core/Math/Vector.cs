using System;
// ReSharper disable MemberCanBePrivate.Global

namespace ConsoleGameEngine.Core.Math
{
    using static System.Math;
    
    public readonly struct Vector : IEquatable<Vector>
    {
        private const float TOLERANCE_EPSILON = 0.01f;
        
        public static Vector Zero { get; } = new(0, 0);
        
        public static Vector Left { get; } = new(-1, 0);
        public static Vector Right { get; } = new(1, 0);
        public static Vector Up { get; } = new(0, -1);
        public static Vector Down { get; } = new(0, 1);
        
        public float X { get; }
        public float Y { get; }

        public float Magnitude => (float) Sqrt(X * X + Y * Y);
        
        public Vector Normalized => Magnitude == 0f ? Zero : this / Magnitude;

        public Vector Rounded => new((int) X, (int) Y);
        
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

        public override string ToString()
        {
            return $"(X: {X}, Y: {Y})";
        }

        public static Vector operator *(Vector v, float scalar)
        {
            return new(v.X * scalar, v.Y * scalar);
        }
        
        public static Vector operator *(Vector v, int scalar)
        {
            return new(v.X * scalar, v.Y * scalar);
        }

        public static Vector operator *(float scalar, Vector v)
        {
            return new(v.X * scalar, v.Y * scalar);
        }

        public static Vector operator *(int scalar, Vector v)
        {
            return new(v.X * scalar, v.Y * scalar);
        }

        public static Vector operator /(Vector v, float scalar)
        {
            return new(v.X / scalar, v.Y / scalar);
        }
        
        public static Vector operator /(float scalar, Vector v)
        {
            return new(scalar / v.X, scalar / v.Y);
        }
        
        public static Vector operator /(Vector v, int scalar)
        {
            return new(v.X / scalar, v.Y / scalar);
        }

        public static Vector operator +(Vector l, Vector r)
        {
            return new(l.X + r.X, l.Y + r.Y);
        }
        
        public static Vector operator -(Vector l, Vector r)
        {
            return new(l.X - r.X, l.Y - r.Y);
        }

        public static bool operator ==(Vector l, Vector r)
        {
            return Abs(l.X - r.X) < TOLERANCE_EPSILON && 
                   Abs(l.Y - r.Y) < TOLERANCE_EPSILON;
        }

        public static bool operator !=(Vector l, Vector r)
        {
            return !(l == r);
        }
        
        public bool Equals(Vector other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}