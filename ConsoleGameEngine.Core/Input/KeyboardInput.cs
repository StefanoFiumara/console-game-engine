using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace ConsoleGameEngine.Core.Input
{
    public class KeyboardInput
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetAsyncKeyState(int keyCode);
        
        private readonly bool[] _currentKeyState;
        private readonly bool[] _previousKeyState;
        
        private readonly KeyState[] _keyStates;

        private readonly KeyCode[] _keys;

        internal KeyboardInput()
        {
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
        
        internal void Update()
        {
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
}