using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Physics;

public class ParticleOptions
{
    // TODO: Defaults?
    public float Lifetime { get; init; }
    public float Gravity { get; init; }
    public float Friction { get; init; }
}

public class ParticleSystem
{
    private readonly Vector _position;
    private readonly PhysicsEngine _engine;
    private readonly Sprite[] _particles;
    private readonly Action<PhysicsObject> _initParticleAction;
    private readonly float _spawnInterval;
    private readonly float _particleLifetime;
    private readonly ObjectPool<PhysicsObject> _pool;
    
    private float _spawnTimer;
    private readonly Dictionary<PhysicsObject, float> _particleTimers;

    public IReadOnlyList<PhysicsObject> ActiveParticles => _engine.Objects;

    public ParticleSystem(
        Vector position,
        Sprite[] particles, 
        Action<PhysicsObject> initParticleAction, 
        float spawnInterval,
        ParticleOptions options)
    {
        _position = position;
        _particles = particles;
        _initParticleAction = initParticleAction;
        _spawnInterval = spawnInterval;
        
        _particleLifetime = options.Lifetime;
        _engine = new PhysicsEngine
        {
            Gravity = options.Gravity,
            FrictionCoefficient = options.Friction
        };

        _particleTimers = new Dictionary<PhysicsObject, float>();
        _spawnTimer = _spawnInterval;
        _pool = new ObjectPool<PhysicsObject>(CreateParticle);
    }

    private int _spriteIndex = 0;

    private PhysicsObject CreateParticle()
    {
        var sprite = _particles[_spriteIndex++ % _particles.Length];
        var particle = new PhysicsObject(sprite, _position);
        return particle;
    }

    public void Update(float elapsedTime)
    {
        _spawnTimer -= elapsedTime;
        if (_spawnTimer <= 0)
        {
            _spawnTimer = _spawnInterval;
            var particle = _pool.Get();
            particle.Position = _position;
            _initParticleAction(particle);
            _engine.Objects.Add(particle);
            _particleTimers.Add(particle, _particleLifetime);
        }
        
        _engine.Update(elapsedTime);
        for (var i = _engine.Objects.Count - 1; i >= 0; i--)
        {
            var particle = _engine.Objects[i];
            _particleTimers[particle] -= elapsedTime;
            
            if (_particleTimers[particle] <= 0)
            {
                _pool.Return(particle);
                _engine.Objects.Remove(particle);
                _particleTimers.Remove(particle);
            }
        }
    }
}