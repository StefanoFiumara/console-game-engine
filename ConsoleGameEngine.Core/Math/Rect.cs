namespace ConsoleGameEngine.Core.Math
{
    public readonly struct Rect
    {
        public Vector Position { get; }
        public Vector Size { get; }
        public Vector Center => Position.Rounded + Size * 0.5f;

        public Rect(Vector position, Vector size)
        {
            Position = position;
            Size = size;
        }
    }
}