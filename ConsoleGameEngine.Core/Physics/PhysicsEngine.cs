using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Physics;

public class PhysicsEngine(List<PhysicsObject> objects)
{
    /*TODO:
        * Collisions 
     */
    public List<PhysicsObject> Objects { get; } = objects;
    
    public float Gravity { get; set; } = 25f;
    public float TerminalVelocity { get; set; } = 55f;
    public float FrictionCoefficient { get; set; } = 0.2f;

    private readonly ParallelOptions _parallelOptions = new()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount
    };

    public PhysicsEngine() : this([]) { }

    public void Update(float elapsedTime)
    {
        Parallel.ForEach(Objects, _parallelOptions, obj => UpdatePhysicsObject(obj, elapsedTime));
    }

    private void UpdatePhysicsObject(PhysicsObject obj, float elapsedTime)
    {
        // Friction
        if (obj.Velocity.Magnitude > 0)
        {
            var friction = -obj.Velocity.Normalized.X * FrictionCoefficient;
            obj.ApplyForce(new Vector(friction, 0));
        }

        // Gravity
        obj.ApplyForce(Gravity * Vector.Down);

        // Update Velocity
        obj.Velocity += obj.Acceleration * elapsedTime;

        // Apply terminal velocity
        if (obj.Velocity.Y > TerminalVelocity) 
            obj.Velocity = new Vector(obj.Velocity.X, TerminalVelocity);

        if (obj.Velocity.Magnitude < 0.01f) 
            obj.Velocity = Vector.Zero;

        // Calculate position based on velocity
        obj.Position += obj.Velocity * elapsedTime;

        // Reset Acceleration
        obj.Acceleration = Vector.Zero;
    }
}