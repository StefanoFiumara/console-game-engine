using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games
{
    using static Math;
    
    // ReSharper disable once UnusedType.Global
    public class Maze : ConsoleGameEngineBase
    {
        protected override string Name => "First Person Shooter";

        private const float TURN_SPEED = 2f;
        private const float MOVE_SPEED = 5.0f;
        private const float RAYCAST_STEP = 0.05f;
        
        private Vector _playerPosition;
        private float _playerAngle;
        
        private Vector PlayerFacingAngle => new(
            (float) Sin(_playerAngle), 
            (float) Cos(_playerAngle));

        private float _fieldOfView = 3.14159f / 4f; // 90 degree fov
        private Sprite _map;

        public Maze()
        {
            PerformanceModeEnabled = true;
            InitConsole(160, 120, 8);
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
            
            
            
            // Basic raycast algorithm for each column on the screen
            for (int x = 0; x < ScreenWidth; x++)
            {
                // Create ray vector
                double rayAngle = (_playerAngle - _fieldOfView / 2.0f) + ((x / (double) ScreenWidth) * _fieldOfView);
                float distanceToWall = 0f;
                float maxDepth = 16f;
                bool hitWall = false;
                bool hitBoundary = false;

                var direction = new Vector((float) Sin(rayAngle), (float) Cos(rayAngle));

                // Calculate distance 
                while (!hitWall && distanceToWall < maxDepth) 
                {
                    distanceToWall += RAYCAST_STEP;

                    var testPos = (_playerPosition + direction * distanceToWall);

                    // Test position on map
                    if (testPos.X < 0 || testPos.X >= _map.Width ||
                        testPos.Y < 0 || testPos.Y >= _map.Height)
                    {
                        // Out of bounds
                        hitWall = true;
                        distanceToWall = maxDepth;
                    }
                    else if (_map.GetGlyph(testPos) == '#')
                    {
                        hitWall = true;

                        // To highlight tile boundaries, cast a ray from each corner
                        // of the tile, to the player. The more coincident this ray
                        // is to the rendering ray, the closer we are to a tile 
                        // boundary, which we'll shade to add detail to the walls
                        var boundaryRays = new List<(float distance, float dotProduct)>(4);

                        // Test each corner of hit tile, storing the distance from
                        // the player, and the calculated dot product of the two rays
                        for (int cornerX = 0; cornerX < 2; cornerX++)
                        {
                            for (int cornerY = 0; cornerY < 2; cornerY++)
                            {
                                // Angle of corner to eye
                                var cornerRay = new Vector(
                                    testPos.Rounded.X + cornerX - _playerPosition.X, 
                                    testPos.Rounded.Y + cornerY - _playerPosition.Y);

                                // TODO: formalize dot product in Vector Class
                                float dot = (direction.X * cornerRay.X / cornerRay.Magnitude) + (direction.Y * cornerRay.Y / cornerRay.Magnitude);

                                boundaryRays.Add((cornerRay.Magnitude, dot));
                            }
                        }

                        // Sort Pairs from closest to farthest
                        boundaryRays = boundaryRays.OrderBy(v => v.distance).ToList();

                        // First two/three are closest (we will never see all four)
                        float fBound = 0.0025f;
                        if (Acos(boundaryRays[0].dotProduct) < fBound) hitBoundary = true;
                        if (Acos(boundaryRays[1].dotProduct) < fBound) hitBoundary = true;
                        if (Acos(boundaryRays[2].dotProduct) < fBound) hitBoundary = true;
                    }
                }
                
                // Use distance to wall to determine ceiling and floor height for this column
                // From the midpoint (height / 2), subtract an amount proportional to the distance of the wall
                int ceiling = (int) (ScreenHeight / 2f - ScreenHeight / distanceToWall);
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
                        // TODO: Clean up shading logic
                        char emptyShade = '#';
                        char lightShade = 'X';
                        char mediumShade = '.';
                        char darkShade = '-';
                        char fullShade = '.';

                        char shade;
                        var fgColor = ConsoleColor.White;
                        var bgColor = ConsoleColor.DarkGray;

                        if (distanceToWall <= maxDepth / 4f)
                        {
                            shade = fullShade; 
                            fgColor = ConsoleColor.Gray;
                            bgColor = ConsoleColor.White;
                        }
                        else if (distanceToWall <= maxDepth / 3f)
                        {
                            shade = darkShade;
                            fgColor = ConsoleColor.Gray;
                            bgColor = ConsoleColor.White;
                        }
                        else if (distanceToWall <= maxDepth / 2f)
                        {
                            shade = mediumShade;
                            fgColor = ConsoleColor.DarkGray;
                            bgColor = ConsoleColor.Gray;
                        }
                        else if (distanceToWall <= maxDepth)
                        {
                            fgColor = ConsoleColor.Gray;
                            bgColor = ConsoleColor.DarkGray;
                            shade = lightShade;
                        }
                        else
                        {
                            fgColor = ConsoleColor.DarkGray;
                            shade = emptyShade;
                        }

                        if (hitBoundary)
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

                        if (groundDistance <= 0.25f)
                        {
                            shade = fullShade; 
                            fgColor = ConsoleColor.Green;
                            bgColor = ConsoleColor.Green;
                            
                        }
                        else if (groundDistance <= 0.5f)
                        {
                            shade = darkShade;
                            fgColor = ConsoleColor.DarkGreen;
                            bgColor = ConsoleColor.Green;
                            
                        }
                        else if (groundDistance <= 0.75f)
                        {
                            fgColor = ConsoleColor.Black;
                            bgColor = ConsoleColor.DarkGreen;
                            shade = mediumShade;
                            
                        }
                        else if (groundDistance <= 0.9f)
                        {
                            fgColor = ConsoleColor.DarkGreen;
                            bgColor = ConsoleColor.Black;
                            shade = lightShade;
                        }
                        else
                        {
                            shade = emptyShade;
                            bgColor = ConsoleColor.DarkGreen;
                        }

                        Draw(x, y, shade, fgColor, bgColor);
                    }
                }
            }
            
            // Draw HUD
            DrawSprite(_map);
            Draw(_map.Position + _playerPosition.Rounded, 'X', ConsoleColor.Red);
            
            return !input.IsKeyDown(KeyCode.Esc);
        }
    }
}
