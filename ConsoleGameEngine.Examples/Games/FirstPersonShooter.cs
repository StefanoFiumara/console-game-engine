using System;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Physics;

namespace ConsoleGameEngine.Runner.Games;

using static Math;

// ReSharper disable once UnusedType.Global
public class FirstPersonShooter() : ConsoleGame(width: 320, height: 240, pixelSize: 2, targetFps: 120)
{
    private const float TurnSpeed = 1.2f;
    private const float MoveSpeed = 3.0f;
    private const float BoundaryTolerance = 0.005f;
    private const float FieldOfView = 3.14159f / 4f; // 90 degree fov
        
    private Vector _playerPosition;
    private float _playerAngle;

    private Vector _miniMapPosition;
    private Color24[] _skyGradient;
    private Color24[] _grassGradient;
    private Color24[] _wallGradient;
    
    private Vector PlayerFacingAngle => new(
        (float) Sin(_playerAngle),
        (float) Cos(_playerAngle));

    private Sprite _map;
    

    protected override bool Create(IRenderer renderer)
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
        map += "#........#######\n";
        map += "#..............#\n";
        map += "#....#.........#\n";
        map += "#....#.........#\n";
        map += "#....#.........#\n";
        map += "################\n";

        _map = new Sprite(map);
        _miniMapPosition = new Vector(renderer.Width - _map.Width, renderer.Height - _map.Height);
        _playerPosition = new Vector(8, 8);
        _playerAngle = 0f;
        
        _skyGradient = Color24.CreateGradient((renderer.Height / 2), Color24.Cyan, Color24.DarkCyan);
        _grassGradient = Color24.CreateGradient((renderer.Height / 2), new Color24(100, 20, 0), new Color24(223,161, 0));
        _wallGradient = Color24.CreateGradient((renderer.Width / 2), Color24.White, Color24.Black);
        return true;
    }

    protected override bool Update(float elapsedTime, IRenderer renderer, PlayerInput input)
    {
        // handle input
        if (input.IsKeyHeld(KeyCode.Left))  _playerAngle -= TurnSpeed * elapsedTime;
        if (input.IsKeyHeld(KeyCode.Right)) _playerAngle += TurnSpeed * elapsedTime;
            
        if (input.IsKeyHeld(KeyCode.Up))
        {
            _playerPosition += PlayerFacingAngle * MoveSpeed * elapsedTime;

            if (_map[(int) _playerPosition.Rounded.X, (int) _playerPosition.Rounded.Y] == '#')
            {
                _playerPosition -= PlayerFacingAngle * MoveSpeed * elapsedTime;
            }
        }

        if (input.IsKeyHeld(KeyCode.Down))
        {
            _playerPosition -= PlayerFacingAngle * MoveSpeed * elapsedTime;
            if (_map[(int) _playerPosition.Rounded.X, (int) _playerPosition.Rounded.Y] == '#')
            {
                _playerPosition += PlayerFacingAngle * MoveSpeed * elapsedTime;
            }
        }
            
        // Basic raycast algorithm for each column on the screen
        for (int x = 0; x < renderer.Width; x++)
        {
            // Create ray vector
            double rayAngle = (_playerAngle - FieldOfView / 2.0d) + ((x / (double) renderer.Width) * FieldOfView);
            var direction = new Vector((float) Sin(rayAngle), (float) Cos(rayAngle));

            var ray = Raycast.Send(_map, _playerPosition, direction, '#', BoundaryTolerance);

            // Use distance to wall to determine ceiling and floor height for this column
            // From the midpoint (height / 2), subtract an amount proportional to the distance of the wall
            int ceiling = (int) (renderer.Height / 2f - renderer.Height / ray.Distance);
            // Floor is mirror of ceiling
            int floor = renderer.Height - ceiling;

            // Render column based on ceiling and floor values
            for (int y = 0; y < renderer.Height; y++)
            {
                if (y < ceiling)
                {
                    renderer.Draw(x, y, Sprite.SolidPixel, _skyGradient[y]);
                }
                else if (y >= ceiling && y <= floor)
                {
                    int wallIndex = (int)((_wallGradient.Length - 1) * ray.Distance / Raycast.MaxRaycastDepth);
                    renderer.Draw(x, y, Sprite.SolidPixel, _wallGradient[wallIndex]);
                }
                else
                {
                    renderer.Draw(x, y, Sprite.SolidPixel, _grassGradient[y - floor]);
                }
            }
        }

        // Draw HUD
        renderer.DrawSprite(_map, _miniMapPosition);
        renderer.DrawString(renderer.Width, (int)_miniMapPosition.Y - 1, $"Boundary Tol: {BoundaryTolerance}", alignment: TextAlignment.Right);
        renderer.Draw(_miniMapPosition + _playerPosition.Rounded, 'X', Color24.Red, Color24.Black);

        return !input.IsKeyDown(KeyCode.Esc);
    }
}