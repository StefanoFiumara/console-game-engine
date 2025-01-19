using System;
using System.Linq;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Physics;

namespace ConsoleGameEngine.Runner.Games;

// ReSharper disable once UnusedType.Global
public class Particles() : ConsoleGame(new ConsoleRenderer(width: 160, height: 120))
{
    private Vector _spiralPosition;
    private ParticleSystem _spiralParticles;

    private Vector _fountainPosition;
    private ParticleSystem _fountainParticles;

    private Random _rng;

    protected override bool Create(IRenderer renderer)
    {
        _spiralPosition = renderer.Screen.Center + Vector.Down * 5 + Vector.Left * 30;
        _fountainPosition = renderer.Screen.Center + Vector.Down * 5 + Vector.Right * 30;
        _rng = new Random();
        
        var spiralSprites = Enum.GetValues<ConsoleColor>()
            .Select(c => new Sprite("*", c))
            .ToArray();

        var fountainSprites = new[]
        {
            new Sprite("*", Color24.Blue),
            new Sprite("*", Color24.DarkBlue),
            new Sprite("*", Color24.Cyan),
            new Sprite("*", Color24.DarkCyan),
            new Sprite("*", Color24.White),
        };

        _spiralParticles = new ParticleSystem(
            _spiralPosition,
            spiralSprites,
            initParticleAction: p => p.Velocity = GenerateSpiralForce(), 
            spawnInterval: 0.02f, 
            new ParticleOptions()
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
            new ParticleOptions()
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
    
    protected override bool Update(float elapsedTime, IRenderer renderer, PlayerInput input)
    {
        if(input.IsKeyHeld(KeyCode.Esc)) 
        {
            return false;
        }
        renderer.Fill(' ');
        
        _spiralParticles.Update(elapsedTime);
        _fountainParticles.Update(elapsedTime);

        foreach (var particle in _spiralParticles.ActiveParticles)
        {
            renderer.DrawObject(particle); 
        }
        
        foreach (var particle in _fountainParticles.ActiveParticles)
        {
            renderer.DrawObject(particle); 
        }
        
        return true;
    }
}