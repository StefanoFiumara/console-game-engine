using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Input;

public class PlayerInput
{
    private readonly short _pixelSize;

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern short GetAsyncKeyState(int keyCode);
        
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(ref Point lpPoint);

    private readonly bool[] _currentKeyState;
    private readonly bool[] _previousKeyState;
        
    private readonly KeyState[] _keyStates;

    private readonly KeyCode[] _keys;
        
    // Point that will be updated by the function with the current mouse coordinates
    private Point _pointRef;
    private Vector _mousePosition;
        
    public Vector MousePosition => (_mousePosition / _pixelSize).Rounded;
        
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
    public bool IsKeyDown(KeyCode k)
    {
        return _keyStates[(int) k].IsDown;
    }

    /// <summary>
    /// Returns true if the given key was released in the current frame 
    /// </summary>
    public bool IsKeyUp(KeyCode k)
    {
        return _keyStates[(int) k].IsReleased;
    }

    /// <summary>
    /// Returns true if the given key is being held down in the current frame 
    /// </summary>
    public bool IsKeyHeld(KeyCode k)
    {
        return _keyStates[(int) k].IsHeld;
    }

    internal void Update(Vector windowPos)
    {
        // Update Mouse Position
        GetCursorPos(ref _pointRef);
        _mousePosition.X = _pointRef.X - windowPos.X - 8; // TODO: not sure why this is needed but the point ref and window pos values are slightly off
        _mousePosition.Y = _pointRef.Y - windowPos.Y - 30; // TODO: is there a programmatic way to measure the title bar?
            
        // only loop through supported keys in the KeyCode enum
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
        return (GetAsyncKeyState(key) & 0x8000) == 0x8000;
    }
}