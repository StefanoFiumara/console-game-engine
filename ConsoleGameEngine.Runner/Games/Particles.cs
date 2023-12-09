using System;
using System.Linq;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Physics;

namespace ConsoleGameEngine.Runner.Games;

public class Particles : ConsoleGameEngineBase
{
    private Vector _spiralPosition;
    private ParticleSystem _spiralParticles;

    private Vector _fountainPosition;
    private ParticleSystem _fountainParticles;

    private Random _rng;
    
    public Particles()
    {
        InitConsole(160, 120);
        PerformanceModeEnabled = true;
    }
    
    protected override bool Create()
    {
        _spiralPosition = ScreenRect.Center + Vector.Down * 5 + Vector.Left * 30;
        _fountainPosition = ScreenRect.Center + Vector.Down * 5 + Vector.Right * 30;
        _rng = new Random();
        
        var spiralSprites = Enum.GetValues<ConsoleColor>()
            .Select(c => new Sprite("*", c))
            .ToArray();

        var fountainSprites = new[]
        {
            new Sprite("*", ConsoleColor.Blue),
            new Sprite("*", ConsoleColor.DarkBlue),
            new Sprite("*", ConsoleColor.Cyan),
            new Sprite("*", ConsoleColor.DarkCyan),
            new Sprite("*", ConsoleColor.White),
        };

        _spiralParticles = new ParticleSystem(
            _spiralPosition,
            spiralSprites,
            initParticleAction: p => p.Velocity = GenerateSpiralForce(), 
            spawnInterval: 0.02f, 
            new()
            {
                Friction = 0, 
                Gravity = 0, 
                Lifetime = 1.5f
            });
        
        _fountainParticles = new ParticleSystem(
            _fountainPosition,
            fountainSprites,
            initParticleAction: p => p.Velocity = GenerateFountainForce(), 
            spawnInterval: 0.02f, 
            new()
            {
                Friction = 0.2f, 
                Gravity = 25f, 
                Lifetime = 1.5f
            });

        return true;
    }

    private int _spiralAngle = 1;

    private Vector GenerateSpiralForce()
    {
        var angle = _spiralAngle;
        _spiralAngle += 10;
        if (_spiralAngle >= 360) _spiralAngle = 0;
        var radians = angle * (Math.PI / 180);
        
        var x = Math.Cos(radians);
        var y = -Math.Sin(radians);

        return new Vector(x, y).Normalized * 20;
    }
    
    private Vector GenerateFountainForce()
    {
        var angle = _rng.Next(80, 100);
        var radians = angle * (Math.PI / 180);
        
        var x = Math.Cos(radians);
        var y = -Math.Sin(radians);

        return new Vector(x, y).Normalized * _rng.Next(120, 150);
    }
    
    protected override bool Update(float elapsedTime, PlayerInput input)
    {
        if(input.IsKeyHeld(KeyCode.Esc)) 
        {
            return false;
        }
        Fill(ScreenRect, ' ');
        
        _spiralParticles.Update(elapsedTime);
        _fountainParticles.Update(elapsedTime);

        foreach (var particle in _spiralParticles.ActiveParticles)
        {
            DrawObject(particle); 
        }
        
        foreach (var particle in _fountainParticles.ActiveParticles)
        {
            DrawObject(particle); 
        }
        
        return true;
    }
}