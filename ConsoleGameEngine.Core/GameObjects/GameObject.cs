using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects;

public abstract class GameObject(Vector position = default)
{
    public Vector Position { get; set; } = position;
    public abstract Rect Bounds { get; }
    public abstract void Draw(IRenderer renderer);
}