using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Physics;

namespace ConsoleGameEngine.Runner.Games
{
    using static Math;

    // ReSharper disable once UnusedType.Global
    public class FirstPersonShooter : ConsoleGameEngineBase
    {
        protected override string Name => "First Person Shooter";

        private const float TURN_SPEED = 2f;
        private const float MOVE_SPEED = 5.0f;

        private Vector _playerPosition;
        private float _playerAngle;

        private Vector PlayerFacingAngle => new(
            (float) Sin(_playerAngle),
            (float) Cos(_playerAngle));

        private readonly float _fieldOfView = 3.14159f / 4f; // 90 degree fov
        private Sprite _map;

        private float _boundaryTolerance = 0.005f;

        public FirstPersonShooter()
        {
            PerformanceModeEnabled = true;
            InitConsole(240, 135, 6);
        }

        protected override bool Create()
        {
            var map = "";
            map += "################\n";
            map += "#..............#\n";
            map += "#..............#\n";
            map += "#####..........#\n";
            map += "#..............#\n";
            map += "#.........#....#\n";
            map += "#..............#\n";
            map += "#..............#\n";
            map += "#..............#\n";
            map += "#..............#\n";
            map += "#.......########\n";
            map += "#..............#\n";
            map += "#...#..........#\n";
            map += "#...#..........#\n";
            map += "#...#..........#\n";
            map += "################\n";

            _map = new Sprite(map);
            _map.Position = new Vector(ScreenWidth - _map.Width, ScreenHeight - _map.Height);
            _playerPosition = new Vector(8, 8);
            _playerAngle = 0f;
            return true;
        }

        protected override bool Update(float elapsedTime, PlayerInput input)
        {
            // handle input
            if (input.IsKeyHeld(KeyCode.Left))  _playerAngle -= TURN_SPEED * elapsedTime;
            if (input.IsKeyHeld(KeyCode.Right)) _playerAngle += TURN_SPEED * elapsedTime;
            
            if (input.IsKeyHeld(KeyCode.Up))
            {
                _playerPosition += PlayerFacingAngle * MOVE_SPEED * elapsedTime;

                if (_map.GetGlyph((int) _playerPosition.Rounded.X, (int) _playerPosition.Rounded.Y) == '#')
                {
                    _playerPosition -= PlayerFacingAngle * MOVE_SPEED * elapsedTime;
                }
            }

            if (input.IsKeyHeld(KeyCode.Down))
            {
                _playerPosition -= PlayerFacingAngle * MOVE_SPEED * elapsedTime;
                if (_map.GetGlyph((int) _playerPosition.Rounded.X, (int) _playerPosition.Rounded.Y) == '#')
                {
                    _playerPosition += PlayerFacingAngle * MOVE_SPEED * elapsedTime;
                }
            }

            if (input.IsKeyHeld(KeyCode.Z))
            {
                _boundaryTolerance -= 0.01f * elapsedTime;
                if (_boundaryTolerance < 0f) _boundaryTolerance = 0f;
            }
            
            if (input.IsKeyHeld(KeyCode.X))
            {
                _boundaryTolerance += 0.01f * elapsedTime;
            }
            
            // Basic raycast algorithm for each column on the screen
            for (int x = 0; x < ScreenWidth; x++)
            {
                // Create ray vector
                double rayAngle = (_playerAngle - _fieldOfView / 2.0d) + ((x / (double) ScreenWidth) * _fieldOfView);
                var direction = new Vector((float) Sin(rayAngle), (float) Cos(rayAngle));

                var ray = Raycast.Send(_map, _playerPosition, direction, '#', _boundaryTolerance);

                // Use distance to wall to determine ceiling and floor height for this column
                // From the midpoint (height / 2), subtract an amount proportional to the distance of the wall
                int ceiling = (int) (ScreenHeight / 2f - ScreenHeight / ray.Distance);
                // Floor is mirror of ceiling
                int floor = ScreenHeight - ceiling;


                // Render column based on ceiling and floor values
                for (int y = 0; y < ScreenHeight; y++)
                {
                    if (y < ceiling)
                    {
                        Draw(x, y, ' ', bgColor: ConsoleColor.Cyan);
                    }
                    else if (y >= ceiling && y <= floor)
                    {
                        var shade = ray.HitBoundary ? Shade.Black : CalculateShade(WallShades, ray.Distance);
                        Draw(x, y, shade.Character, shade.ForegroundColor, shade.BackgroundColor);
                    }
                    else
                    {
                        var groundDistance = 1.0f - (y - ScreenHeight / 2.0f) / (ScreenHeight / 2.0f);
                        
                        var shade = CalculateShade(GroundShades, groundDistance);
                        Draw(x, y, shade.Character, shade.ForegroundColor, shade.BackgroundColor);
                    }
                }
            }

            // Draw HUD
            DrawSprite(_map);
            DrawString(ScreenWidth, (int)_map.Position.Y - 1, $"Boundary Tol: {_boundaryTolerance}", alignment: TextAlignment.Right);
            Draw(_map.Position + _playerPosition.Rounded, 'X', ConsoleColor.Red);

            return !input.IsKeyDown(KeyCode.Esc);
        }
        
        private static Shade CalculateShade(IEnumerable<Shade> shades, float groundDistance)
        {
            foreach (var shade in shades)
            {
                if (groundDistance <= shade.DistanceThreshold)
                {
                    return shade;
                }
            }

            return Shade.Default;
        }
        
        private static readonly List<Shade> WallShades = new()
        {
            new(Raycast.MAX_RAYCAST_DEPTH / 4f, Shade.MEDIUM_SHADE, ConsoleColor.Gray,     ConsoleColor.White),
            new(Raycast.MAX_RAYCAST_DEPTH / 3f, Shade.DARK_SHADE,   ConsoleColor.Gray,     ConsoleColor.White),
            new(Raycast.MAX_RAYCAST_DEPTH / 2f, Shade.LIGHT_SHADE,  ConsoleColor.DarkGray, ConsoleColor.Gray),
            new(Raycast.MAX_RAYCAST_DEPTH / 1f, Shade.MEDIUM_SHADE, ConsoleColor.DarkGray, ConsoleColor.DarkGray)
        };
        
        private static readonly List<Shade> GroundShades = new()
        {
            new(0.25f, Shade.FULL_SHADE,   ConsoleColor.Green,     ConsoleColor.Green),
            new(0.5f,  Shade.DARK_SHADE,   ConsoleColor.DarkGreen, ConsoleColor.Green),
            new(0.75f, Shade.MEDIUM_SHADE, ConsoleColor.Black,     ConsoleColor.DarkGreen),
            new(0.9f,  Shade.LIGHT_SHADE,  ConsoleColor.DarkGreen, ConsoleColor.Black)
        };
        
        private class Shade
        {
            public static readonly Shade Default = new(0, '#', ConsoleColor.Magenta, ConsoleColor.DarkMagenta);
            public static readonly Shade Black = new(0, ' ', ConsoleColor.Black, ConsoleColor.Black);
            
            public const char FULL_SHADE = ' ';
            public const char DARK_SHADE = '-';
            public const char MEDIUM_SHADE = '.';
            public const char LIGHT_SHADE = 'X';

            public float DistanceThreshold { get; }
            public char Character { get; }
            public ConsoleColor ForegroundColor { get; }
            public ConsoleColor BackgroundColor { get; }

            public Shade(float distanceThreshold, char character, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
            {
                DistanceThreshold = distanceThreshold;
                Character = character;
                ForegroundColor = foregroundColor;
                BackgroundColor = backgroundColor;
            }
        }
    }
}
