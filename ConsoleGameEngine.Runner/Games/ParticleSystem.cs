using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Physics;

namespace ConsoleGameEngine.Runner.Games;

public class ParticleSystem : ConsoleGameEngineBase
{
    private Vector _fountainPosition;

    private ObjectPool<PhysicsObject> _pool;
    private readonly Random _rng;
    
    private const float GameTick = 0.03f;
    private float _gameTimer;

    private PhysicsEngine _physicsEngine;

    private int _colorIndex = 0;
    private readonly ConsoleColor[] _colors =
    {
        ConsoleColor.Blue, 
        ConsoleColor.DarkBlue, 
         ConsoleColor.Cyan,
         ConsoleColor.DarkCyan,
        ConsoleColor.White ,
        ConsoleColor.Yellow
        
    };

    public ParticleSystem()
    {
        InitConsole(160, 120);
        PerformanceModeEnabled = true;
        _rng = new Random();
    }
    
    protected override bool Create()
    {
        _fountainPosition = ScreenRect.Center + 7 * Vector.Down;
        _pool = new ObjectPool<PhysicsObject>(CreateParticle);
        _physicsEngine = new PhysicsEngine(new List<PhysicsObject>());
        _gameTimer = GameTick;
        
        return true;
    }

    private PhysicsObject CreateParticle()
    {
        var color = _colors[_colorIndex++ % _colors.Length];
        return new PhysicsObject(new Sprite("*", color), _fountainPosition);
    }
    
    public Vector GenerateParticleForce()
    {
        var angleDegrees = _rng.Next(80, 100);
        var angleRadians = angleDegrees * (Math.PI / 180);
        
        var x = Math.Cos(angleRadians);
        var y = -Math.Sin(angleRadians);

        return new Vector(x, y).Normalized * _rng.Next(85, 125);
    }
    
    protected override bool Update(float elapsedTime, PlayerInput input)
    {
        if(input.IsKeyHeld(KeyCode.Esc)) 
        {
            return false;
        }
        Fill(ScreenRect, ' ');
        
        _gameTimer -= elapsedTime;
        if (_gameTimer <= 0f)
        {
            // Spawn a particle every GameTick.
            _gameTimer = GameTick;
            
            var particle = _pool.Get();
            
            particle.Position = _fountainPosition;
            particle.Velocity = GenerateParticleForce();
            
            _physicsEngine.Objects.Add(particle);
        }

        _physicsEngine.Update(elapsedTime);
        for (var i = _physicsEngine.Objects.Count - 1; i >= 0; i--)
        {
            var particle = _physicsEngine.Objects[i];
            if (particle.Position.Y > _fountainPosition.Y)
            {
                _pool.Return(particle);
                _physicsEngine.Objects.Remove(particle);
            }
            else
            {
                DrawObject(particle);
            }
        }

        return true;
    }
}