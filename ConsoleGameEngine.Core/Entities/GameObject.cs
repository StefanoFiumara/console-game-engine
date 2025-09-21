using System;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Entities;

public abstract class GameObject(Vector position = default)
{
    public Vector Position { get; set; } = position;
    public abstract Rect Bounds { get; }
    public abstract void Draw(IRenderer renderer);
}

public class Button : GameObject
{
    private readonly Action _onClick;
    public override Rect Bounds => new(Position, _size);
    
    public string Label { get; set; }
    private readonly Vector _size;

    public Button(string label, int width, int height, Action onClick, Vector position = default)
    {
        Label = label;
        Position = position;
        _onClick = onClick;
        width = System.Math.Max(label.Length, width);
        height = System.Math.Max(3, height);
        _size = new Vector(width, height);
    }
    
    // TODO: How do we listen for clicks? 
    
    public override void Draw(IRenderer renderer)
    {
        
    }
}