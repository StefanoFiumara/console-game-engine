using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games;

// ReSharper disable once UnusedType.Global
public class Snake : ConsoleGameEngineBase
{
    private const char PlayerHead = '0';
    private const char PlayerBody = 'O';
    private const char Pellet = '*';
    private const char Wall = '#';

    private const float GameTick = 0.12f;

    private const int SnakeStartingSize = 3;

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
    private readonly GameObject _map;

    private float _gameTimer;

    public Snake()
    {
        InitConsole(50, 50, 14);
        _rng = new Random();

        // Create a 32x32 map with WALL chars along the edges.
        var map = "################################\n";
        for (int i = 0; i < 30; i++)
        {
            map += "#                              #\n";
        }
        map += "################################\n";
            
        _map = new GameObject(Sprite.Create(map));
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

        for (int i = 0; i < SnakeStartingSize; i++)
        {
            _body.Insert(0, _head - _input * i);
        }

        _food = _rng.NextVector(_map.Bounds);

        _gameTimer = GameTick - _level * 0.02f;

        return true;
    }

    protected override bool Update(float elapsedTime, PlayerInput input)
    {
        if (input.IsKeyDown(KeyCode.Esc))
        {
            return false;
        }

        Fill(ScreenRect, ' ');

        // TODO: create a toggle for different control methods (AI vs manual)
        // Handle Input
        // if(input.IsKeyHeld(KeyCode.Left)  && _snakeDirection != Vector.Right) _input = Vector.Left;
        // if(input.IsKeyHeld(KeyCode.Right) && _snakeDirection != Vector.Left)  _input = Vector.Right;
        // if(input.IsKeyHeld(KeyCode.Up)    && _snakeDirection != Vector.Down)  _input = Vector.Up;
        // if(input.IsKeyHeld(KeyCode.Down)  && _snakeDirection != Vector.Up)    _input = Vector.Down;

        // Ticks the game forward every GAME_TICK seconds.
        _gameTimer -= elapsedTime;
        if (_gameTimer <= 0f)
        {
            // Game ticks faster based on current level
            _gameTimer = GameTick - _level * 0.02f;

            (_input, var score) = DetermineNextDirection(_head);
            if (score == int.MinValue)
            {
                Debugger.Break();
            }
            
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
            if (_map.Sprite.GetGlyph((int) (_head.X - _map.Position.X), (int) (_head.Y - _map.Position.Y)) == Wall)
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

                // Make snake longer
                _body.Insert(0, _body[^1]);
            }
        }

        Draw(_food, Pellet, ConsoleColor.Red);

        foreach (var piece in _body)
        {
            var gfx = piece == _head ? PlayerHead : PlayerBody;
            Draw(piece, gfx, ConsoleColor.Green);
        }

        var title = $"SNAKE";

        DrawString(ScreenWidth / 2, 1, title, alignment: TextAlignment.Centered);
        DrawString(1,5, "Arrow Keys: Move");
        DrawString(1,7, "ESC: Exit");
        DrawString(1,10, $"High Score: {_highScore}");
        DrawString(1,12, $"Score: {_score}");
        DrawString(1,14, $"Level: {_level}");

        DrawObject(_map);

        return true;
    }
    
    private static readonly int LookAheadDepth = 10;
    private readonly Vector[] _directions = { Vector.Left, Vector.Right, Vector.Up, Vector.Down };
    private Dictionary<(Vector, Vector, int), int> _scoreMemo = new();
    
    private (Vector direction, int score) DetermineNextDirection(Vector startingPos)
    {
        _scoreMemo = new Dictionary<(Vector, Vector, int), int>();
        var scores = new Dictionary<Vector, int>
        {
            [Vector.Left] = 0,
            [Vector.Right] = 0,
            [Vector.Up] = 0,
            [Vector.Down] = 0
        };
    
        foreach (var direction in scores.Keys)
        {
            scores[direction] = GetScoreForPosition(startingPos, direction, LookAheadDepth);
        }
        
        var maxScore = scores.Values.Max();
        var nextDirection = scores.First(kvp => kvp.Value == maxScore).Key;
        return (nextDirection, maxScore);
    }
    
    private int GetScoreForPosition(Vector startingPos, Vector direction, int depth)
    {
        var nextPosition = startingPos + direction;
        if (depth == 0) return 0;
        
        if (_scoreMemo.TryGetValue((nextPosition, direction, depth), out int cachedScore))
        {
            return cachedScore;
        }
    
        if (_body.Any(b => b == nextPosition) || _map.Sprite.GetGlyph((int)(nextPosition.X - _map.Position.X), (int)(nextPosition.Y - _map.Position.Y)) == Wall) 
        {
            return _scoreMemo[(nextPosition, direction, depth)] = int.MinValue;
        }
    
        int score = 1000; // base score for non-collision move

        // Add to score based on closeness to the food
        var distanceToFood = (int)(_food - nextPosition).Magnitude;
        score -= distanceToFood * 10;

        if (distanceToFood == 0) score += 1000;

        // Add to score based on availability of next potential moves
        var freedSpacesAfterMove = _directions.Count(dir => IsSpaceFree(nextPosition + dir));
        score += freedSpacesAfterMove * 500;

        // Other factors could be added here (like distance to tail)

        // Recur for look-ahead depth
        var futureScores = _directions.Select(dir => GetScoreForPosition(nextPosition, dir, depth - 1)).Max();
        return _scoreMemo[(nextPosition, direction, depth)] = score + futureScores;
    }

    private bool IsSpaceFree(Vector pos)
    {
        return _body.All(b => b != pos) && _map.Sprite.GetGlyph((int)(pos.X - _map.Position.X), (int)(pos.Y - _map.Position.Y)) != Wall;
    }

}