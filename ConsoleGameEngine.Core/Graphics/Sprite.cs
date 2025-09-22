using System;
using System.Linq;
using System.Text;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Graphics;

// TODO: Animated Sprite
/// <summary>
/// Sprites encapsulate a static graphical asset, storing image data and size information without any positional or rendering logic.
/// It serves as the visual building block that game entities use to draw themselves on screen.
/// </summary>
public class Sprite
{
    public const char SolidPixel = '\u2588';
          
    private readonly char[] _glyphs;
    private readonly Color24[] _fg;
    private readonly Color24[] _bg;

    public Vector Size { get; }
    public float Width => Size.X;
    public float Height => Size.Y;

    public SpriteRenderer GetRenderer() => new SpriteRenderer(this);
    
    public Sprite(int width, int height) : this(width, height, Color24.White, Color24.Black) { }
    public Sprite(int width, int height, Color24 fgColor) : this(width, height, fgColor, Color24.Black) { }
    public Sprite(int size, Color24 fgColor) : this(size, size, fgColor, Color24.Black) { }
    public Sprite(int size, Color24 fgColor, Color24 bgColor) : this(size, size, fgColor, bgColor) { }
    public Sprite(int width, int height, Color24 fgColor, Color24 bgColor)
    {
        Size = new(width, height);
        _glyphs = new char[width * height];
        
        for (int i = 0; i < _glyphs.Length; i++)
            _glyphs[i] = ' ';
        
        _fg = new Color24[_glyphs.Length];
        _bg = new Color24[_glyphs.Length];
        
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
        Size = new Vector(width, height);

        // Set up glyphs
        // Ensure rectangular dimensions by padding width where applicable.
        for (var i = 0; i < splitGfx.Length; i++)
        {
            if (splitGfx[i].Length != width)
            {
                splitGfx[i] = splitGfx[i].PadRight(width);
            }
        }
            
        // Map splitGfx into _glyphs
        _glyphs = new char[width * height];
        for (int i = 0; i < splitGfx.Length; i++)
        {
            for (int j = 0; j < splitGfx[i].Length; j++)
            {
                _glyphs[i * width + j] = splitGfx[i][j];
            }
        }
        
        _fg = new Color24[_glyphs.Length];
        _bg = new Color24[_glyphs.Length];
        
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
    public static Sprite Create(string spr, Color24 fgColor, Color24 bgColor) => new(spr, fgColor, bgColor);

    public Sprite(Sprite spr)
    {
        Size = spr.Size;
        
        _glyphs = new char[spr._glyphs.Length];
        _fg = new Color24[_glyphs.Length];
        _bg = new Color24[_glyphs.Length];
        
        for (int i = 0; i < spr.Height; i++)
        {
            for (int j = 0; j < spr.Width; j++)
            {
                _glyphs[i * (int)Size.X + j] = spr.GetGlyph(j, i);
                _fg[i * (int)Size.X + j] = spr.GetFgColor(j, i);
                _bg[i * (int)Size.X + j] = spr.GetBgColor(j, i);
            }
        }
    }
    
    public void SetSpriteColor(Color24 color)
    {
        for (int i = 0; i < _fg.Length; i++)
        {
            _fg[i] = color;
            if (_glyphs[i] == SolidPixel)
            {
                _bg[i] = color;
            }
        }
    }
    
    public void SetSpriteBackground(Color24 color)
    {
        for (int i = 0; i < _fg.Length; i++) 
            _bg[i] = color;
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
        if (index < 0 || index >= _fg.Length)
        {
            return Color24.Black;
        }

        return _fg[index];
    }
    
    public Color24 GetBgColor(int index)
    {
        if (index < 0 || index >= _bg.Length)
        {
            return Color24.Black;
        }

        return _bg[index];
    }
    
    public Color24 GetFgColor(Vector pos) => GetFgColor((int)pos.X, (int)pos.Y);
    public Color24 GetFgColor(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return Color24.Black;
        
        return _fg[y * (int)Width + x];
    }
    
    public Color24 GetBgColor(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return Color24.Black;
        
        return _bg[y * (int)Width + x];
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
            return;

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

        _fg[y * (int) Width + x] = c;
        
        if (GetGlyph(x,y) == SolidPixel)
        {
            _bg[y * (int) Width + x] = c;
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

        _bg[y * (int) Width + x] = c;
        
        if (GetGlyph(x,y) == SolidPixel)
        {
            _fg[y * (int) Width + x] = c;
        }
    }

    // Serialization support methods
    public char[] GetGlyphs() => (char[])_glyphs.Clone();
    public Color24[] GetForegroundColors() => (Color24[])_fg.Clone();
    public Color24[] GetBackgroundColors() => (Color24[])_bg.Clone();

    public static Sprite FromSerializationData(Vector size, char[] glyphs, Color24[] fg, Color24[] bg)
    {
        var sprite = new Sprite((int)size.X, (int)size.Y);
        Array.Copy(glyphs, sprite._glyphs, glyphs.Length);
        Array.Copy(fg, sprite._fg, fg.Length);
        Array.Copy(bg, sprite._bg, bg.Length);
        return sprite;
    }
}
