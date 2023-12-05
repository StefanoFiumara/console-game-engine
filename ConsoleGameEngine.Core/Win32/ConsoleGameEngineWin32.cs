using System;
using System.IO;
using System.Runtime.InteropServices;
using ConsoleGameEngine.Core.Math;
using Microsoft.Win32.SafeHandles;
// ReSharper disable StringLiteralTypo

namespace ConsoleGameEngine.Core.Win32;

public abstract class ConsoleGameEngineWin32
{
    private const uint EnableEditMode = 0x0040;
    private const int StdInputHandle = -10;
    private const int StandardOutputHandle = -11;
        
    private const int MfBycommand = 0x00000000;
    private const int ScClose = 0xF060;
    private const int ScMinimize = 0xF020;
    private const int ScMaximize = 0xF030;
    private const int ScSize = 0xF000;
        
    private const int FixedWidthTrueType = 54;

    private static readonly IntPtr ConsoleOutputHandle = GetStdHandle(StandardOutputHandle);
    private readonly SafeFileHandle _consoleHandle;
        
    /// <summary>
    /// This class does some Win32 API stuff to configure the console window
    /// Things like writing to the console buffer directly and disabling resize options or "quick edit mode"
    /// These features are not accessible in native C#, so we import some kernel32.dll functions to achieve it.
    /// </summary>
    protected ConsoleGameEngineWin32()
    {
        Console.CursorVisible = false;
        DisableResize();
        DisableMouseInput();
        _consoleHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
        if (_consoleHandle.IsInvalid)
        {
            throw new Exception("Console handle is invalid!");
        }
    }
        
    protected void DrawBuffer(CharInfo[] buffer, int width, int height)
    {
        var boundsRect = new SmallRect 
        { 
            Left = 0, 
            Top = 0, 
            Right = (short)width, 
            Bottom = (short)height
        };

        WriteConsoleOutput(_consoleHandle, buffer,
            new Coord((short)width, (short)height),
            new Coord(0,0),
            ref boundsRect);
    }

    protected Vector GetWindowPosition()
    {
        if (!GetWindowRect(GetConsoleWindow(), out IntRect rect))
        {
            var ex = Marshal.GetLastWin32Error();
            Console.WriteLine("Set error " + ex);
            throw new System.ComponentModel.Win32Exception(ex);
        }

        return new Vector(rect.Left, rect.Top);
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

    #region DLL Imports

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
        
    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern SafeFileHandle CreateFile(
        string fileName,
        [MarshalAs(UnmanagedType.U4)] uint fileAccess,
        [MarshalAs(UnmanagedType.U4)] uint fileShare,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] int flags,
        IntPtr template);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteConsoleOutput(
        SafeFileHandle hConsoleOutput, 
        CharInfo[] lpBuffer, 
        Coord dwBufferSize, 
        Coord dwBufferCoord, 
        ref SmallRect lpWriteRegion);
        
    [DllImport("user32.dll")]
    private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();
        
    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool maximumWindow, ref FontInfo consoleCurrentFontEx);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool maximumWindow, ref FontInfo consoleCurrentFontEx);
        
    [DllImport(@"user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out IntRect lpRect);
        
    #endregion
        
}