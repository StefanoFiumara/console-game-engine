using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects;

public class PhysicsObject(Sprite sprite, Vector position) : GameObject(sprite, position)
{
    public Vector Velocity { get; set; }
    public Vector Acceleration { get; set; }

    // IDEA: Calculate Mass based on sprite size?
    public float Mass { get; set; } = 0.1f;

    public void ApplyForce(Vector force)
    {
        Acceleration += force / Mass;
    }
}