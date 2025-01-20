using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games;

// ReSharper disable once UnusedType.Global
public class ColorTest() : ConsoleGame(new ConsoleRenderer(50, 51, 8, enable24BitColorMode: true))
{
    private const int GradientSize = 50;
    private Color24[] _gradient;
    
    protected override bool Create(IRenderer renderer)
    {
        _gradient = GenerateGradient(GradientSize);
        return true;
    }

    protected override bool Update(float elapsedTime, IRenderer renderer, PlayerInput input)
    {
        if (input.IsKeyDown(KeyCode.Esc)) return false;
        
        renderer.Fill(' ');
        renderer.DrawString(0,0, $"{input.MousePosition}");
        
        var coord = (input.MousePosition with { Y = input.MousePosition.Y - 1 }).Rounded;

        if (coord.X >= 0 && coord.X < renderer.ScreenWidth && coord.Y >= 0 && coord.Y < renderer.ScreenHeight - 1)
        {
            var hoverIndex = (int)(coord.Y * GradientSize + coord.X);
            var color = _gradient[hoverIndex];
            renderer.DrawString(renderer.ScreenWidth,0, $"{color}", TextAlignment.Right);
        }
        
        for (var i = 0; i < _gradient.Length; i++)
        {
            renderer.Draw(i % GradientSize, 1 + i / GradientSize, ' ', _gradient[i], _gradient[i]);
        }
        
        return true;
    }

    private Color24[] GenerateGradient(int size)
    {
        var result = new Color24[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float hue = (float)x / size;
                result[y * size + x] = Color24.FromHsv(hue * 360, 1f, 1 - (float)y / (size - 1));
            }
        }

        return result;
    }
}