using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Graphics.Renderers;

/// <summary>
/// Allows us to treat a sprite as a rendering target
/// </summary>
/// <param name="target"></param>
public class SpriteRenderer(Sprite target) : BaseRenderer
{
    public override Rect Bounds => new(Vector.Zero, target.Size);

    public override void Draw(int x, int y, char c, Color24 fgColor, Color24 bgColor)
    {
        if (x >= Width || x < 0 || y >= Height || y < 0)
            return;
        
        target[x, y] = c;
        target.SetFgColor(x, y, fgColor);
        target.SetBgColor(x, y, c == Sprite.SolidPixel ? fgColor : bgColor);
    }
}