using System;
using System.Linq;
using ConsoleGameEngine.Core.Math;
// ReSharper disable MemberCanBePrivate.Global

namespace ConsoleGameEngine.Core.GameObjects
{
    public class Sprite
    {
        private readonly char[] _glyphs;
        private readonly ConsoleColor[] _fgColors;
        private readonly ConsoleColor[] _bgColors;

        public Vector Position { get; set; }
        public Vector Size { get; }
        public Vector Velocity { get; set; }

        public float Width => Size.X;
        public float Height => Size.Y;
        
        public Rect Bounds => new(Position, Size);

        public Sprite(string gfx, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
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
            
            _fgColors = new ConsoleColor[_glyphs.Length];
            _bgColors = new ConsoleColor[_glyphs.Length];
            
            SetSpriteColor(fgColor);
            SetSpriteBackground(bgColor);
        }

        public Sprite(Sprite spr)
        {
            Size = spr.Size;
            Position = spr.Position;
            Velocity = spr.Velocity;
            
            _glyphs = new char[spr._glyphs.Length];
            _fgColors = new ConsoleColor[_glyphs.Length];
            _bgColors = new ConsoleColor[_glyphs.Length];
            
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
        
        public void SetSpriteColor(ConsoleColor color)
        {
            for (int i = 0; i < _fgColors.Length; i++)
            {
                _fgColors[i] = color;
            }
        }
        
        public void SetSpriteBackground(ConsoleColor color)
        {
            for (int i = 0; i < _fgColors.Length; i++)
            {
                _bgColors[i] = color;
            }
        }

        public char GetGlyph(Vector pos)
        {
            return GetGlyph((int) pos.X, (int) pos.Y);
        }
        
        public char GetGlyph(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return ' ';
            
            return _glyphs[y * (int)Width + x];
        }

        public char GetGlyph(int index)
        {
            if (index < 0 || index >= _glyphs.Length)
            {
                return ' ';
            }

            return _glyphs[index];
        }

        public ConsoleColor GetFgColor(int index)
        {
            if (index < 0 || index >= _fgColors.Length)
            {
                return ConsoleColor.Black;
            }

            return _fgColors[index];
        }
        
        public ConsoleColor GetBgColor(int index)
        {
            if (index < 0 || index >= _bgColors.Length)
            {
                return ConsoleColor.Black;
            }

            return _bgColors[index];
        }
        
        public ConsoleColor GetFgColor(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return ConsoleColor.Black;
            
            return _fgColors[y * (int)Width + x];
        }
        
        public ConsoleColor GetBgColor(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return ConsoleColor.Black;
            
            return _bgColors[y * (int)Width + x];
        }

        public void SetGlyph(Vector pos, char c)
        {
            SetGlyph((int)pos.X, (int)pos.Y, c);
        }

        public void SetGlyph(int x, int y, char c)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            _glyphs[y * (int) Width + x] = c;
        }

        public void SetFgColor(Vector pos, ConsoleColor c)
        {
            SetFgColor((int)pos.X, (int)pos.Y, c);
        }
        
        public void SetFgColor(int x, int y, ConsoleColor c)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            _fgColors[y * (int) Width + x] = c;
        }

        public void SetBgColor(Vector pos, ConsoleColor c)
        {
            SetBgColor((int)pos.X, (int)pos.Y, c);
        }
        
        public void SetBgColor(int x, int y, ConsoleColor c)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            _bgColors[y * (int) Width + x] = c;
        }
    }
}