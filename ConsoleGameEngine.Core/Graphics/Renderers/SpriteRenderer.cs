using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Graphics.Renderers;

// helper class allowing us to treat a sprite as a rendering target and call the various Draw methods on it
public class SpriteRenderer(Sprite target) : IRenderer
{
    private readonly Sprite _target = target;
    
    public int ScreenWidth => (int)_target.Width;
    public int ScreenHeight => (int)_target.Height;
    public Rect Screen => new(Vector.Zero, _target.Size);
    public short PixelSize => 1;

    public Vector GetWindowPosition() => Vector.Zero;

    public void Render() { /* Not Needed */ }

    public void Draw(int x, int y, char c, Color24 fgColor, Color24 bgColor)
    {
        if (x >= ScreenWidth || x < 0 || y >= ScreenHeight || y < 0)
            return;
        
        _target[x, y] = c;
        _target.SetFgColor(x, y, fgColor);
        _target.SetBgColor(x, y, c == Sprite.SolidPixel ? fgColor : bgColor);
    }
}