using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;


namespace ConsoleGameEngine.Example
{
    public class CustomConsoleGameExample : ConsoleGameEngineBase
    {
        private const float GRAVITY = 25f;
        private const float TERMINAL_VELOCITY = 55f;

        private const float FRICTION = 15f;

        private const float MOVE_ACCEL = 50f;
        private const float JUMP_VEL = 40f;

        private const int MAX_TRAIL_COUNT = 40;
        private const float TRAIL_RESET_TIME = 0.02f;

        private const ConsoleColor BG_COLOR = ConsoleColor.DarkBlue;
        private const ConsoleColor PLAYER_COLOR = ConsoleColor.White;
        private const ConsoleColor TRAIL_COLOR = ConsoleColor.Red;
        
        private Sprite _player;

        private List<Vector> _trail;
        private float _trailCooldown;

        protected override bool Create()
        {
            var spriteGfx = string.Empty;

            spriteGfx += " ---\n";
            spriteGfx += "|   |\n";
            spriteGfx += "| * |\n";
            spriteGfx += "|   |\n";
            spriteGfx += " ---\n";

            _player = new Sprite(spriteGfx, PLAYER_COLOR, BG_COLOR);
            
            _trail = new List<Vector>(MAX_TRAIL_COUNT);

            _trailCooldown = TRAIL_RESET_TIME;

            _player.Position = new Vector(ScreenWidth / 2f, ScreenHeight / 2f);

            PerformanceModeEnabled = true;
            return true;
        }

        protected override bool Update(float elapsedTime)
        {
            // Clear the screen each frame
            Fill(ScreenRect, ' ', bgColor: BG_COLOR);

            if(IsKeyDown(Keys.Esc)) 
            {
                // Close the game
                return false;
            }

            // Input
            if (IsKeyDown(Keys.Left))
            {
                _player.Velocity += Vector.Left * MOVE_ACCEL * elapsedTime;
            }
            else if (IsKeyDown(Keys.Right))
            {
                _player.Velocity += Vector.Right * MOVE_ACCEL * elapsedTime;
            }

            if (IsKeyDown(Keys.Space) && _player.Velocity.Y > 0f)
            {
                _player.Velocity += Vector.Up * JUMP_VEL; // No elapsedTime here, instant force.
            }
            else
            {
                if (_player.Velocity.X < 0)
                {
                    _player.Velocity += Vector.Right * FRICTION * elapsedTime;
                    if (_player.Velocity.X > 0)
                    {
                        _player.Velocity = new Vector(0, _player.Velocity.Y);
                    }

                }
                else
                {
                    _player.Velocity += Vector.Left * FRICTION * elapsedTime;
                    if (_player.Velocity.X < 0)
                    {
                        _player.Velocity = new Vector(0, _player.Velocity.Y);
                    }
                }
                
            }
            
            // Physics
            _player.Velocity += Vector.Down * GRAVITY * elapsedTime;
            
            // Clamp to terminal velocity
            if (_player.Velocity.Y > TERMINAL_VELOCITY)
            {
                _player.Velocity = new Vector(_player.Velocity.X, TERMINAL_VELOCITY);
            }

            _player.Position += _player.Velocity * elapsedTime;
            
            // Collision
            if ((int)_player.Position.Y + _player.Height > ScreenHeight+1)
            {
                _player.Position = new Vector(_player.Position.X, ScreenHeight - _player.Height);
                _player.Velocity = new Vector(_player.Velocity.X, -_player.Velocity.Y * 0.9f);
            }

            if (_player.Position.X <= 0)
            {
                _player.Position = new Vector(0, _player.Position.Y);
                _player.Velocity = new Vector(-_player.Velocity.X, _player.Velocity.Y);
            }
            else if ((int)_player.Position.X + _player.Width > ScreenWidth)
            {
                _player.Position = new Vector(ScreenWidth - _player.Width, _player.Position.Y);
                _player.Velocity = new Vector(-_player.Velocity.X, _player.Velocity.Y);
            }
            
            // Trail
            _trailCooldown -= elapsedTime;
            if (_trailCooldown < 0f)
            {
                if (_trail.Count == MAX_TRAIL_COUNT)
                {
                    _trail.RemoveAt(0);
                }
                
                _trail.Add(_player.Center);
                _trailCooldown = TRAIL_RESET_TIME;
            }
            
            ////////////////////////
            // Draw trail and player
            for (int i = 0; i < _trail.Count; i++)
            {
                var (x, y) = ((int)_trail[i].X, (int)_trail[i].Y);
                
                Draw(x, y, '*', TRAIL_COLOR, BG_COLOR);
            }
            
            DrawSprite(_player);

            // HUD
            DrawString(1,1, "INSTRUCTIONS", bgColor: BG_COLOR);
            DrawString(1,2, "  LEFT/RIGHT: Move Player", bgColor: BG_COLOR);
            DrawString(1,4, "  SPACE: Jump", bgColor: BG_COLOR);
            DrawString(1,5, "  ESC: Exit Game", bgColor: BG_COLOR);

            return true;
        }
    }
}