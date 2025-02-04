using System;
using System.Runtime.InteropServices;
using System.Text;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Utilities;

namespace ConsoleGameEngine.Core.Graphics.Renderers;

public struct PixelInfo
{
    public char Char;
    public Color24 Foreground;
    public Color24 Background;
}

public class ConsoleRenderer : BaseRenderer
{
    private const uint EnableEditModeFlag = 0x0040;
    private const int EnableVirtualTerminalProcessingFlag = 0x0004;
    private const int StdInputHandle = -10;
    private const int StdOutputHandle = -11;

    private const int MfBycommand = 0x00000000;
    private const int ScClose = 0xF060;
    private const int ScMinimize = 0xF020;
    private const int ScMaximize = 0xF030;
    private const int ScSize = 0xF000;

    private static readonly IntPtr ConsoleOutputHandle = Win32.GetStdHandle(StdOutputHandle);
    private const int FixedWidthTrueType = 54;

    private readonly PixelInfo[] _screenBuffer;
    private bool _isDirty = true;

    public override Rect Bounds { get; }

    public ConsoleRenderer(int width, int height, short pixelSize = 8)
    {
        Console.CursorVisible = false;
        DisableResize();
        DisableMouseInput();

        SetCurrentFont("Modern DOS 8x8", pixelSize);

        // Clamp width and height while maintaining aspect ratio
        var maxWidth = Console.LargestWindowWidth - 1;
        var maxHeight = Console.LargestWindowHeight - 1;

        if (width > maxWidth || height > maxHeight)
        {
            var widthRatio = (float)maxWidth / width;
            var heightRatio = (float)maxHeight / height;

            // use whichever multiplier is smaller
            var ratio = widthRatio < heightRatio ? widthRatio : heightRatio;

            width = (int)(width * ratio);
            height = (int)(height * ratio);
        }

        Bounds = new Rect(Vector.Zero, new Vector(width, height));

        EnableVirtualTerminalProcessing();
        _screenBuffer = new PixelInfo[width * height];

#pragma warning disable CA1416
        Console.SetWindowSize(width, height);
        Console.SetBufferSize(width, height);
#pragma warning restore CA1416
    }

    public override void Render()
    {
        if (!_isDirty) return;

        var ansiSequence = GenerateAnsiSequence(_screenBuffer);
        byte[] buffer = Encoding.ASCII.GetBytes(ansiSequence);

        Win32.WriteFile(ConsoleOutputHandle, buffer, (uint)buffer.Length, out _, IntPtr.Zero);
        _isDirty = false;
    }

    private string GenerateAnsiSequence(PixelInfo[] buffer)
    {
        var sb = new StringBuilder();

        Color24? currentFgColor = null;
        Color24? currentBgColor = null;

        for (int i = 0; i < buffer.Length; i++)
        {
            var cell = buffer[i];

            if (currentFgColor != cell.Foreground)
            {
                string fgColorSeq = $"\e[38;2;{cell.Foreground.R};{cell.Foreground.G};{cell.Foreground.B}m";
                sb.Append(fgColorSeq);
                currentFgColor = cell.Foreground;
            }

            if (currentBgColor != cell.Background)
            {
                string bgColorSeq = $"\e[48;2;{cell.Background.R};{cell.Background.G};{cell.Background.B}m";
                sb.Append(bgColorSeq);
                currentBgColor = cell.Background;
            }

            // Append the character (or blank space) for this cell
            sb.Append(cell.Char);

            // Check if we've reached the end of a row, and if so, add a newline
            bool isRowEnd = (i + 1) % Width == 0;
            // Do not add a new line for the last row in the buffer
            if (isRowEnd && i != buffer.Length - 1)
            {
                sb.Append(Environment.NewLine);
            }
        }

        return sb.ToString();
    }

    public override void Draw(int x, int y, char c, Color24 fgColor, Color24 bgColor)
    {
        if (x >= Width || x < 0 || y >= Height || y < 0)
            return;

        var index = y * Width + x;

        // TODO: only mark dirty if the replaced character/color in the buffer differs from this one
        _isDirty = true;

        _screenBuffer[index].Char = c;
        _screenBuffer[index].Foreground = fgColor;
        _screenBuffer[index].Background = c == Sprite.SolidPixel ? fgColor : bgColor;
    }

    public static void SetCurrentFont(string font, short fontSize = 0)
    {
        var before = new FontInfo
        {
            cbSize = Marshal.SizeOf<FontInfo>()
        };

        if (Win32.GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref before))
        {
            var set = new FontInfo
            {
                cbSize = Marshal.SizeOf<FontInfo>(),
                FontIndex = 0,
                FontFamily = FixedWidthTrueType,
                FontName = font,
                FontWeight = 400,
                FontSize = fontSize > 0 ? fontSize : before.FontSize
            };

            // Get some settings from current font.
            if (!Win32.SetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref set))
            {
                var ex = Marshal.GetLastWin32Error();
                Console.WriteLine("Set error " + ex);
                throw new System.ComponentModel.Win32Exception(ex);
            }

            var after = new FontInfo
            {
                cbSize = Marshal.SizeOf<FontInfo>()
            };
            Win32.GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref after);

            return;
        }

        var er = Marshal.GetLastWin32Error();
        Console.WriteLine("Get error " + er);
        throw new System.ComponentModel.Win32Exception(er);
    }

    private static void DisableMouseInput()
    {
        var consoleHandle = Win32.GetStdHandle(StdInputHandle);
        Win32.GetConsoleMode(consoleHandle, out var consoleMode);

        // Clear the edit mode bit in the mode flags
        consoleMode &= ~EnableEditModeFlag;

        Win32.SetConsoleMode(consoleHandle, consoleMode);
    }

    private static void DisableResize()
    {
        var handle = Win32.GetConsoleWindow();
        var sysMenu = Win32.GetSystemMenu(handle, false);

        if (handle != IntPtr.Zero)
        {
            Win32.DeleteMenu(sysMenu, ScClose, MfBycommand);
            Win32.DeleteMenu(sysMenu, ScMinimize, MfBycommand);
            Win32.DeleteMenu(sysMenu, ScMaximize, MfBycommand);
            Win32.DeleteMenu(sysMenu, ScSize, MfBycommand);
        }
    }

    private static void EnableVirtualTerminalProcessing()
    {
        if (!Win32.GetConsoleMode(ConsoleOutputHandle, out var mode))
            throw new InvalidOperationException("Failed to get console mode.");

        mode |= EnableVirtualTerminalProcessingFlag;

        if (!Win32.SetConsoleMode(ConsoleOutputHandle, mode))
            throw new InvalidOperationException("Failed to set console mode.");
    }
}