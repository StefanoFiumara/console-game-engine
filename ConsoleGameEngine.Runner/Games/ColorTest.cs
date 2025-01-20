using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games;

// ReSharper disable once UnusedType.Global
public class ColorTest() : ConsoleGame(new ConsoleRenderer(128, 128, 2, enable24BitColorMode: true))
{
    protected override bool Create(IRenderer renderer)
    {
        var gradient = GenerateGradient(renderer.ScreenWidth, renderer.ScreenHeight);

        for (var i = 0; i < gradient.Length; i++)
        {
            var color = gradient[i];
            renderer.Draw(i % renderer.ScreenWidth, i / renderer.ScreenWidth, color.Char, color.Foreground, color.Background);
        }

        return true;
    }

    public CharInfo24[] GenerateGradient(int width, int height)
    {
        var buffer = new CharInfo24[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float hue = (float)x / width;
                var color = Color24.FromHsv(hue * 360, 1f, 1 - (float)y / (height - 1));

                buffer[y * width + x] = new CharInfo24
                {
                    Char = '*',
                    Foreground = color,
                    Background = Color24.Black
                };
            }
        }

        return buffer;
    }

    protected override bool Update(float elapsedTime, IRenderer renderer, PlayerInput input)
    {
        if (input.IsKeyDown(KeyCode.Esc)) return false;

        return true;
    }
}