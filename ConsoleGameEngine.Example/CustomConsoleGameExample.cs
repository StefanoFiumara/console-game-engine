using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;


namespace ConsoleGameEngine.Example
{
    public class CustomConsoleGameExample : ConsoleGameEngineBase
    {
        private const float GRAVITY = 15f;

        private const float MOVE_SPEED = 30f;
        private const float JUMP_SPEED = 40f;

        private const int MAX_TRAIL_COUNT = 50;
        private const float TRAIL_RESET_TIME = 0.5f;

        private const ConsoleColor BG_COLOR = ConsoleColor.Blue;
        private const ConsoleColor PLAYER_COLOR = ConsoleColor.White;
        private const ConsoleColor TRAIL_COLOR = ConsoleColor.Red;
        
        private Sprite _player;

        private List<Vector> _trail;
        private float _trailCooldown;

        protected override bool Create()
        {
            var spriteGfx = string.Empty;

            spriteGfx += "-----\n";
            spriteGfx += "|   |\n";
            spriteGfx += "| * |\n";
            spriteGfx += "|   |\n";
            spriteGfx += "-----\n";

            _player = new Sprite(spriteGfx, PLAYER_COLOR);
            
            _trail = new List<Vector>(MAX_TRAIL_COUNT);

            _trailCooldown = TRAIL_RESET_TIME;

            _player.Position = new Vector(ScreenWidth / 2f, ScreenHeight / 2f);

            return true;
        }

        protected override bool Update(float elapsedTime)
        {
            // Clear the screen each frame
            Fill(0,0,ScreenWidth, ScreenHeight, ' ', bgColor: BG_COLOR);

            // Input
            if (IsKeyDown(Keys.Left))
            {
                _player.Velocity += Vector.Left * MOVE_SPEED * elapsedTime;
            }
            else if (IsKeyDown(Keys.Right))
            {
                _player.Velocity += Vector.Right * MOVE_SPEED * elapsedTime;
            }
            else
            {
                var friction = _player.Velocity.X < 0 ? MOVE_SPEED / 2 : -MOVE_SPEED / 2;
                _player.Velocity += Vector.Right * friction * elapsedTime;
            }

            if (IsKeyDown(Keys.Up) && _player.Velocity.Y > 0f)
            {
                _player.Velocity += Vector.Up * JUMP_SPEED; // No elapsedTime here, instant force.
            }
            
            if(IsKeyDown(Keys.Space)) 
            {
                return false;
            }

            // Trail
            _trailCooldown -= elapsedTime;
            if (_trailCooldown < 0f)
            {
                if (_trail.Count == MAX_TRAIL_COUNT)
                {
                    _trail.RemoveAt(0);
                }
                
                _trail.Add(_player.Position);
            }
            
            // Physics
            _player.Velocity += Vector.Down * GRAVITY * elapsedTime;
            
            _player.Position += _player.Velocity * elapsedTime;

            // Collision
            if ((int)_player.Position.Y + _player.Height >= ScreenHeight+1)
            {
                _player.Position = new Vector(_player.Position.X, ScreenHeight - _player.Height);
                _player.Velocity = new Vector(_player.Velocity.X, -_player.Velocity.Y);
            }

            if (_player.Position.X <= 0)
            {
                _player.Position = new Vector(0, _player.Position.Y);
                _player.Velocity = new Vector(-_player.Velocity.X, _player.Velocity.Y);
            }
            else if ((int)_player.Position.X + _player.Width >= ScreenWidth+1)
            {
                _player.Position = new Vector(ScreenWidth - _player.Width, _player.Position.Y);
                _player.Velocity = new Vector(-_player.Velocity.X, _player.Velocity.Y);
            }
            
            ////////////////////////
            // Draw trail and player
            for (int i = 0; i < _trail.Count; i++)
            {
                var (x, y) = (_trail[i].X, _trail[i].Y);
                
                Draw((int) (x + _player.Width/2), (int) (y + _player.Height/2), '*', TRAIL_COLOR, BG_COLOR);
            }
            
            DrawSprite(_player);
            
            // HUD
            DrawString(2,2, $"Player X: {(int)_player.Position.X}");
            DrawString(2,3, $"Player Y: {(int)_player.Position.Y}");
            
            DrawString(2,5, $"Player Vel X: {_player.Velocity.X:F2}");
            DrawString(2,6, $"Player Vel Y: {_player.Velocity.Y:F2}");

            

            return true;
        }
    }
}