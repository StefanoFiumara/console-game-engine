using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Graphics;

public interface IRenderer
{
    int Width => (int)Bounds.Width;
    int Height => (int)Bounds.Height;
    Rect Bounds { get; }
    
    /// <summary>
    /// Called by the Engine when a frame needs to be drawn.
    /// You do not need to call this method manually in your Update() method.
    /// </summary>
    void Render();
    
    void Draw(Vector position, char c) => Draw(position, c, Color24.White, Color24.Black);
    void Draw(Vector position, char c, Color24 fgColor) => Draw(position, c, fgColor, Color24.Black);
    void Draw(Vector position, char c, Color24 fgColor, Color24 bgColor) => Draw((int)position.X, (int)position.Y, c, fgColor, bgColor);
    void Draw(int x, int y, char c, Color24 fgColor) => Draw(x, y, c, fgColor, Color24.Black);
    void Draw(int x, int y, char c) => Draw(x, y, c, Color24.White, Color24.Black);
    void Draw(int x, int y, char c, Color24 fgColor, Color24 bgColor);
}

public abstract class BaseRenderer : IRenderer
{
    public int Width => (int)Bounds.Width;
    public int Height => (int)Bounds.Height;
    public abstract Rect Bounds { get; }
    
    /// <summary>
    /// Called by the Engine when a frame needs to be drawn.
    /// You do not need to call this method manually in your Update() method.
    /// </summary>
    public virtual void Render() { }
    
    /// <summary>
    /// Base method to draw a character to the renderers internal buffer
    /// </summary>
    public abstract void Draw(int x, int y, char c, Color24 fgColor, Color24 bgColor);
}