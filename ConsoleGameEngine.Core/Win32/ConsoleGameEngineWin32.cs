using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
// ReSharper disable StringLiteralTypo

namespace ConsoleGameEngine.Core.Win32
{
    public abstract class ConsoleGameEngineWin32
    {
        private const uint ENABLE_EDIT_MODE = 0x0040;
        private const int STD_INPUT_HANDLE = -10;
        private const int STANDARD_OUTPUT_HANDLE = -11;
        
        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xF060;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_SIZE = 0xF000;
        
        private const int FIXED_WIDTH_TRUE_TYPE = 54;

        private static readonly IntPtr ConsoleOutputHandle = GetStdHandle(STANDARD_OUTPUT_HANDLE);
        private readonly SafeFileHandle _consoleHandle;

        /// <summary>
        /// This class does some Win32 API stuff to configure the console window
        /// Things like writing to the console buffer directly and disabling resize options or "quick edit mode"
        /// These features are not accessible in native C#, so we import some kernel32.dll functions to achieve it.
        /// </summary>
        protected ConsoleGameEngineWin32()
        {
            Console.CursorVisible = false;
            DisableMouseInput();
            DisableResize();

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

        private static void DisableMouseInput()
        {
            var consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(consoleHandle, out var consoleMode);
            
            // Clear the edit mode bit in the mode flags
            consoleMode &= ~ENABLE_EDIT_MODE;

            SetConsoleMode(consoleHandle, consoleMode);
        }

        private static void DisableResize()
        {
            var handle = GetConsoleWindow();
            var sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }
        }
        
        
        
        public static void SetCurrentFont(string font, short fontSize = 0)
        {
            FontInfo before = new FontInfo
            {
                cbSize = Marshal.SizeOf<FontInfo>()
            };

            if (GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref before))
            {

                FontInfo set = new FontInfo
                {
                    cbSize = Marshal.SizeOf<FontInfo>(),
                    FontIndex = 0,
                    FontFamily = FIXED_WIDTH_TRUE_TYPE,
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

                FontInfo after = new FontInfo
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
    }
}
