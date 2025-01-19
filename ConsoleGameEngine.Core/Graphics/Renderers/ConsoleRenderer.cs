using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ConsoleGameEngine.Core.Math;
using Microsoft.Win32.SafeHandles;

namespace ConsoleGameEngine.Core.Graphics.Renderers;

public class ConsoleRenderer : IRenderer
{
    private const uint EnableEditMode = 0x0040;
    private const int StdInputHandle = -10;

    private const int MfBycommand = 0x00000000;
    private const int ScClose = 0xF060;
    private const int ScMinimize = 0xF020;
    private const int ScMaximize = 0xF030;
    private const int ScSize = 0xF000;
    
    private const int StandardOutputHandle = -11;
    private static readonly IntPtr ConsoleOutputHandle = GetStdHandle(StandardOutputHandle);
    private const int FixedWidthTrueType = 54;
    private readonly SafeFileHandle _consoleHandle;
    
    private readonly CharInfo[] _screenBuffer;
    
    public int ScreenWidth => (int)Screen.Size.X;
    public int ScreenHeight => (int)Screen.Size.Y;
    public short PixelSize { get; private set; }
    
    public Rect Screen { get; }

    // TODO: Add flag for 24-bit color mode support
    public ConsoleRenderer(int width, int height, short pixelSize = 8)
    {
        _consoleHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
        if (_consoleHandle.IsInvalid)
        {
            throw new Exception("Console handle is invalid!");
        }
        
        Console.CursorVisible = false;
        DisableResize();
        DisableMouseInput();
        
        Console.OutputEncoding = Encoding.UTF8;

        if (pixelSize < 4) pixelSize = 4;
        PixelSize = pixelSize;
        SetCurrentFont("Modern DOS 8x8", pixelSize);

        // Clamp width and height while maintaining aspect ratio
        var maxWidth = Console.LargestWindowWidth - 1;
        var maxHeight = Console.LargestWindowHeight - 1;

        if (width  > maxWidth || height > maxHeight)
        {
            var widthRatio = (float) maxWidth / width;
            var heightRatio = (float) maxHeight / height;

            // use whichever multiplier is smaller
            var ratio = widthRatio < heightRatio ? widthRatio : heightRatio;

            width = (int) (width * ratio);
            height = (int) (height * ratio);
        }

        Screen = new Rect(Vector.Zero, new Vector(width, height));
        _screenBuffer = new CharInfo[width * height];

        #pragma warning disable CA1416
        Console.SetWindowSize(width, height);
        Console.SetBufferSize(width, height);
        #pragma warning restore CA1416
    }

    public void Render()
    {
        var boundsRect = new SmallRect 
        { 
            Left = 0, 
            Top = 0, 
            Right = (short)ScreenWidth, 
            Bottom = (short)ScreenHeight
        };

        WriteConsoleOutput(_consoleHandle, _screenBuffer,
            new Coord((short)ScreenWidth, (short)ScreenHeight),
            new Coord(0,0),
            ref boundsRect);
    }
    
    public void Draw(int x, int y, char c, Color24 fgColor, Color24 bgColor)
    {
        if (x >= ScreenWidth || x < 0 ||
            y >= ScreenHeight || y < 0)
        {
            return;
        }
    
        // TODO: write to buffer using Color24 when 24-bit color mode is enabled
        // Convert Color24 to 16-bit color
        ConsoleColor fgConsoleColor = fgColor;
        ConsoleColor bgConsoleColor = bgColor;
    
        // Compute the attributes as a short value
        var color = (short)((int)fgConsoleColor + ((int)bgConsoleColor << 4));
    
        // Update the screen buffer
        var index = y * ScreenWidth + x;
        _screenBuffer[index].Attributes = color;
        _screenBuffer[index].Char.UnicodeChar = c;
    }
    
    public Vector GetWindowPosition()
    {
        if (!GetWindowRect(GetConsoleWindow(), out IntRect rect))
        {
            var ex = Marshal.GetLastWin32Error();
            Console.WriteLine("Set error " + ex);
            throw new System.ComponentModel.Win32Exception(ex);
        }

        return new Vector(rect.Left, rect.Top);
    }
    
    public static void SetCurrentFont(string font, short fontSize = 0)
    {
        var before = new FontInfo
        {
            cbSize = Marshal.SizeOf<FontInfo>()
        };

        if (GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref before))
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
            if (!SetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref set))
            {
                var ex = Marshal.GetLastWin32Error();
                Console.WriteLine("Set error " + ex);
                throw new System.ComponentModel.Win32Exception(ex);
            }

            var after = new FontInfo
            {
                cbSize = Marshal.SizeOf<FontInfo>()
            };
            GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref after);

            return;
        }

        var er = Marshal.GetLastWin32Error();
        Console.WriteLine("Get error " + er);
        throw new System.ComponentModel.Win32Exception(er);
    }
    
    private static void DisableMouseInput()
    {
        var consoleHandle = GetStdHandle(StdInputHandle);
        GetConsoleMode(consoleHandle, out var consoleMode);
            
        // Clear the edit mode bit in the mode flags
        consoleMode &= ~EnableEditMode;

        SetConsoleMode(consoleHandle, consoleMode);
    }
        
    private static void DisableResize()
    {
        var handle = GetConsoleWindow();
        var sysMenu = GetSystemMenu(handle, false);

        if (handle != IntPtr.Zero)
        {
            DeleteMenu(sysMenu, ScClose, MfBycommand);
            DeleteMenu(sysMenu, ScMinimize, MfBycommand);
            DeleteMenu(sysMenu, ScMaximize, MfBycommand);
            DeleteMenu(sysMenu, ScSize, MfBycommand);
        }
    }
    
    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool maximumWindow, ref FontInfo consoleCurrentFontEx);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    
    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool maximumWindow, ref FontInfo consoleCurrentFontEx);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteConsoleOutput(
        SafeFileHandle hConsoleOutput, 
        CharInfo[] lpBuffer, 
        Coord dwBufferSize, 
        Coord dwBufferCoord, 
        ref SmallRect lpWriteRegion);
    
    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern SafeFileHandle CreateFile(
        string fileName,
        [MarshalAs(UnmanagedType.U4)] uint fileAccess,
        [MarshalAs(UnmanagedType.U4)] uint fileShare,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] int flags,
        IntPtr template);
    
    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        
    [DllImport("user32.dll")]
    private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();
        
    [DllImport(@"user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out IntRect lpRect);
}

[StructLayout(LayoutKind.Sequential)]
public struct Coord
{
    public short X;
    public short Y;

    public Coord(short x, short y)
    {
        X = x;
        Y = y;
    }
};

[StructLayout(LayoutKind.Explicit)]
public struct CharUnion
{
    [FieldOffset(0)] public char UnicodeChar;
    [FieldOffset(0)] public byte AsciiChar;
}

[StructLayout(LayoutKind.Explicit)]
public struct CharInfo
{
    [FieldOffset(0)] public CharUnion Char;
    [FieldOffset(2)] public short Attributes;
}

[StructLayout(LayoutKind.Sequential)]
public struct SmallRect
{
    public short Left;
    public short Top;
    public short Right;
    public short Bottom;
}
    
[StructLayout(LayoutKind.Sequential)]
struct IntRect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public int Width => Right - Left;
    public int Height => Bottom - Top;
}
    
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FontInfo
{
    internal int cbSize;
    internal int FontIndex;
    internal short FontWidth;
    public short FontSize;
    public int FontFamily;
    public int FontWeight;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string FontName;
}