using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Graphics;

public enum TextAlignment
{
    Left,
    Centered,
    Right
}

public static class RendererExtensions
{
    public static void Fill(this IRenderer renderer, char c) => Fill(renderer, renderer.Bounds, c, Color24.White, Color24.Black);
    public static void Fill(this IRenderer renderer, Rect rect, char c) => Fill(renderer, rect, c, Color24.White, Color24.Black);
    public static void Fill(this IRenderer renderer, Rect rect, char c, Color24 fgColor) => Fill(renderer, rect.Position, rect.Size, c, fgColor, Color24.Black);
    public static void Fill(this IRenderer renderer, Rect rect, char c, Color24 fgColor, Color24 bgColor) => Fill(renderer, rect.Position, rect.Size, c, fgColor, bgColor);
    public static void Fill(this IRenderer renderer, Vector position, Vector size, char c) => Fill(renderer, position, size, c, Color24.White, Color24.Black);
    public static void Fill(this IRenderer renderer, Vector position, Vector size, char c, Color24 fgColor) => Fill(renderer, position, size, c, fgColor, Color24.Black);
    public static void Fill(this IRenderer renderer, Vector position, Vector size, char c, Color24 fgColor, Color24 bgColor) => Fill(renderer, (int)position.X, (int)position.Y, (int)(size.X + position.X), (int)(size.Y + position.Y), c, fgColor, bgColor);
    public static void Fill(this IRenderer renderer, int x1, int y1, int x2, int y2, char c, Color24 fgColor, Color24 bgColor)
    {
        Clip(ref x1, ref y1, renderer.Width, renderer.Height);
        Clip(ref x2, ref y2, renderer.Width, renderer.Height);

        for (int y = y1; y < y2; y++)
        {
            for (int x = x1; x < x2; x++)
            {
                renderer.Draw(x, y, c, fgColor, bgColor);
            }
        }
    }
    
    public static void DrawBox(this IRenderer renderer, Rect rect, char c) => DrawBox(renderer, rect, c, Color24.White, Color24.Black);
    public static void DrawBox(this IRenderer renderer, Rect rect, char c, Color24 fgColor) => DrawBox(renderer, rect, c, fgColor, Color24.Black);
    public static void DrawBox(this IRenderer renderer, Rect rect, char c, Color24 fgColor, Color24 bgColor) => DrawBox(renderer, rect.Position, rect.Size, c, fgColor, bgColor);
    public static void DrawBox(this IRenderer renderer, Vector position, Vector size, char c, Color24 fgColor) => DrawBox(renderer, position, size, c, fgColor, Color24.Black);
    public static void DrawBox(this IRenderer renderer, Vector position, Vector size, char c) => DrawBox(renderer, position, size, c, Color24.White, Color24.Black);
    public static void DrawBox(this IRenderer renderer, Vector position, Vector size, char c, Color24 fgColor, Color24 bgColor)
    {
        var topLeft = position;
        var topRight = position + Vector.Right * size.X;
        var bottomLeft = position + Vector.Down * size.Y;
        var bottomRight = position + size;
            
        renderer.DrawLine(topLeft, topRight, c, fgColor, bgColor);
        renderer.DrawLine(bottomLeft, bottomRight, c, fgColor, bgColor);
        renderer.DrawLine(topLeft, bottomLeft, c, fgColor, bgColor);
        renderer.DrawLine(topRight, bottomRight, c, fgColor, bgColor);
    }
    
    public static void DrawBorder(this IRenderer renderer, Rect rect) => DrawBorder(renderer, rect, Color24.White, Color24.Black);
    public static void DrawBorder(this IRenderer renderer, Rect rect, Color24 fgColor) => DrawBorder(renderer, rect, fgColor, Color24.Black);
    public static void DrawBorder(this IRenderer renderer, Rect rect, Color24 fgColor, Color24 bgColor)
    {
        var position = new Vector(rect.Position.X - 1, rect.Position.Y - 1);
        var size = new Vector(rect.Width + 1, rect.Height + 1);
            
        var topLeft = position;
        var topRight = position + Vector.Right * size.X;
        var bottomLeft = position + Vector.Down * size.Y;
        var bottomRight = position + size;
        
        renderer.DrawLine(topLeft, topRight, '-', fgColor, bgColor);
        renderer.DrawLine(bottomLeft, bottomRight, '-', fgColor, bgColor);
        renderer.DrawLine(topLeft, bottomLeft, '|', fgColor, bgColor);
        renderer.DrawLine(topRight, bottomRight, '|', fgColor, bgColor);
        
        renderer.Draw(topLeft, '+', fgColor, bgColor);
        renderer.Draw(topRight, '+', fgColor, bgColor);
        renderer.Draw(bottomLeft, '+', fgColor, bgColor);
        renderer.Draw(bottomRight, '+', fgColor, bgColor);
    }
    
