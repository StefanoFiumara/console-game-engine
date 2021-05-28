using System;
using System.Linq;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects
{
    public class Sprite
    {
        private readonly char[] _glyphs;
        private readonly ConsoleColor[] _fgColors;
        private readonly ConsoleColor[] _bgColors;

        public Vector Position { get; set; }
        public Vector Velocity { get; set; }

        public float Width { get; }
        public float Height { get; }

        public Vector Center => new Vector((int)Position.X, (int)Position.Y) + new Vector(Width, Height) * 0.5f;

        public Sprite(string gfx, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
        {
            var splitGfx = gfx.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var width = splitGfx.Max(c => c.Length);
            var height = splitGfx.Length;

            // ensure rectangular dimensions by padding width where applicable.
            for (var i = 0; i < splitGfx.Length; i++)
            {
                if (splitGfx[i].Length != width)
                {
                    splitGfx[i] = splitGfx[i].PadRight(width);
                }
            }

            Width = width;
            Height = height;
            
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
        
        public char GetGlyph(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return ' ';
            
            return _glyphs[y * (int)Width + x];
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
        
        public void SetGlyph(int x, int y, char c)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            _glyphs[y * (int) Width + x] = c;
        }
        
        public void SetFgColor(int x, int y, ConsoleColor c)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            _fgColors[y * (int) Width + x] = c;
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