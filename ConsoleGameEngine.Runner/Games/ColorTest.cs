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

        // Loop through each cell in the buffer
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate a color based on position
                float hue = (float)x / width; // Range from 0.0 to 1.0
                var color = HsvToRgb(hue * 360, 1 - (float) y / height, 1 - (float)y / height); // Full saturation and brightness

                // Set the character and colors
                buffer[y * width + x] = new CharInfo24
                {
                    Char = ' ', // Full block for better color visibility
                    Foreground = color,
                    Background = color // Black background for contrast
                };
            }
        }

        return buffer;
    }

    private static Color24 HsvToRgb(float hue, float saturation, float value)
    {
        int hi = (int)(hue / 60) % 6;
        float f = (hue / 60) - hi;

        float p = value * (1 - saturation);
        float q = value * (1 - f * saturation);
        float t = value * (1 - (1 - f) * saturation);

        float r = 0, g = 0, b = 0;
        switch (hi)
        {
            case 0: r = value; g = t; b = p; break;
            case 1: r = q; g = value; b = p; break;
            case 2: r = p; g = value; b = t; break;
            case 3: r = p; g = q; b = value; break;
            case 4: r = t; g = p; b = value; break;
            case 5: r = value; g = p; b = q; break;
        }

        return new Color24((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }
    
    protected override bool Update(float elapsedTime, IRenderer renderer, PlayerInput input)
    {
        if (input.IsKeyDown(KeyCode.Esc)) return false;

        return true;
    }
}