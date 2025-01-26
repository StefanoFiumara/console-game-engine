using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects;

public class GameObject(Sprite sprite, Vector position = default)
{
    public Vector Position { get; set; } = position;
    
    // TODO: Can we get away with not doing a sprite copy here?
    public Sprite Sprite { get; set; } = new(sprite);

    public Rect Bounds => new(Position, Sprite.Size);
}