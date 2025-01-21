using System;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games;

// ReSharper disable once UnusedType.Global
public class SpriteEditor() : ConsoleGame(new ConsoleRenderer(width: 192, height: 128, pixelSize: 8), targetFps: 120)
{
    private const int CanvasSize = 64;
    private const int PaletteSize = 50;
    private GameObject _canvas;
    private GameObject _palette;
    private GameObject _colorPreview;
    private GameObject _primaryPreview;
    private GameObject _secondaryPreview;
        
    private Color24 _primary = Color24.Red;
    private Color24 _secondary = Color24.Blue;

    private float _saturation = 1f;

    /*
     * TODO: Sprite Editor Features
        * Adjustable Brush Size
        * Adjustable Canvas Size
        * Transparency
        * Save sprite to file (name file?)
        * Load sprite from file
     */
        
    protected override bool Create(IRenderer renderer)
    {
        var canvasSpr = Sprite.CreateSolid(CanvasSize, CanvasSize, Color24.White); 
        var canvasPos = (renderer.Screen.Center - canvasSpr.Size * 0.5f).Rounded;
        _canvas = new GameObject(canvasSpr, canvasPos);
        
        var paletteSpr = CreateColorPalette(size: PaletteSize, _saturation);
        var palettePos = _canvas.Bounds.TopRight + Vector.Right * 4;
        _palette = new GameObject(paletteSpr, palettePos);
        
        var previewSpr = Sprite.CreateSolid(8, 8, Color24.Black);
        var previewPos = _palette.Bounds.TopRight + Vector.Up * 10 + Vector.Left * 8;
        _colorPreview = new GameObject(previewSpr, previewPos);
        
        var primarySpr = Sprite.CreateSolid(8, 8, _primary);
        var primaryPos = _palette.Bounds.BottomLeft + Vector.Down * 2;
        _primaryPreview = new GameObject(primarySpr, primaryPos);
        
        var secondarySpr = Sprite.CreateSolid(8, 8, _secondary);
        var secondaryPos = _primaryPreview.Bounds.TopRight + Vector.Right * 4;
        _secondaryPreview = new GameObject(secondarySpr, secondaryPos);
        
        return true;
    }

    protected override bool Update(float elapsedTime, IRenderer renderer, PlayerInput input)
    {
        if (input.IsKeyUp(KeyCode.Esc)) return false;
        
        renderer.Fill(' ');
        renderer.DrawString((int) renderer.Screen.Center.X, 3, "SPRITE EDITOR", alignment: TextAlignment.Centered);
        
        renderer.DrawString(10, (int)_canvas.Position.Y + 1, "CANVAS");
        renderer.DrawString(10, (int)_canvas.Position.Y + 4, "Left Click: Draw Primary Color");
        renderer.DrawString(10, (int)_canvas.Position.Y + 6, "Right Click: Draw Secondary Color");
        renderer.DrawString(10, (int)_canvas.Position.Y + 10, "PALETTE");
        renderer.DrawString(10, (int)_canvas.Position.Y + 13, "Left Click: Set Primary Color");
        renderer.DrawString(10, (int)_canvas.Position.Y + 15, "Right Click: Set Secondary Color");
        renderer.DrawString(10, (int)_canvas.Position.Y + 17, "Up Arrow: Increase Saturation");
        renderer.DrawString(10, (int)_canvas.Position.Y + 19, "Down Arrow: Decrease Saturation");
        
        renderer.DrawObject(_canvas);
        renderer.DrawBorder(_canvas.Bounds);
        
        // Canvas
        if (_canvas.Bounds.Contains(input.MousePosition))
        {
            var canvasPos = (input.MousePosition - _canvas.Position).Rounded;

            // Show canvas position
            renderer.DrawString(_canvas.Bounds.TopRight  + Vector.Up * 3, canvasPos.ToString(), alignment: TextAlignment.Right);
            
            // Show Preview Brush
            renderer.Draw(input.MousePosition, Sprite.SolidPixel, _primary);
            
            // Draw selected color onto canvas
            if (input.IsKeyHeld(KeyCode.LeftMouse))
                _canvas.Sprite.SetFgColor(canvasPos, _primary);
            else if (input.IsKeyHeld(KeyCode.RightMouse)) 
                _canvas.Sprite.SetFgColor(canvasPos, _secondary);
        }
        
        // Palette Saturation Control
        if (input.IsKeyHeld(KeyCode.Up))
        {
            _saturation = MathF.Min(_saturation + elapsedTime, 1f);
            _palette.Sprite = CreateColorPalette(PaletteSize, _saturation);
            
        }
        else if (input.IsKeyHeld(KeyCode.Down))
        {
            _saturation = MathF.Max(_saturation - elapsedTime, 0f);
            _palette.Sprite = CreateColorPalette(PaletteSize, _saturation);
        }
        
        // Palette
        renderer.DrawObject(_palette);
        renderer.DrawBorder(_palette.Bounds);
        
        // Palette Interaction
        if (_palette.Bounds.Contains(input.MousePosition))
        {
            var palettePos = (input.MousePosition - _palette.Position).Rounded;
            var color = _palette.Sprite.GetFgColor((int)palettePos.X, (int)palettePos.Y);
            
            _colorPreview.Sprite.SetSpriteColor(color);
            renderer.DrawString(_colorPreview.Bounds.BottomLeft + Vector.Left + Vector.Up * 3, $"R {color.R.ToString(),3}", alignment: TextAlignment.Right);
            renderer.DrawString(_colorPreview.Bounds.BottomLeft + Vector.Left + Vector.Up * 2, $"G {color.G.ToString(),3}", alignment: TextAlignment.Right);
            renderer.DrawString(_colorPreview.Bounds.BottomLeft + Vector.Left + Vector.Up * 1, $"B {color.B.ToString(),3}", alignment: TextAlignment.Right);

            if (input.IsKeyUp(KeyCode.LeftMouse))
            {
                _primary = color;
                _primaryPreview.Sprite.SetSpriteColor(_primary);
            }
            else if (input.IsKeyUp(KeyCode.RightMouse))
            {
                _secondary = color;
                _secondaryPreview.Sprite.SetSpriteColor(_secondary);
            }
            
            renderer.DrawObject(_colorPreview);
            renderer.DrawBorder(_colorPreview.Bounds);
        }
        
        // Selected colors
        renderer.DrawObject(_primaryPreview);
        renderer.DrawBorder(_primaryPreview.Bounds);
        
        renderer.DrawObject(_secondaryPreview);
        renderer.DrawBorder(_secondaryPreview.Bounds);
        return true;
    }

    private static Sprite CreateColorPalette(int size, float saturation)
    {
        var result = new Sprite(size, size);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var color = Color24.FromHsv((float)x / size * 360, saturation, 1 - (float)y / (size - 1));
                result[x, y] = Sprite.SolidPixel;
                result.SetFgColor(x, y, color);
            }
        }

        return result;
    }
}