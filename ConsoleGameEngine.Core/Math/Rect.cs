using System;

namespace ConsoleGameEngine.Core.Math;

public struct Rect : IEquatable<Rect>
{
    public Vector Position { get; set; }
    public Vector Size { get; set; }
        
    public Vector Center => Position + Size * 0.5f;
    public float Width => Size.X;
    public float Height => Size.Y;

    public Vector TopLeft => Position;
    public Vector TopRight => Position with { X = Position.X + Width };
    public Vector BottomLeft => Position with { Y = Position.Y + Height };
    public Vector BottomRight => Position + Size;
    
    public float Left => Position.X;
    public float Top => Position.Y;
    public float Right => Left + Size.X;
    public float Bottom => Top + Size.Y;

    public Rect(Vector position, Vector size)
    {
        Position = position;
        Size = size;
    }

    public bool Contains(Vector point)
    {
        return point.X >= Position.X && point.X < Position.X + Size.X &&
               point.Y >= Position.Y && point.Y < Position.Y + Size.Y;
    }

    // TODO: Test this function
    public bool Overlaps(Rect other)
    {
        return Left < other.Right && Right > other.Left &&
               Top > other.Bottom && Bottom < other.Top;
    }
        
    public override string ToString()
    {
        return $"Position: {Position}, Size: {Size}";
    }

    public bool Equals(Rect other)
    {
        return Position.Equals(other.Position) && Size.Equals(other.Size);
    }

    public override bool Equals(object obj)
    {
        return obj is Rect other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position, Size);
    }
}