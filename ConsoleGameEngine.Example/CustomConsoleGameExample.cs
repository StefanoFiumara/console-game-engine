using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core;


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
        
        // TODO: Vectors
        private float _posX;
        private float _posY;
        
        private float _velX;
        private float _velY;
   
        private List<string> _player;

        private List<(float x, float y)> _trail;
        private float _trailCooldown;

        protected override bool Create()
        {
            // TODO: Encapsulate in Sprite class
            _player = new List<string>()
            {
                "-----",
                "|   |",
                "| * |",
                "|   |",
                "-----",
            };

            _trail = new List<(float x, float y)>(MAX_TRAIL_COUNT);

            _trailCooldown = TRAIL_RESET_TIME;

            _posX = ScreenWidth / 2f;
            _posY = ScreenHeight / 2f;
            
            return true;
        }

        protected override bool Update(float elapsedTime)
        {
            // Clear the screen each frame
            Fill(0,0,ScreenWidth, ScreenHeight, ' ', bgColor: BG_COLOR);

            // Input
            if (IsKeyDown(Keys.Left))
            {
                _velX -= MOVE_SPEED * elapsedTime;
            }
            else if (IsKeyDown(Keys.Right))
            {
                _velX += MOVE_SPEED * elapsedTime;
            }
            else
            {
                var friction = _velX < 0 ? MOVE_SPEED / 2 : -MOVE_SPEED / 2;
                _velX += friction * elapsedTime;
            }

            if (IsKeyDown(Keys.Up) && _velY > 0f)
            {
                _velY = -JUMP_SPEED;
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
                
                _trail.Add((_posX, _posY));
            }
            
            // Physics
            _posX += _velX * elapsedTime;
            _posY += _velY * elapsedTime;

            _velY += GRAVITY * elapsedTime;

            // Collision
            if ((int)_posY + _player.Count >= ScreenHeight+1)
            {
                _posY = ScreenHeight - _player.Count;
                _velY *= -0.8f;
            }

            if (_posX <= 0)
            {
                _posX = 0;
                _velX *= -0.9f;
            }
            else if ((int)_posX + _player[0].Length >= ScreenWidth+1)
            {
                _posX = ScreenWidth - _player[0].Length;
                _velX *= -0.9f;
            }
            
            ////////////////////////
            // Draw trail and player
            for (int i = 0; i < _trail.Count; i++)
            {
                var (x, y) = _trail[i];
                
                Draw((int) x + _player[0].Length/2, (int) y+ _player.Count/2, '*', TRAIL_COLOR, BG_COLOR);
            }
            
            DrawSprite((int)_posX, (int)_posY, _player, PLAYER_COLOR, bgColor: BG_COLOR);
            
            DrawString(2,2, $"Player Vel X: {_velX:F}");
            DrawString(2,4, $"Player Vel Y: {_velY:F}");
            return true;
        }
    }
}