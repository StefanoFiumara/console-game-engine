using System.Collections.Generic;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Physics;

namespace ConsoleGameEngine.Runner.Games;

// ReSharper disable once UnusedType.Global
public class Physics() : ConsoleGame(width: 160, height: 120)
{
    private const float Gravity = 25f;
    private const float TerminalVelocity = 55f;

    private const float Friction = 15f;

    private const float MoveAccel = 50f;
    private const float JumpVel = 40f;

    private const int MaxTrailCount = 40;
    private const float TrailResetTime = 0.02f;
    
    private static readonly Color24 PlayerColor = Color24.White;
    private static readonly Color24 TrailColor = Color24.Red;
        
    private PhysicsEntity _player;

    private List<Vector> _trail;
    private float _trailCooldown;

    protected override bool Create(IRenderer renderer)
    {
        // TODO: Upgrade to use physics Engine
        _player = new PhysicsEntity(Sprite.CreateSolid(3,3, PlayerColor), renderer.Bounds.Center);
        _trail = new List<Vector>(MaxTrailCount);
        _trailCooldown = TrailResetTime;
            
        return true;
    }

    protected override bool Update(float elapsedTime, IRenderer renderer, PlayerInput input)
    {
        // Clear the screen each frame
        renderer.Fill(' ');
            
        if(input.IsKeyHeld(KeyCode.Esc)) 
        {
            return false;
        }

        // Input
        if (input.IsKeyHeld(KeyCode.Left))
        {
            _player.Velocity += Vector.Left * MoveAccel * elapsedTime;
        }
        else if (input.IsKeyHeld(KeyCode.Right))
        {
            _player.Velocity += Vector.Right * MoveAccel * elapsedTime;
        }

        if (input.IsKeyHeld(KeyCode.Space) && _player.Velocity.Y > 0f)
        {
            _player.Velocity += Vector.Up * JumpVel; // No elapsedTime here, instant force.
        }
        else
        {
            // Apply friction along the X axis to slowly bring the player to a stop
            if (_player.Velocity.X < 0)
            {
                _player.Velocity += Vector.Right * Friction * elapsedTime;
                if (_player.Velocity.X > 0)
                {
                    _player.Velocity = new Vector(0, _player.Velocity.Y);
                }

            }
            else
            {
                _player.Velocity += Vector.Left * Friction * elapsedTime;
                if (_player.Velocity.X < 0)
                {
                    _player.Velocity = new Vector(0, _player.Velocity.Y);
                }
            }
                
        }
            
        // Gravity
        _player.Velocity += Vector.Down * Gravity * elapsedTime;
            
        // Clamp to terminal velocity
        if (_player.Velocity.Y > TerminalVelocity)
        {
            _player.Velocity = new Vector(_player.Velocity.X, TerminalVelocity);
        }

        // Calculate position based on velocity
        _player.Position += _player.Velocity * elapsedTime;
            
        // Check for Collisions
        if ((int)_player.Position.Y + _player.Bounds.Height > renderer.Height+1)
        {
            _player.Position = new Vector(_player.Position.X, renderer.Height - _player.Bounds.Height);
            _player.Velocity = new Vector(_player.Velocity.X, -_player.Velocity.Y * 0.9f);
        }

        if (_player.Position.X <= 0)
        {
            _player.Position = new Vector(0, _player.Position.Y);
            _player.Velocity = new Vector(-_player.Velocity.X, _player.Velocity.Y);
        }
        else if ((int)_player.Position.X + _player.Bounds.Width > renderer.Width)
        {
            _player.Position = new Vector(renderer.Width - _player.Bounds.Width, _player.Position.Y);
            _player.Velocity = new Vector(-_player.Velocity.X, _player.Velocity.Y);
        }
            
        // Calculate Trail
        _trailCooldown -= elapsedTime;
        if (_trailCooldown < 0f)
        {
            if (_trail.Count == MaxTrailCount)
            {
                _trail.RemoveAt(0);
            }
                
            _trail.Add(_player.Bounds.Center);
            _trailCooldown = TrailResetTime;
        }
            
        ////////////////////////
        // Draw trail and player
        var trailGradient = Color24.CreateGradient(_trail.Count, Color24.Black, TrailColor);
        for (int i = 0; i < _trail.Count; i++)
        {
            var (x, y) = ((int)_trail[i].X, (int)_trail[i].Y);
                
            renderer.Draw(x, y, '*', trailGradient[i]);
        }
            
        renderer.DrawObject(_player);

        // HUD
        renderer.DrawString(1,1, "INSTRUCTIONS");
        renderer.DrawString(1,3, "  LEFT/RIGHT: Move Player");
        renderer.DrawString(1,5, "  SPACE: Jump");
        renderer.DrawString(1,7, "  ESC: Exit Game");

        return true;
    }
}