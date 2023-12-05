using System.Collections.Generic;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Physics;

public class PhysicsEngine
{
    public float Gravity { get; set; } = 25f;
    public float TerminalVelocity { get; set; } = 55f;
    public float Friction { get; set; } = 15f;

    public List<PhysicsObject> Objects { get; }

    public PhysicsEngine(List<PhysicsObject> objects)
    {
        Objects = objects;
    }

    public void Update(float elapsedTime)
    {
        for (int i = 0; i < Objects.Count; i++)
        {
            var obj = Objects[i];
            
            // Gravity
            obj.Velocity += Vector.Down * Gravity * elapsedTime;
            
            // Clamp to terminal velocity
            if (obj.Velocity.Y > TerminalVelocity)
            {
                obj.Velocity = new Vector(obj.Velocity.X, TerminalVelocity);
            }
            
            // Apply friction along the X axis to slowly bring the object to a stop
            // TODO: Use clamping to clean up logic
            if (obj.Velocity.X < 0)
            {
                obj.Velocity += Vector.Right * Friction * elapsedTime;
                if (obj.Velocity.X > 0)
                {
                    obj.Velocity = new Vector(0, obj.Velocity.Y);
                }
            }
            else
            {
                obj.Velocity += Vector.Left * Friction * elapsedTime;
                if (obj.Velocity.X < 0)
                {
                    obj.Velocity = new Vector(0, obj.Velocity.Y);
                }
            }
            
            // Calculate position based on velocity
            obj.Position += obj.Velocity * elapsedTime;
            
            // TODO: Can we handle collisions here? Maybe check against a separate static collision shape?
        }
    }
}