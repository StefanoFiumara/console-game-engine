using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects;

public class PhysicsObject : GameObject
{
    public Vector Velocity { get; set; }
    
    public Vector Acceleration { get; set; }

    public float Mass { get; set; } = 0.1f;
        
    public PhysicsObject(Sprite sprite, Vector position) : base(sprite, position)
    {
        // IDEA: Calculate Mass based on sprite size?
    }

    public void ApplyForce(Vector force)
    {
        Acceleration += force / Mass;
    }
}