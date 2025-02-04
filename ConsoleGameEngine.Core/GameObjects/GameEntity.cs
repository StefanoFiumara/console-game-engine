using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects;

public class GameEntity(Sprite sprite, Vector position = default) : GameObject(position)
{
    public Sprite Sprite { get; set; } = sprite;
    public override Rect Bounds => new(Position, Sprite.Size);

    public override void Draw(IRenderer renderer)
    {
        renderer.DrawSprite(Sprite, Position);
    }
}