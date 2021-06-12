using System;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games
{
    using static System.Math;
    
    public class FirstPersonShooter : ConsoleGameEngineBase
    {
        protected override string Name => "First Person Shooter";

        private const float TURN_SPEED = 0.5f;
        private const float MOVE_SPEED = 2.0f;
        
        private Vector _playerPosition;
        private float _playerAngle;
        
        private Vector PlayerFacingAngle => new(
            (float) Sin(_playerAngle), 
            (float) Cos(_playerAngle));
        
        private int _mapHeight = 16;
        private int _mapWidth = 16;

        private float _fieldOfView = 3.14159f / 4f;
        private Sprite _map;

        public FirstPersonShooter()
        {
            InitConsole(420, 140, 4);
        }
        
        protected override bool Create()
        {
            var map = "";
            map += "#################\n";
            map += "#...............#\n";
            map += "#...............#\n";
            map += "#####...........#\n";
            map += "#...............#\n";
            map += "#...............#\n";
            map += "#...............#\n";
            map += "#...............#\n";
            map += "#...............#\n";
            map += "#...............#\n";
            map += "#.......#########\n";
            map += "#...............#\n";
            map += "#...#...........#\n";
            map += "#...#...........#\n";
            map += "#...#...........#\n";
            map += "#################\n";

            _map = new Sprite(map);
            _playerPosition = new Vector(8, 8);
            _playerAngle = 0f;
            return true;
        }

        protected override bool Update(float elapsedTime, KeyboardInput input)
        {
            // handle input

            if (input.IsKeyHeld(KeyCode.Left))
            {
                _playerAngle -= TURN_SPEED * elapsedTime;
            }
            
            if (input.IsKeyHeld(KeyCode.Right))
            {
                _playerAngle += TURN_SPEED * elapsedTime;
            }

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
                    distanceToWall += 0.1f; // Raycast step

                    var testPos = (_playerPosition + direction * distanceToWall).Rounded;

                    // Test position on map
                    if (testPos.X < 0 || testPos.X >= _mapWidth ||
                        testPos.Y < 0 || testPos.Y >= _mapHeight)
                    {
                        // Out of bounds
                        hitWall = true;
                        distanceToWall = maxDepth;
                    }
                    else
                    {
                        if (_map.GetGlyph((int) testPos.X, (int) testPos.Y) == '#')
                        {
                            hitWall = true;
                        }
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

                        Draw(x, y, shade, fgColor, bgColor);
                    }
                    else
                    {
                        float b = 1.0f - ((y - ScreenHeight / 2.0f) / (ScreenHeight / 2.0f));
                        
                        char emptyShade = '#';
                        char lightShade = 'X';
                        char mediumShade = '.';
                        char darkShade = '-';
                        char fullShade = ' ';

                        char shade;
                        var fgColor = ConsoleColor.Green;
                        ConsoleColor bgColor;

                        if (b <= 0.25f)
                        {
                            shade = fullShade; 
                            fgColor = ConsoleColor.Green;
                            bgColor = ConsoleColor.Green;
                            
                        }
                        else if (b <= 0.5f)
                        {
                            shade = darkShade;
                            fgColor = ConsoleColor.DarkGreen;
                            bgColor = ConsoleColor.Green;
                            
                        }
                        else if (b <= 0.75f)
                        {
                            fgColor = ConsoleColor.Black;
                            bgColor = ConsoleColor.DarkGreen;
                            shade = mediumShade;
                            
                        }
                        else if (b <= 0.9f)
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
            
            
            
            
            return !input.IsKeyDown(KeyCode.Esc);
        }
    }
}
