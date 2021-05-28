using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core;


namespace ConsoleGameEngine.Example
{
    public class CustomConsoleGameExample : ConsoleGameEngineBase
    {
        private float _posX = 0f;
        private float _posY = 0f;
        
        private float _velX = 0f;
        private float _velY = 0f;

        private float _gravity = 15f;

        private float _moveSpeed = 30f;
        private float _jumpSpeed = 40f;
        
        private List<string> _sprite;

        private List<(float x, float y)> _trail;
        private float _trailCooldown;
        private const int MAX_TRAIL_COUNT = 50;

        private float _trailReset = 0.5f;

        private const ConsoleColor BG_COLOR = ConsoleColor.Black;
        private const ConsoleColor PLAYER_COLOR = ConsoleColor.White;
        private const ConsoleColor TRAIL_COLOR = ConsoleColor.Red;

        protected override bool Create()
        {
            _sprite = new List<string>()
            {
                "-----",
                "|   |",
                "| * |",
                "|   |",
                "-----",
            };

            _trail = new List<(float x, float y)>(MAX_TRAIL_COUNT)
            {
                (_posX, _posY)
            };

            _trailCooldown = _trailReset;

            _posX = ScreenWidth / 2f;
            _posY = ScreenHeight / 2f;
            
            DrawSprite((int)_posX, (int)_posY, _sprite, bgColor: BG_COLOR);
            return true;
        }

        protected override bool Update(float elapsedTime)
        {
            Fill(0,0,ScreenWidth, ScreenHeight, ' ', bgColor: BG_COLOR);

            if (IsKeyDown(Keys.Left))
            {
                _velX -= _moveSpeed * elapsedTime;
            }
            
            if (IsKeyDown(Keys.Right))
            {
                _velX += _moveSpeed * elapsedTime;
            }

            if (IsKeyDown(Keys.Up) && _velY > 0f)
            {
                _velY = -_jumpSpeed;
            }
            
            if(IsKeyDown(Keys.Space)) 
            {
                return false;
            }

            _trailCooldown -= elapsedTime;
            if (_trailCooldown < 0f)
            {
                if (_trail.Count == MAX_TRAIL_COUNT)
                {
                    _trail.RemoveAt(0);
                }
                
                _trail.Add((_posX, _posY));
            }
            
            _posX += _velX * elapsedTime;
            _posY += _velY * elapsedTime;

            _velY += _gravity * elapsedTime;

            if ((int)_posY + _sprite.Count >= ScreenHeight)
            {
                _posY = ScreenHeight - _sprite.Count;
                _velY *= -0.8f;
            }

            if (_posX <= 0)
            {
                _posX = 0;
                _velX *= -0.9f;
            }
            else if ((int)_posX + _sprite[0].Length >= ScreenWidth)
            {
                _posX = ScreenWidth - _sprite[0].Length;
                _velX *= -0.9f;
            }
            
            // Render trail
            for (int i = 0; i < _trail.Count; i++)
            {
                var (x, y) = _trail[i];
                
                Draw((int) x + _sprite[0].Length/2, (int) y+ _sprite.Count/2, '*', TRAIL_COLOR, BG_COLOR);
            }
            
            DrawSprite((int)_posX, (int)_posY, _sprite, PLAYER_COLOR, bgColor: BG_COLOR);
            return true;
        }
    }
}