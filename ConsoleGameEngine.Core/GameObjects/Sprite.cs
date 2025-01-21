using System;
using System.Linq;
using System.Text;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects;

// TODO: Animated Sprite
public class Sprite
{
    public const char SolidPixel = '\u2588';
          
    private readonly char[] _glyphs;
    private readonly Color24[] _fgColors;
    private readonly Color24[] _bgColors;

    public Vector Size { get; }

    public float Width => Size.X;
    public float Height => Size.Y;
    
    public Rect Bounds => new(Vector.Zero, Size);
    
    public Sprite(int width, int height) : this(width, height, Color24.White, Color24.Black) { }
    public Sprite(int width, int height, Color24 fgColor) : this(width, height, fgColor, Color24.Black) { }
    public Sprite(int size, Color24 fgColor, Color24 bgColor) : this(size, size, fgColor, bgColor) { }
    public Sprite(int width, int height, Color24 fgColor, Color24 bgColor)
    {
        Size = new(width, height);
        _glyphs = new char[width * height];
        
        for (int i = 0; i < _glyphs.Length; i++)
            _glyphs[i] = ' ';
        
        _fgColors = new Color24[_glyphs.Length];
        _bgColors = new Color24[_glyphs.Length];
        
        SetSpriteBackground(bgColor);
        SetSpriteColor(fgColor);
    }

    public Sprite(string gfx) : this(gfx, Color24.White, Color24.Black) { }
    public Sprite(string gfx, Color24 fgColor) : this(gfx, fgColor, Color24.Black) { }
    public Sprite(string gfx, Color24 fgColor, Color24 bgColor)
    {
        var splitGfx = gfx.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var width = splitGfx.Max(c => c.Length);
        var height = splitGfx.Length;

        // Ensure rectangular dimensions by padding width where applicable.
        for (var i = 0; i < splitGfx.Length; i++)
        {
            if (splitGfx[i].Length != width)
            {
                splitGfx[i] = splitGfx[i].PadRight(width);
            }
        }

        Size = new Vector(width, height);
                    
        _glyphs = new char[width * height];
        
        for (int i = 0; i < splitGfx.Length; i++)
        {
            for (int j = 0; j < splitGfx[i].Length; j++)
            {
                _glyphs[i * width + j] = splitGfx[i][j];
            }
        }
        
        _fgColors = new Color24[_glyphs.Length];
        _bgColors = new Color24[_glyphs.Length];
        
        SetSpriteBackground(bgColor);
        SetSpriteColor(fgColor);
    }

    public static Sprite CreateSolid(int width, int height, Color24 color)
    {
        var sb = new StringBuilder();
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                sb.Append(SolidPixel);
            }

            sb.Append('\n');
        }

        return new Sprite(sb.ToString(), color);
    }

    public static Sprite Create(string spr) => Create(spr, Color24.White, Color24.Black);
    public static Sprite Create(string spr, Color24 fgColor) => Create(spr, fgColor, Color24.Black);
    public static Sprite Create(string spr, Color24 fgColor, Color24 bgColor)
    {
        return new Sprite(spr, fgColor, bgColor);
    }
    
    public Sprite(Sprite spr)
    {
        Size = spr.Size;
        
        _glyphs = new char[spr._glyphs.Length];
        _fgColors = new Color24[_glyphs.Length];
        _bgColors = new Color24[_glyphs.Length];
        
        for (int i = 0; i < spr.Height; i++)
        {
            for (int j = 0; j < spr.Width; j++)
            {
                _glyphs[i * (int)Size.X + j] = spr.GetGlyph(j, i);
                _fgColors[i * (int)Size.X + j] = spr.GetFgColor(j, i);
                _bgColors[i * (int)Size.X + j] = spr.GetBgColor(j, i);
            }
        }
    }
    
    public void SetSpriteColor(Color24 color)
    {
        for (int i = 0; i < _fgColors.Length; i++)
        {
            _fgColors[i] = color;
            if (_glyphs[i] == SolidPixel)
            {
                _bgColors[i] = color;
            }
        }
    }
    
    public void SetSpriteBackground(Color24 color)
    {
        for (int i = 0; i < _fgColors.Length; i++) 
            _bgColors[i] = color;
    }

    private char GetGlyph(Vector pos)
    {
        return GetGlyph((int) pos.X, (int) pos.Y);
    }
    
    private char GetGlyph(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return ' ';
        
        return _glyphs[y * (int)Width + x];
    }

    private char GetGlyph(int index)
    {
        if (index < 0 || index >= _glyphs.Length)
        {
            return ' ';
        }

        return _glyphs[index];
    }

    public char this[int i]
    {
        get => GetGlyph(i);
        set => SetGlyph(i, value);
    }

    public char this[int x, int y]
    {
        get => GetGlyph(x, y);
        set => SetGlyph(x, y, value);
    }

    public char this[Vector pos]
    {
        get => GetGlyph(pos);
        set => SetGlyph(pos, value);
    }

    public Color24 GetFgColor(int index)
    {
        if (index < 0 || index >= _fgColors.Length)
        {
            return Color24.Black;
        }

        return _fgColors[index];
    }
    
    public Color24 GetBgColor(int index)
    {
        if (index < 0 || index >= _bgColors.Length)
        {
            return Color24.Black;
        }

        return _bgColors[index];
    }
    
    public Color24 GetFgColor(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return Color24.Black;
        
        return _fgColors[y * (int)Width + x];
    }
    
    public Color24 GetBgColor(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return Color24.Black;
        
        return _bgColors[y * (int)Width + x];
    }

    private void SetGlyph(int index, char c)
    {
        if (index >= 0 && index < _glyphs.Length)
        {
            _glyphs[index] = c;
        }
    }
    private void SetGlyph(Vector pos, char c)
    {
        SetGlyph((int)pos.X, (int)pos.Y, c);
    }

    private void SetGlyph(int x, int y, char c)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        _glyphs[y * (int) Width + x] = c;
    }

    public void SetFgColor(Vector pos, Color24 c)
    {
        SetFgColor((int)pos.X, (int)pos.Y, c);
    }
    
    public void SetFgColor(int x, int y, Color24 c)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        _fgColors[y * (int) Width + x] = c;
        
        if (GetGlyph(x,y) == SolidPixel)
        {
            _bgColors[y * (int) Width + x] = c;
        }
    }

    public void SetBgColor(Vector pos, Color24 c)
    {
        SetBgColor((int)pos.X, (int)pos.Y, c);
    }
    
    public void SetBgColor(int x, int y, Color24 c)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        _bgColors[y * (int) Width + x] = c;
        
        if (GetGlyph(x,y) == SolidPixel)
        {
            _fgColors[y * (int) Width + x] = c;
        }
    }
}