    public static void DrawLine(this IRenderer renderer, Vector start, Vector end, char c) => DrawLine(renderer, start, end, c, Color24.White, Color24.Black);
    public static void DrawLine(this IRenderer renderer, Vector start, Vector end, char c, Color24 fgColor) => DrawLine(renderer, start, end, c, fgColor, Color24.Black);
    public static void DrawLine(this IRenderer renderer, Vector start, Vector end, char c, Color24 fgColor, Color24 bgColor) => DrawLine(renderer, (int) start.X, (int) start.Y, (int) end.X, (int) end.Y, c, fgColor, bgColor);
    public static void DrawLine(this IRenderer renderer, int x1, int y1, int x2, int y2, char c, Color24 fgColor, Color24 bgColor)
    {
        int x;
        int y;

        var dx = x2 - x1;
        var dy = y2 - y1;

        var dx1 = System.Math.Abs(dx);
        var dy1 = System.Math.Abs(dy);

        var px = 2 * dy1 - dx1;
        var py = 2 * dx1 - dy1;

        if (dy1 <= dx1)
        {
            int xe;
            if (dx >= 0)
            {
                x = x1;
                y = y1;
                xe = x2;
            }
            else
            {
                x = x2;
                y = y2;
                xe = x1;
            }

            renderer.Draw(x, y, c, fgColor, bgColor);

            while(x < xe)
            {
                x += 1;
                if (px < 0)
                {
                    px += 2 * dy1;
                }
                else
                {
                    if (dx < 0 && dy < 0 || dx > 0 && dy > 0)
                    {
                        y += 1;
                    }
                    else
                    {
                        y -= 1;
                    }
                    px += 2 * (dy1 - dx1);
                }

                renderer.Draw(x, y, c, fgColor, bgColor);
            }
        }
        else
        {
            int ye;
            if (dy >= 0)
            {
                x = x1;
                y = y1;
                ye = y2;
            }
            else
            {
                x = x2;
                y = y2;
                ye = y1;
            }

            renderer.Draw(x, y, c, fgColor, bgColor);

            while(y < ye)
            {
                y += 1;
                if (py <= 0)
                {
                    py += 2 * dx1;
                }
                else
                {
                    if (dx < 0 && dy < 0 || dx > 0 && dy > 0)
                    {
                        x += 1;
                    }
                    else
                    {
                        x -= 1;
                    }
                    py += 2 * (dx1 - dy1);
                }

                renderer.Draw(x, y, c, fgColor, bgColor);
            }
        }
    }
    
    public static void DrawString(this IRenderer renderer, Vector position, string msg, TextAlignment alignment = TextAlignment.Left) => DrawString(renderer, position, msg, Color24.White, Color24.Black, alignment);
    public static void DrawString(this IRenderer renderer, Vector position, string msg, Color24 fgColor, TextAlignment alignment = TextAlignment.Left) => DrawString(renderer, (int) position.X, (int) position.Y, msg, fgColor, Color24.Black, alignment);
    public static void DrawString(this IRenderer renderer, Vector position, string msg, Color24 fgColor, Color24 bgColor, TextAlignment alignment = TextAlignment.Left) => DrawString(renderer, (int) position.X, (int) position.Y, msg, fgColor, bgColor, alignment);
    public static void DrawString(this IRenderer renderer, int x, int y, string msg, TextAlignment alignment = TextAlignment.Left) => DrawString(renderer, x,y, msg, Color24.White, Color24.Black, alignment);
    public static void DrawString(this IRenderer renderer, int x, int y, string msg, Color24 fgColor, TextAlignment alignment = TextAlignment.Left) => DrawString(renderer, x, y, msg, fgColor, Color24.Black, alignment);
    public static void DrawString(this IRenderer renderer, int x, int y, string msg, Color24 fgColor, Color24 bgColor, TextAlignment alignment = TextAlignment.Left)
    {
        if (alignment == TextAlignment.Centered)
            x -= msg.Length / 2;
        else if (alignment == TextAlignment.Right) 
            x -= msg.Length;

        // TODO: handle multi-line strings?
        for (int i = 0; i < msg.Length; i++)
        {
            renderer.Draw(x + i, y, msg[i], fgColor, bgColor);
        }
    }
    
    public static void DrawSprite(this IRenderer renderer, Sprite sprite, Vector position)
    {
        for (var y = 0; y < sprite.Height; y++)
        {
            for (int x = 0; x < sprite.Width; x++)
            {
                if (sprite[x, y] != ' ')
                {
                    renderer.Draw(
                        (int)position.X + x,
                        (int)position.Y + y,
                        sprite[x, y],
                        sprite.GetFgColor(x, y),
                        sprite.GetBgColor(x, y));
                }
            }
        }
    }

    public static void DrawObject(this IRenderer renderer, GameObject obj)
    {
        renderer.DrawSprite(obj.Sprite, obj.Position);
    }
    
    private static void Clip(ref int x, ref int y, int width, int height)
    {
        if (x < 0) x = 0;
        if (x >= width) x = width;
        if (y < 0) y = 0;
        if (y >= height) y = height;
    }
}