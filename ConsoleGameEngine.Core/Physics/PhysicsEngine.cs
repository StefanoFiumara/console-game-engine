using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Physics;

public class PhysicsEngine(List<PhysicsEntity> entities)
{
    public PhysicsEngine() : this([]) { }
    
    /* TODO:
        * Collisions 
     */
    public List<PhysicsEntity> Entities { get; } = entities;
    
    public float Gravity { get; set; } = 25f;
    public float TerminalVelocity { get; set; } = 55f;
    public float FrictionCoefficient { get; set; } = 0.2f;

    private readonly ParallelOptions _parallelOptions = new()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount
    };

    public void Update(float elapsedTime)
    {
        Parallel.ForEach(Entities, _parallelOptions, entity => UpdatePhysicsEntity(entity, elapsedTime));
    }

    private void UpdatePhysicsEntity(PhysicsEntity entity, float elapsedTime)
    {
        // Friction
        if (entity.Velocity.Magnitude > 0)
        {
            var friction = -entity.Velocity.Normalized.X * FrictionCoefficient;
            entity.ApplyForce(new Vector(friction, 0));
        }

        // Gravity
        entity.ApplyForce(Gravity * Vector.Down);

        // Update Velocity
        entity.Velocity += entity.Acceleration * elapsedTime;

        // Apply terminal velocity
        if (entity.Velocity.Y > TerminalVelocity) 
            entity.Velocity = new Vector(entity.Velocity.X, TerminalVelocity);

        // Snap small velocities to zero
        if (entity.Velocity.Magnitude < 0.01f) 
            entity.Velocity = Vector.Zero;

        // Calculate position based on velocity
        entity.Position += entity.Velocity * elapsedTime;

        // Reset Acceleration
        entity.Acceleration = Vector.Zero;
    }
}