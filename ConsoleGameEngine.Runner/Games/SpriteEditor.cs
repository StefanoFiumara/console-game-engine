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
        protected override string Name => "Canvas";

        private Sprite _canvas;
        
        public SpriteEditor()
        {
            InitConsole(128, 96, 12);
        }
        
        /*
         * TODO: Sprite Editor Features
         *
         * 1. Select FG and BG colors from color palette (all console colors available)
         * 2. Adjust Canvas Size
         * 3. Apply FG color with left click, BG color with right click
         * 4. Transparency with middle click?
         * 5. Save sprite to file (allow to name file?)
         * 6. Update Sprite class to be able to load from file
         * 7. Load sprite in editor from file in working directory (show file list?) 
         */
        
        protected override bool Create()
        {
            var gfx = "";
            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 24; j++)
                {
                    gfx += "#"; // TODO: Use special char here to tell the Sprite that FG and BG color should be the same 
                }

                gfx += "\n";
            }

            _canvas = new Sprite(gfx, fgColor: ConsoleColor.DarkGray, bgColor: ConsoleColor.DarkGray);
            _canvas.Position = ScreenRect.Center - Vector.Right * _canvas.Width * 0.5f;
            
            return true;
        }

        protected override bool Update(float elapsedTime, PlayerInput input)
        {
            if (input.IsKeyUp(KeyCode.Esc)) return false;
            
            Fill(ScreenRect, ' ');

            // TODO: Formalize this computation
            var tilePos = (input.MousePosition / PixelSize).Rounded;
            
            DrawString(Vector.Zero, $"Mouse: {input.MousePosition}");
            DrawString(0, 2, $"Tile Pos: {tilePos}");
            DrawString(0, 4, $"Screen Pos: {ScreenPosition}");
            DrawString(0, 6, $"Mouse Left: {input.IsKeyHeld(KeyCode.LeftMouse)}");
            DrawString(0, 8, $"Mouse Right: {input.IsKeyHeld(KeyCode.RightMouse)}");
            DrawString(0, 10, $"Mouse Middle: {input.IsKeyHeld(KeyCode.MiddleMouse)}");

            var canvasPos = tilePos - _canvas.Position;
            if (input.IsKeyHeld(KeyCode.LeftMouse))
            {
                _canvas.SetBgColor(canvasPos, ConsoleColor.DarkRed);
                _canvas.SetFgColor(canvasPos, ConsoleColor.DarkRed);
            }
            else if (input.IsKeyHeld(KeyCode.RightMouse))
            {
                _canvas.SetBgColor(canvasPos, ConsoleColor.DarkGray);
                _canvas.SetFgColor(canvasPos, ConsoleColor.DarkGray);
            }
            
            DrawSprite(_canvas);

            return true;
        }
    }
}
