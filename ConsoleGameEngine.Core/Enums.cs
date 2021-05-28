using System;

namespace ConsoleGameEngine.Core
{
    public enum Keys
    {
        Esc = 0x1B,
        Space = 0x20,
        
        Left = 0x25,
        Up = 0x26,
        Right = 0x27,
        Down = 0x28,
    }
    
    [Flags]
    public enum KeyStates
    {
        None = 0,
        Down = 1,
        Toggled = 2
    }
}