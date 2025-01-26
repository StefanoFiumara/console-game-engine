using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Graphics.Renderers;

// helper class allowing us to treat a sprite as a rendering target and call the various Draw methods on it
public class SpriteRenderer(Sprite target) : IRenderer
{
    public int ScreenWidth => (int)target.Width;
    public int ScreenHeight => (int)target.Height;
    public Rect Screen => new(Vector.Zero, target.Size);
    public short PixelSize => 1;

    public Vector GetWindowPosition() => Vector.Zero;

    public void Render() { /* Not Needed */ }

    public void Draw(int x, int y, char c, Color24 fgColor, Color24 bgColor)
    {
        if (x >= ScreenWidth || x < 0 || y >= ScreenHeight || y < 0)
            return;
        
        target[x, y] = c;
        target.SetFgColor(x, y, fgColor);
        target.SetBgColor(x, y, c == Sprite.SolidPixel ? fgColor : bgColor);
    }
}