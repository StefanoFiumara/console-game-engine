using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games
{
    // ReSharper disable once UnusedType.Global
    public class Snake : ConsoleGameEngineBase
    {
        protected override string Name { get; set; } = "Snake Game";

        private const char PLAYER_HEAD = '0';
        private const char PLAYER_BODY = 'O';
        private const char PELLET = '*';
        
        private const char WALL = '#';
        
        private const float GAME_TICK = 0.12f;

        private const int SNAKE_STARTING_SIZE = 3;

        private Vector _input;
        private Vector _snakeDirection;

        private Vector _food;

        private Vector _head;
        private  List<Vector> _body;

        private int _level = 1;
        private int _nextLevelGoal = 10;
        private int _score = 0;
        private int _highScore = 0;

        private readonly Random _rng;
        private readonly Sprite _map;
        
        private float _gameTimer;

        public Snake()
        {
            InitConsole(50, 50, 14);
            
            _rng = new Random();

            // Create a 32x32 map with WALL chars along the edges.
            var mapLayout = "################################\n";
            for (int i = 0; i < 30; i++)
            {
                mapLayout += "#                              #\n";
            }
            mapLayout += "################################\n";
            _map = new Sprite(mapLayout);
            _map.Position = ScreenRect.Center - _map.Bounds.Size * 0.5f + 7 * Vector.Down;
        }

        protected override bool Create()
        {
            _score = 0;
            _input = Vector.Right;
            _level = 1;
            _nextLevelGoal = 10;
            
            _head = _map.Bounds.Center;
            _body = new List<Vector>();
            
            for (int i = 0; i < SNAKE_STARTING_SIZE; i++)
            {
                _body.Insert(0, _head - _input * i);
            }

            _food = _rng.NextVector(_map.Bounds);

            _gameTimer = GAME_TICK - _level * 0.02f;
            
            return true;
        }

        protected override bool Update(float elapsedTime)
        {
            if (IsKeyDown(Keys.Esc))
            {
                return false;
            }

            Fill(ScreenRect, ' ');

            // Handle Input
            if(IsKeyDown(Keys.Left)  && _snakeDirection != Vector.Right) _input = Vector.Left;
            if(IsKeyDown(Keys.Right) && _snakeDirection != Vector.Left)  _input = Vector.Right;
            if(IsKeyDown(Keys.Up)    && _snakeDirection != Vector.Down)  _input = Vector.Up;
            if(IsKeyDown(Keys.Down)  && _snakeDirection != Vector.Up)    _input = Vector.Down;

            // Ticks the game forward every GAME_TICK seconds.
            _gameTimer -= elapsedTime;
            if (_gameTimer <= 0f)
            {
                _gameTimer = GAME_TICK - _level * 0.02f;

                _snakeDirection = _input;
                _head += _snakeDirection;
                _body.Add(_head);
                _body.RemoveAt(0);
                
                // Collision check against the tail pieces.
                for (int i = 0; i < _body.Count-1; i++)
                {
                    if (_body[i] == _head)
                    {
                        return Create(); // Reset game.
                    }
                }
                    
                // Collision check against the game bounds.
                if (_map.GetGlyph((int) (_head.X - _map.Position.X), (int) (_head.Y - _map.Position.Y)) == WALL)
                {
                    return Create(); // Reset Game
                }
                
                if (_head.Rounded == _food.Rounded)
                {
                    _score++;
                    if (_score > _highScore)
                    {
                        _highScore = _score;
                    }

                    if (_score >= _nextLevelGoal)
                    {
                        _level++;
                        _nextLevelGoal += 10;
                    }

                    do
                    {
                        _food = _rng.NextVector(_map.Bounds);
                    } while (_body.Contains(_food)); // Ensure food pellet doesn't spawn in snake's body. 
                    
                    _body.Insert(0, _body[^1]);
                }
            }
            
            Draw(_food, PELLET, ConsoleColor.Red);

            foreach (var piece in _body)
            {
                var gfx = piece == _head ? PLAYER_HEAD : PLAYER_BODY;
                Draw(piece, gfx, ConsoleColor.Green);
            }

            var title = $"SNAKE GAME";
            
            DrawString(1,1, title.PadLeft(ScreenWidth/2 + title.Length/2));
            DrawString(1,3, $"Up/Down/Left/Right: Move");
            DrawString(1,5, $"ESC: Exit");
            DrawString(1,9, $"High Score: {_highScore}");
            DrawString(1,11, $"Score: {_score}");
            DrawString(1,13, $"Level: {_level}");
            
            DrawSprite(_map);
            
            return true;
        }
    }
}