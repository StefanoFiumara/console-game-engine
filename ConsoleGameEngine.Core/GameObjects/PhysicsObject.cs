using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects;

public class PhysicsObject : GameObject
{
    public Vector Velocity { get; set; }
    // TODO: Acceleration + Physics Update here?
        
    public PhysicsObject(Sprite sprite, Vector position) : base(sprite, position)
    {
    }
}