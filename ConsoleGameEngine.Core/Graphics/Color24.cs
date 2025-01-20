using System;
using System.Collections.Generic;

namespace ConsoleGameEngine.Core.Graphics;

public readonly record struct Color24(byte R, byte G, byte B)
{
    // Predefined color constants that match ConsoleColor values
    public static readonly Color24 Black       = new(0, 0, 0);
    public static readonly Color24 DarkBlue    = new(0, 0, 128);
    public static readonly Color24 DarkGreen   = new(0, 128, 0);
    public static readonly Color24 DarkCyan    = new(0, 128, 128);
    public static readonly Color24 DarkRed     = new(128, 0, 0);
    public static readonly Color24 DarkMagenta = new(128, 0, 128);
    public static readonly Color24 DarkYellow  = new(128, 128, 0);
    public static readonly Color24 Gray        = new(192, 192, 192);
    public static readonly Color24 DarkGray    = new(128, 128, 128);
    public static readonly Color24 Blue        = new(0, 0, 255);
    public static readonly Color24 Green       = new(0, 255, 0);
    public static readonly Color24 Cyan        = new(0, 255, 255);
    public static readonly Color24 Red         = new (255, 0, 0);
    public static readonly Color24 Magenta     = new(255, 0, 255);
    public static readonly Color24 Yellow      = new(255, 255, 0);
    public static readonly Color24 White       = new(255, 255, 255);
    
    private static readonly Dictionary<ConsoleColor, Color24> ConsoleColorToColor24 = new()
    {
        { ConsoleColor.Black, Black },
        { ConsoleColor.DarkBlue, DarkBlue },
        { ConsoleColor.DarkGreen, DarkGreen },
        { ConsoleColor.DarkCyan, DarkCyan },
        { ConsoleColor.DarkRed, DarkRed },
        { ConsoleColor.DarkMagenta, DarkMagenta },
        { ConsoleColor.DarkYellow, DarkYellow },
        { ConsoleColor.Gray, Gray },
        { ConsoleColor.DarkGray, DarkGray },
        { ConsoleColor.Blue, Blue },
        { ConsoleColor.Green, Green },
        { ConsoleColor.Cyan, Cyan },
        { ConsoleColor.Red, Red },
        { ConsoleColor.Magenta, Magenta },
        { ConsoleColor.Yellow, Yellow },
        { ConsoleColor.White, White }
    };
    
    private static readonly Dictionary<Color24, ConsoleColor> Color24ToConsoleColor = new()
    {
        { Black, ConsoleColor.Black },
        { DarkBlue, ConsoleColor.DarkBlue },
        { DarkGreen, ConsoleColor.DarkGreen },
        { DarkCyan, ConsoleColor.DarkCyan },
        { DarkRed, ConsoleColor.DarkRed },
        { DarkMagenta, ConsoleColor.DarkMagenta },
        { DarkYellow, ConsoleColor.DarkYellow },
        { Gray, ConsoleColor.Gray },
        { DarkGray, ConsoleColor.DarkGray },
        { Blue, ConsoleColor.Blue },
        { Green, ConsoleColor.Green },
        { Cyan, ConsoleColor.Cyan },
        { Red, ConsoleColor.Red },
        { Magenta, ConsoleColor.Magenta },
        { Yellow, ConsoleColor.Yellow },
        { White, ConsoleColor.White }
    };
    
    private static readonly Dictionary<Color24, ConsoleColor> ColorMappingCache = new();
    
    public static implicit operator Color24(ConsoleColor color) => ConsoleColorToColor24.GetValueOrDefault(color);
    public static implicit operator ConsoleColor(Color24 color)
    {
        if(Color24ToConsoleColor.TryGetValue(color, out var consoleColor)) return consoleColor;
        if(ColorMappingCache.TryGetValue(color, out consoleColor)) return consoleColor;
        
        // Calculate and map to the closest ConsoleColor
        var minDistance = double.MaxValue;
        var closestColor = ConsoleColor.Black;
        
        foreach (var kvp in ConsoleColorToColor24)
        {
            var distance = CalculateColorDistance(color, kvp.Value);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = kvp.Key;
            }
        }
        
        ColorMappingCache.Add(color, closestColor);
        return closestColor;
    }
    
    public static Color24 FromHsv(float hue, float saturation, float value)
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
    
    private static double CalculateColorDistance(Color24 color1, Color24 color2)
    {
        int rDiff = color1.R - color2.R;
        int gDiff = color1.G - color2.G;
        int bDiff = color1.B - color2.B;
        return System.Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
    }

    public override string ToString() => $"RGB({R}, {G}, {B})";
}