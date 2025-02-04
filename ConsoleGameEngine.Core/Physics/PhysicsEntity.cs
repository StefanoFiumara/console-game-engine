using ConsoleGameEngine.Core.Entities;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Physics;

public class PhysicsEntity(Sprite sprite, Vector position) : GameEntity(sprite, position)
{
    public Vector Velocity { get; set; }
    public Vector Acceleration { get; set; }

    // NOTE: Mass is current unused
    public float Mass { get; set; } = 0.1f;

    public void ApplyForce(Vector force)
    {
        Acceleration += force / Mass;
    }
}