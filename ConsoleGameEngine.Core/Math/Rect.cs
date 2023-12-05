namespace ConsoleGameEngine.Core.Math;

public struct Rect
{
    public Vector Position { get; set; }
    public Vector Size { get; set; }
        
    public Vector Center => Position + Size * 0.5f;
    public float Width => Size.X;
    public float Height => Size.Y;

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
}