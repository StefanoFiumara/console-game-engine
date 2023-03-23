using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Physics;

namespace ConsoleGameEngine.Runner.Games
{
    using static Math;

    // ReSharper disable once UnusedType.Global
    public class Maze : ConsoleGameEngineBase
    {
        protected override string Name => "Maze";

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

        public Maze()
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

        protected override bool Update(float elapsedTime, KeyboardInput input)
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
            
            int raycastStepCount = 0;
            // Basic raycast algorithm for each column on the screen
            for (int x = 0; x < ScreenWidth; x++)
            {
                // Create ray vector
                double rayAngle = (_playerAngle - _fieldOfView / 2.0d) + ((x / (double) ScreenWidth) * _fieldOfView);
                var direction = new Vector((float) Sin(rayAngle), (float) Cos(rayAngle));

                var ray = Raycast.Send(_map, _playerPosition, direction, '#', _boundaryTolerance);
                raycastStepCount += ray.StepCount;
                
                // Use distance to wall to determine ceiling and floor height for this column
                // From the midpoint (height / 2), subtract an amount proportional to the distance of the wall
                int ceiling = (int) (ScreenHeight / 2f - ScreenHeight / ray.Distance);
                // Floor is mirror of ceiling
                int floor = ScreenHeight - ceiling;


                // Render column based on ceiling and floor values
                // TODO: use shading map to determine colors
                for (int y = 0; y < ScreenHeight; y++)
                {
                    if (y < ceiling)
                    {
                        Draw(x, y, ' ', bgColor: ConsoleColor.Cyan);
                    }
                    else if (y >= ceiling && y <= floor)
                    {
                        // TODO: Clean up shading logic
                        char emptyShade = '#';
                        char lightShade = 'X';
                        char mediumShade = '.';
                        char darkShade = '-';
                        char fullShade = '.';

                        char shade;
                        var fgColor = ConsoleColor.White;
                        var bgColor = ConsoleColor.DarkGray;

                        switch (ray.Distance)
                        {
                            case <= Raycast.MAX_RAYCAST_DEPTH / 4f:
                                shade = fullShade;
                                fgColor = ConsoleColor.Gray;
                                bgColor = ConsoleColor.White;
                                break;
                            case <= Raycast.MAX_RAYCAST_DEPTH / 3f:
                                shade = darkShade;
                                fgColor = ConsoleColor.Gray;
                                bgColor = ConsoleColor.White;
                                break;
                            case <= Raycast.MAX_RAYCAST_DEPTH / 2f:
                                shade = mediumShade;
                                fgColor = ConsoleColor.DarkGray;
                                bgColor = ConsoleColor.Gray;
                                break;
                            case <= Raycast.MAX_RAYCAST_DEPTH:
                                fgColor = ConsoleColor.Gray;
                                bgColor = ConsoleColor.DarkGray;
                                shade = lightShade;
                                break;
                            default:
                                fgColor = ConsoleColor.Magenta;
                                shade = emptyShade;
                                break;
                        }

                        if (ray.HitBoundary)
                        {
                            fgColor = ConsoleColor.Black;
                            bgColor = ConsoleColor.Black;
                        }

                        Draw(x, y, shade, fgColor, bgColor);
                    }
                    else
                    {
                        float groundDistance = 1.0f - (y - ScreenHeight / 2.0f) / (ScreenHeight / 2.0f);

                        char emptyShade = '#';
                        char lightShade = 'X';
                        char mediumShade = '.';
                        char darkShade = '-';
                        char fullShade = ' ';

                        char shade;
                        var fgColor = ConsoleColor.Green;
                        ConsoleColor bgColor;

                        switch (groundDistance)
                        {
                            case <= 0.25f:
                                shade = fullShade;
                                fgColor = ConsoleColor.Green;
                                bgColor = ConsoleColor.Green;
                                break;
                            case <= 0.5f:
                                shade = darkShade;
                                fgColor = ConsoleColor.DarkGreen;
                                bgColor = ConsoleColor.Green;
                                break;
                            case <= 0.75f:
                                fgColor = ConsoleColor.Black;
                                bgColor = ConsoleColor.DarkGreen;
                                shade = mediumShade;
                                break;
                            case <= 0.9f:
                                fgColor = ConsoleColor.DarkGreen;
                                bgColor = ConsoleColor.Black;
                                shade = lightShade;
                                break;
                            default:
                                shade = emptyShade;
                                bgColor = ConsoleColor.Magenta;
                                break;
                        }

                        Draw(x, y, shade, fgColor, bgColor);
                    }
                }
            }

            // Draw HUD
            DrawSprite(_map);
            DrawString(0, 0, $"Raycast steps: {raycastStepCount}");
            DrawString(0, 1, $"Boundary Tol: {_boundaryTolerance}");
            Draw(_map.Position + _playerPosition.Rounded, 'X', ConsoleColor.Red);

            return !input.IsKeyDown(KeyCode.Esc);
        }
    }
}
