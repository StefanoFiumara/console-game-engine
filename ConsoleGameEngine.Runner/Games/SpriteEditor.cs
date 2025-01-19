using System;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games;

// ReSharper disable once UnusedType.Global
public class SpriteEditor() : ConsoleGame(new ConsoleRenderer(width: 96, height: 64, pixelSize: 16), targetFps: 120)
{
    private GameObject _canvas;
        
    private GameObject[] _palette;
        
    private GameObject _primaryColor;
    private GameObject _secondaryColor;

    private ConsoleColor Primary => _primaryColor.Sprite.GetFgColor(0);
    private ConsoleColor Secondary => _secondaryColor.Sprite.GetFgColor(0);

    /*
     * TODO: Sprite Editor Features
     * 1. Draw Brush
     * 2. Adjustable Brush Size
     * 3. Adjustable Canvas Size
     * 4. Transparency
     * 5. Save sprite to file (allow to name file?)
     * 6. Update Sprite class to be able to load from file
     * 7. Load sprite in editor from file in working directory (show file list?)
     */
        
    protected override bool Create(IRenderer renderer)
    {
        _canvas = CreateCanvas(renderer, 32, 32);
        _palette = CreatePalette(renderer);

        _primaryColor = new GameObject(
            Sprite.CreateSolid(4,4, Color24.Red),
            new Vector(_palette[0].Position.X, _palette[^1].Bounds.Bottom + 4));
            
        _secondaryColor = new GameObject(Sprite.CreateSolid(4,4, Color24.Blue),
            new Vector(_primaryColor.Position.X, _primaryColor.Bounds.Bottom + 1));

        return true;
    }

    private GameObject CreateCanvas(IRenderer renderer, int width, int height)
    {
        var canvas = Sprite.CreateSolid(width, height, Color24.Gray);
        var pos = (renderer.Screen.Center - canvas.Size * 0.5f).Rounded;
        return new GameObject(canvas, pos);
    }

    private GameObject[] CreatePalette(IRenderer renderer)
    {
        var colors = Enum.GetValues<ConsoleColor>();
        var palette = new GameObject[colors.Length];

        // Create the sprites to render the palette colors on the screen
        for (var i = 0; i < colors.Length; i++)
        {
            palette[i] = new GameObject(Sprite.CreateSolid(2, 2, colors[i]));
        }

        // assign positions
        var yStart = (int) (renderer.ScreenHeight * 0.5f - palette.Length);
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

    protected override bool Update(float elapsedTime, IRenderer renderer, PlayerInput input)
    {
        if (input.IsKeyUp(KeyCode.Esc)) return false;
            
        renderer.Fill(' ');

        // Check input
        var canvasPos = input.MousePosition - _canvas.Position;
            
        // Paint selected color to canvas
        if (input.IsKeyHeld(KeyCode.LeftMouse))
        {
            _canvas.Sprite.SetFgColor(canvasPos, Primary);
        }
        else if (input.IsKeyHeld(KeyCode.RightMouse))
        {
            _canvas.Sprite.SetFgColor(canvasPos, Secondary);
        }

        // Show color name on palette hover
        foreach (var color in _palette)
        {
            if (color.Bounds.Contains(input.MousePosition))
            {
                renderer.DrawString(
                    (int)(_canvas.Position.X + _canvas.Bounds.Width), 
                    (int)_canvas.Position.Y - 4, 
                    color.Sprite.GetFgColor(0).ToString(),
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
                    _primaryColor.Sprite.SetSpriteColor(color.Sprite.GetFgColor(0));
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
                    _secondaryColor.Sprite.SetSpriteColor(color.Sprite.GetFgColor(0));
                    break;
                }
            }
        }
            
        // Draw HUD
        renderer.DrawString((int) renderer.Screen.Center.X, 3, "SPRITE EDITOR", alignment: TextAlignment.Centered);

        renderer.DrawBorder(_canvas.Bounds, '*');
        renderer.DrawObject(_canvas);
            
        // On Canvas Hover
        if (_canvas.Bounds.Contains(input.MousePosition))
        {
            // Show canvas position
            renderer.DrawString(
                (int)(_canvas.Position.X + _canvas.Bounds.Width), 
                (int)_canvas.Position.Y - 2, 
                canvasPos.ToString(),
                alignment: TextAlignment.Right);
                
            // Show Brush
            renderer.Draw(input.MousePosition, Sprite.SolidPixel, Primary, Primary);
        }
            
        // Draw Palette
        var paletteBorder = new Rect(_palette[0].Position, new Vector(4, _palette.Length));
        renderer.DrawBorder(paletteBorder, '*');
            
        foreach (var color in _palette)
        {
            renderer.DrawObject(color);
        }
            
        var selectedBorder = new Rect(_primaryColor.Position, new Vector(4, 9));
        renderer.DrawBorder(selectedBorder, '*', Color24.Gray);
        renderer.DrawString((int)_secondaryColor.Position.X, (int)_secondaryColor.Position.Y - 1, "****", Color24.Gray, Color24.Black);

        renderer.DrawObject(_primaryColor);
        renderer.DrawObject(_secondaryColor);

        renderer.DrawString((int)_primaryColor.Bounds.Right + 2, (int)_primaryColor.Position.Y + 1, $"1: {Primary}");
        renderer.DrawString((int)_secondaryColor.Bounds.Right + 2, (int)_secondaryColor.Position.Y + 1, $"2: {Secondary}");
        return true;
    }
}