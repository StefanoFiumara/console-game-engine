using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Utilities;

namespace ConsoleGameEngine.Core.Input;

public class PlayerInput
{
    private readonly short _pixelSize;

    private readonly bool[] _currentKeyState;
    private readonly bool[] _previousKeyState;
        
    private readonly KeyState[] _keyStates;
    private readonly KeyCode[] _keys;
        
    // Point that will be updated by the function with the current mouse coordinates
    private Point _pointRef;
    private Vector _mousePosition;

    /// <summary>
    /// The mouse position represented as a coordinate on the screen
    /// </summary>
    public Vector MousePosition => (_mousePosition / _pixelSize);
    
    internal PlayerInput(short pixelSize)
    {
        _pixelSize = pixelSize;
        _keys = Enum.GetValues<KeyCode>().ToArray();

        _currentKeyState = new bool[256];
        _previousKeyState = new bool[256];
        _keyStates = new KeyState[256];
    }
    
    /// <summary>
    /// Returns true if the given key was pressed in the current frame 
    /// </summary>
    public bool IsKeyDown(KeyCode k) => _keyStates[(int) k].IsDown;

    /// <summary>
    /// Returns true if the given key was released in the current frame 
    /// </summary>
    public bool IsKeyUp(KeyCode k) => _keyStates[(int) k].IsReleased;

    /// <summary>
    /// Returns true if the given key is being held down in the current frame 
    /// </summary>
    public bool IsKeyHeld(KeyCode k) => _keyStates[(int) k].IsHeld;

    /// <summary>
    /// Returns true if the given key combination is pressed in the current frame.
    /// A key combination is considered "pressed" during the KeyDown event of the last key in the list if the first n - 1 keys are being held 
    /// </summary>
    public bool IsCommandPressed(params KeyCode[] keys)
    {
        if (keys == null || keys.Length == 0)
            throw new ArgumentException("At least one key must be provided.", nameof(keys));
        
        for (var i = 0; i < keys.Length - 1 ; i++)
            if(!IsKeyHeld(keys[i])) return false;

        return IsKeyDown(keys[^1]);
    }

    internal void Update()
    {
        // Update Mouse Position
        var windowPos = GetWindowPosition();

        Win32.GetCursorPos(ref _pointRef);
        _mousePosition.X = _pointRef.X - windowPos.X - 8; // TODO: not sure why this is needed but the point ref and window pos values are slightly off
        _mousePosition.Y = _pointRef.Y - windowPos.Y - 30; // TODO: is there a programmatic way to measure the title bar height?
            
        // Loop through supported Keycodes
        for (int i = 0; i < _keys.Length; i++)
        {
            var key = (int) _keys[i];
                
            _currentKeyState[key] = IsKeyPressed(key);

            _keyStates[key].IsDown = false;
            _keyStates[key].IsReleased = false;

            if (_currentKeyState[key] != _previousKeyState[key])
            {
                if (_currentKeyState[key])
                {
                    _keyStates[key].IsDown = !_keyStates[key].IsHeld;
                    _keyStates[key].IsHeld = true;
                }
                else
                {
                    _keyStates[key].IsReleased = true;
                    _keyStates[key].IsHeld = false;
                }
            }

            _previousKeyState[key] = _currentKeyState[key];
        }
    }

    private static bool IsKeyPressed(int key)
    {
        //If the high-order bit is 1, the key is down
        //otherwise, it is up.
        return (Win32.GetAsyncKeyState(key) & 0x8000) == 0x8000;
    }
    
    private Vector GetWindowPosition()
    {
        if (!Win32.GetWindowRect(Win32.GetConsoleWindow(), out IntRect rect))
        {
            var ex = Marshal.GetLastWin32Error();
            Console.WriteLine("Set error " + ex);
            throw new System.ComponentModel.Win32Exception(ex);
        }

        return new Vector(rect.Left, rect.Top);
    }
}