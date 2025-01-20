using System;
using System.Collections.Generic;
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
public class FirstPersonShooter() : ConsoleGame(new ConsoleRenderer(width: 240, height: 135, pixelSize: 6, enable24BitColorMode: true), targetFps: 120)
{
    private const float TurnSpeed = 1.2f;
    private const float MoveSpeed = 3.0f;
    private const float BoundaryTolerance = 0.005f;
    private const float FieldOfView = 3.14159f / 4f; // 90 degree fov
        
    private Vector _playerPosition;
    private float _playerAngle;

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
        _miniMapPosition = new Vector(renderer.ScreenWidth - _map.Width, renderer.ScreenHeight - _map.Height);
        _playerPosition = new Vector(8, 8);
        _playerAngle = 0f;
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
        for (int x = 0; x < renderer.ScreenWidth; x++)
        {
            // Create ray vector
            double rayAngle = (_playerAngle - FieldOfView / 2.0d) + ((x / (double) renderer.ScreenWidth) * FieldOfView);
            var direction = new Vector((float) Sin(rayAngle), (float) Cos(rayAngle));

            var ray = Raycast.Send(_map, _playerPosition, direction, '#', BoundaryTolerance);

            // Use distance to wall to determine ceiling and floor height for this column
            // From the midpoint (height / 2), subtract an amount proportional to the distance of the wall
            int ceiling = (int) (renderer.ScreenHeight / 2f - renderer.ScreenHeight / ray.Distance);
            // Floor is mirror of ceiling
            int floor = renderer.ScreenHeight - ceiling;

            // Render column based on ceiling and floor values
            for (int y = 0; y < renderer.ScreenHeight; y++)
            {
                if (y < ceiling)
                {
                    renderer.Draw(x, y, ' ', Color24.White, Color24.Cyan);
                }
                else if (y >= ceiling && y <= floor)
                {
                    var shade = ray.HitBoundary ? Shade.Black : CalculateShade(WallShades, ray.Distance);
                    renderer.Draw(x, y, shade.Character, shade.ForegroundColor, shade.BackgroundColor);
                }
                else
                {
                    var groundDistance = 1.0f - (y - renderer.ScreenHeight / 2.0f) / (renderer.ScreenHeight / 2.0f);
                        
                    var shade = CalculateShade(GroundShades, groundDistance);
                    renderer.Draw(x, y, shade.Character, shade.ForegroundColor, shade.BackgroundColor);
                }
            }
        }

        // Draw HUD
        renderer.DrawSprite(_map, _miniMapPosition);
        renderer.DrawString(renderer.ScreenWidth, (int)_miniMapPosition.Y - 1, $"Boundary Tol: {BoundaryTolerance}", alignment: TextAlignment.Right);
        renderer.Draw(_miniMapPosition + _playerPosition.Rounded, 'X', Color24.Red, Color24.Black);

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
        new(Raycast.MaxRaycastDepth / 4f, Shade.MediumShade, Color24.Gray,     Color24.White),
        new(Raycast.MaxRaycastDepth / 3f, Shade.DarkShade,   Color24.Gray,     Color24.White),
        new(Raycast.MaxRaycastDepth / 2f, Shade.LightShade,  Color24.DarkGray, Color24.Gray),
        new(Raycast.MaxRaycastDepth / 1f, Shade.MediumShade, Color24.White, Color24.DarkGray)
    };
        
    private static readonly List<Shade> GroundShades = new()
    {
        new(0.25f, Shade.FullShade,   Color24.Green,     Color24.Green),
        new(0.5f,  Shade.DarkShade,   Color24.DarkGreen, Color24.Green),
        new(0.75f, Shade.MediumShade, Color24.Black,     Color24.DarkGreen),
        new(0.9f,  Shade.LightShade,  Color24.DarkGreen, Color24.Black)
    };

    private Vector _miniMapPosition;

    private class Shade
    {
        public static readonly Shade Default = new(0, '#', Color24.Magenta, Color24.DarkMagenta);
        public static readonly Shade Black = new(0, ' ', Color24.Black, Color24.Black);
            
        public const char FullShade = ' ';
        public const char DarkShade = '-';
        public const char MediumShade = '.';
        public const char LightShade = 'X';

        public float DistanceThreshold { get; }
        public char Character { get; }
        public Color24 ForegroundColor { get; }
        public Color24 BackgroundColor { get; }

        public Shade(float distanceThreshold, char character, Color24 foregroundColor, Color24 backgroundColor)
        {
            DistanceThreshold = distanceThreshold;
            Character = character;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }
    }
}