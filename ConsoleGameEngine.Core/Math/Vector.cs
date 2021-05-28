namespace ConsoleGameEngine.Core.Math
{
    public readonly struct Vector
    {
        public static Vector Zero { get; } = new(0, 0);
        
        public static Vector Left { get; } = new(-1, 0);
        public static Vector Right { get; } = new(1, 0);
        public static Vector Up { get; } = new(0, -1);
        public static Vector Down { get; } = new(0, 1);
        
        public float X { get; }
        public float Y { get; }

        public float Magnitude => (float)System.Math.Sqrt(X * X + Y * Y);
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

        public static Vector operator *(Vector v, float scalar)
        {
            return new(v.X * scalar, v.Y * scalar);
        }
        
        public static Vector operator /(Vector v, float scalar)
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
    }
}