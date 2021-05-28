namespace ConsoleGameEngine.Core.Math
{
    public readonly struct Rect
    {
        public Vector Position { get; }
        public Vector Size { get; }

        public Rect(Vector position, Vector size)
        {
            Position = position;
            Size = size;
        }
    }
}