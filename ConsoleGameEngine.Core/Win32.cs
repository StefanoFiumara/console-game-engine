using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ConsoleGameEngine.Core;

internal static class Win32
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    internal static extern short GetAsyncKeyState(int keyCode);
        
    [DllImport("user32.dll")]
    internal static extern bool GetCursorPos(ref Point lpPoint);
    
    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern IntPtr GetConsoleWindow();
        
    [DllImport(@"user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetWindowRect(IntPtr hWnd, out IntRect lpRect);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern IntPtr GetStdHandle(int nStdHandle);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool maximumWindow, ref FontInfo consoleCurrentFontEx);

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool maximumWindow, ref FontInfo consoleCurrentFontEx);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);
    
    [DllImport("kernel32.dll")]
    internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        
    [DllImport("user32.dll")]
    internal static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
}

    
[StructLayout(LayoutKind.Sequential)]
internal struct IntRect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}
    
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FontInfo
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