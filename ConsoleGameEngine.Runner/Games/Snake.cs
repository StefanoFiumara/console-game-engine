using System;
using System.Collections.Generic;
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
    private int _score;
    private int _highScore;

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
            // TEMP: Testing fast AI speed
            _gameTimer = 0f;
            // _gameTimer = GameTick - _level * 0.02f;

            // Snake Movement
            _input = DetermineNextDirection(_head);
            
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
            if (_map.Sprite[(int) (_head.X - _map.Position.X), (int) (_head.Y - _map.Position.Y)] == Wall)
            {
                return Create(); // Reset Game
            }

            // Collision Check against Food pellet
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
   
        // Render
        Draw(_food, Pellet, Color24.Red);

        foreach (var piece in _body)
        {
            var gfx = piece == _head ? PlayerHead : PlayerBody;
            Draw(piece, gfx, Color24.Green);
        }

        DrawString(ScreenWidth / 2, 1, "SNAKE", alignment: TextAlignment.Centered);
        DrawString(1,5, "Arrow Keys: Move");
        DrawString(1,7, "ESC: Exit");
        DrawString(1,10, $"High Score: {_highScore}");
        DrawString(1,12, $"Score: {_score}");
        DrawString(1,14, $"Level: {_level}");

        DrawObject(_map);

        return true;
    }
    
    private readonly Vector[] _directions = { Vector.Left, Vector.Right, Vector.Up, Vector.Down };
    
    private Vector DetermineNextDirection(Vector startingPos)
    {
        var scores = new Dictionary<Vector, float>
        {
            [Vector.Left] = 0,
            [Vector.Right] = 0,
            [Vector.Up] = 0,
            [Vector.Down] = 0
        };
    
        foreach (var direction in _directions)
        {
            if (direction == -_snakeDirection)
            {
                scores.Remove(direction);
            }
            else
            {
                scores[direction] = GetScoreForPosition(startingPos, direction);
            }
        }
        
        var maxScore = scores.Values.Max();
        var potentialDirections = scores.Where(kvp => Math.Abs(kvp.Value - maxScore) < 0.01f).ToList();
        
        var randomIndex = _rng.Next(potentialDirections.Count);
        var nextDirection = potentialDirections[randomIndex].Key;
        
        return nextDirection;
    }
    
    private float GetScoreForPosition(Vector startingPos, Vector direction)
    {
        var nextPosition = startingPos + direction;
        
        if (!IsSpaceFree(nextPosition)) 
        {
            return float.MinValue;
        }
    
        float score = 10000; // base score for non-collision move
        
        var pathDistance = CalculatePathLength(nextPosition, _food);
        if (pathDistance != int.MaxValue)
        {
            // Score based on proximity to food.
            score += 1500;
            score -= pathDistance * 150;
        }
        
        score += CalculateOpenArea(nextPosition) * 100;
        score += DistanceToWall(nextPosition) * 150;

        return score;
    }

    private float DistanceToWall(Vector nextPosition)
    {
        var topWallDistance = nextPosition.Y - _map.Position.Y;
        var bottomWallDistance = (_map.Position.Y + _map.Bounds.Height) - nextPosition.Y;
        var leftWallDistance = nextPosition.X - _map.Position.X;
        var rightWallDistance = (_map.Position.X + _map.Bounds.Width) - nextPosition.X;

        return new[] { topWallDistance, bottomWallDistance, leftWallDistance, rightWallDistance }.Min();
    }

    private bool IsSpaceFree(Vector pos)
    {
        var isMapSpaceFree = _map.Sprite[(int)(pos.X - _map.Position.X), (int)(pos.Y - _map.Position.Y)] != Wall;
        bool isBodyHit = false;
        
        for (int i = 0; i < _body.Count; i++)
        {
            if (pos == _body[i])
            {
                isBodyHit = true;
                break;
            }
        }

        return isMapSpaceFree && !isBodyHit;
    }

    private int CalculatePathLength(Vector start, Vector target)
    {
        //from the graph, get the starting node and set its distance to 0
        //this node is the closest to the starting node because it IS the starting node.
        if (start == target) return 0;
        
        var visited = new HashSet<Vector>();
        var distanceMap = new Dictionary<Vector, (int g, float f)>()
        {
            {start, (0, ManhattanDistance(start, target)) }
        };
        
        while (true)
        {
            var nextUnvisited = distanceMap.Where(kvp => !visited.Contains(kvp.Key)).ToList();
            if (nextUnvisited.Count == 0)
            {
                return int.MaxValue;
            }
            
            var current = nextUnvisited.MinBy(kvp => kvp.Value.f);
            
            visited.Add(current.Key);
            
            var neighbors = _directions
                .Select(d => current.Key + d)
                .Where(IsSpaceFree)
                .Where(n => !visited.Contains(n));
            
            foreach (var neighbor in neighbors)
            {
                int tentativeG = distanceMap[current.Key].g + 1;
                float h = ManhattanDistance(neighbor, target);
                float tentativeF = tentativeG + h;

                if (distanceMap.ContainsKey(neighbor))
                {
                    if (tentativeF < distanceMap[neighbor].f)
                    {
                        distanceMap[neighbor] = (tentativeG, tentativeF);
                    }
                }
                else
                {
                    distanceMap.Add(neighbor, (tentativeG, tentativeF));
                }

                if (neighbor == target)
                {
                    return tentativeG;
                }
            }
        }
    }

    private static float ManhattanDistance(Vector start, Vector end)
    {
        return Math.Abs(end.X - start.X) + Math.Abs(end.Y - start.Y);
    }

    private int CalculateOpenArea(Vector pos)
    {
        var visited = new HashSet<Vector> { pos };
        var queue = new Queue<Vector>();
        queue.Enqueue(pos);
        int count = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            count++;
            foreach (var direction in _directions)
            {
                var next = current + direction;
                if (IsSpaceFree(next) && visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }
        return count;
    }
}