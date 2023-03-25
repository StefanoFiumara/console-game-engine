using System;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games
{
    // ReSharper disable once UnusedType.Global
    public class SpriteEditor : ConsoleGameEngineBase
    {
        protected override string Name => "SPRITE EDITOR";

        private Sprite _canvas;
        
        private Sprite[] _palette;
        
        private Sprite _primaryColor;
        private Sprite _secondaryColor;

        private ConsoleColor Primary => _primaryColor.GetFgColor(0);
        private ConsoleColor Secondary => _secondaryColor.GetFgColor(0);
        
        public SpriteEditor()
        {
            InitConsole(96, 64, 16, targetFps: 120);
        }
        
        /*
         * TODO: Sprite Editor Features
         *
         * 2. Adjust Canvas Size
         * 5. Save sprite to file (allow to name file?)
         * 6. Update Sprite class to be able to load from file
         * 7. Load sprite in editor from file in working directory (show file list?) 
         */
        
        protected override bool Create()
        {
            _canvas = CreateCanvas(32, 32);
            _palette = CreatePalatte();

            _primaryColor = Sprite.CreateSolid(4,4, ConsoleColor.Red);
            _primaryColor.Position = new Vector(_palette[0].Position.X, _palette[^1].Bounds.Bottom + 4);
            
            _secondaryColor = Sprite.CreateSolid(4,4, ConsoleColor.Blue);
            _secondaryColor.Position = new Vector(_primaryColor.Position.X, _primaryColor.Bounds.Bottom + 1);

            return true;
        }

        private Sprite CreateCanvas(int width, int height)
        {
            var canvas = Sprite.CreateSolid(width, height, ConsoleColor.Gray);
            canvas.Position = (ScreenRect.Center - canvas.Size * 0.5f).Rounded;
            return canvas;
        }

        private Sprite[] CreatePalatte()
        {
            var colors = Enum.GetValues<ConsoleColor>();
            var palette = new Sprite[colors.Length];

            // Create the sprites to render the palette colors on the screen
            for (var i = 0; i < colors.Length; i++)
            {
                palette[i] = Sprite.CreateSolid(2, 2, colors[i]);
            }

            // assign positions
            var yStart = (int) (ScreenHeight * 0.5f - palette.Length);
            var xStart = (int) _canvas.Bounds.Right + 4;
            for (int i = 0; i < palette.Length; i++)
            {
                palette[i].Position = new Vector(xStart, yStart);
                xStart += 2;
                yStart += i % 2 == 1 ? 2 : 0;

                if (i % 2 == 1)
                {
                    xStart -= 4;
                }
            }

            return palette;
        }

        protected override bool Update(float elapsedTime, PlayerInput input)
        {
            if (input.IsKeyUp(KeyCode.Esc)) return false;
            
            Fill(ScreenRect, ' ');

            // Check input
            var canvasPos = input.MousePosition - _canvas.Position;
            
            // Paint selected color to canvas
            if (input.IsKeyHeld(KeyCode.LeftMouse))
            {
                _canvas.SetFgColor(canvasPos, Primary);
            }
            else if (input.IsKeyHeld(KeyCode.RightMouse))
            {
                _canvas.SetFgColor(canvasPos, Secondary);
            }

            // Show color name on palette hover
            foreach (var color in _palette)
            {
                if (color.Bounds.Contains(input.MousePosition))
                {
                    DrawString(
                        (int)(_canvas.Position.X + _canvas.Width), 
                        (int)_canvas.Position.Y - 4, 
                        color.GetFgColor(0).ToString(),
                        alignment: TextAlignment.Right);
                    break;
                }
            }
            
            
            // Select new colors from palette
            if (input.IsKeyUp(KeyCode.LeftMouse))
            {
                foreach (var color in _palette)
                {
                    if (color.Bounds.Contains(input.MousePosition))
                    {
                        _primaryColor.SetSpriteColor(color.GetFgColor(0));
                        break;
                    }
                }
            }
            else if (input.IsKeyUp(KeyCode.RightMouse))
            {
                foreach (var color in _palette)
                {
                    if (color.Bounds.Contains(input.MousePosition))
                    {
                        _secondaryColor.SetSpriteColor(color.GetFgColor(0));
                        break;
                    }
                }
            }
            
            // Draw HUD
            DrawString((int) ScreenRect.Center.X, 3, Name, alignment: TextAlignment.Centered);

            DrawBorder(_canvas.Bounds, '*');
            DrawSprite(_canvas);
            
            // Show Canvas Coordinates On Canvas Hover
            if (_canvas.Bounds.Contains(input.MousePosition))
            {
                DrawString(
                    (int)(_canvas.Position.X + _canvas.Width), 
                    (int)_canvas.Position.Y - 2, 
                    canvasPos.ToString(),
                    alignment: TextAlignment.Right);
            }
            
            // Draw Palette
            var paletteBorder = new Rect(_palette[0].Position, new Vector(4, _palette.Length));
            DrawBorder(paletteBorder, '*');
            
            foreach (var color in _palette)
            {
                DrawSprite(color);
            }
            
            var selectedBorder = new Rect(_primaryColor.Position, new Vector(4, 9));
            DrawBorder(selectedBorder, '*', ConsoleColor.Gray);
            DrawString((int)_secondaryColor.Position.X, (int)_secondaryColor.Position.Y - 1, "****", ConsoleColor.Gray);

            DrawSprite(_primaryColor);
            DrawSprite(_secondaryColor);

            DrawString((int)_primaryColor.Bounds.Right + 2, (int)_primaryColor.Position.Y + 1, $"1: {Primary}");
            DrawString((int)_secondaryColor.Bounds.Right + 2, (int)_secondaryColor.Position.Y + 1, $"2: {Secondary}");
            return true;
        }
    }
}

