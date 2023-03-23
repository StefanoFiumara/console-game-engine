namespace ConsoleGameEngine.Core.Math
{
    public struct Rect
    {
        public Vector Position { get; set; }
        public Vector Size { get; set; }
        public Vector Center => Position.Rounded + Size * 0.5f;

        public Rect(Vector position, Vector size)
        {
            Position = position;
            Size = size;
        }

        public override string ToString()
        {
            return $"Position: {Position}, Size: {Size}";
        }
    }
}
