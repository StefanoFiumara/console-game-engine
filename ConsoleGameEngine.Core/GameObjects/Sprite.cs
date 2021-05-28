using System;
using System.Linq;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.GameObjects
{
    public class Sprite
    {
        private readonly char[] _glyphs;
        private readonly ConsoleColor[] _colors;

        public Vector Position { get; set; }
        public Vector Velocity { get; set; }

        public float Width { get; }
        public float Height { get; }

        public Vector Center => Position + new Vector(Width, Height) * 0.5f;

        public Sprite(string gfx, ConsoleColor color = ConsoleColor.White)
        {
            var splitGfx = gfx.Split('\n');
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
            
            _colors = new ConsoleColor[_glyphs.Length];
            
            SetSpriteColor(color);
        }

        public void SetSpriteColor(ConsoleColor color)
        {
            for (int i = 0; i < _colors.Length; i++)
            {
                _colors[i] = color;
            }
        }
        
        public char GetGlyph(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return ' ';
            
            return _glyphs[y * (int)Width + x];
        }

        public ConsoleColor GetColor(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return ConsoleColor.Black;
            
            return _colors[y * (int)Width + x];
        }
        
        public void SetGlyph(int x, int y, char c)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            _glyphs[y * (int) Width + x] = c;
        }
        
        public void SetColor(int x, int y, ConsoleColor c)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            _colors[y * (int) Width + x] = c;
        }
    }
}