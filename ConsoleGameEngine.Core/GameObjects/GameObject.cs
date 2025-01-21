using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects;

public class GameObject
{
    public Vector Position { get; set; }
    public Sprite Sprite { get; set; }
        
    public Rect Bounds => new(Position, Sprite.Size);

    public GameObject(Sprite sprite, Vector position = default)
    {
        Sprite = new Sprite(sprite);
        Position = position;
    }
}