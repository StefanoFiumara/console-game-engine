using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.GameObjects;

namespace ConsoleGameEngine.Runner.Games
{
    // ReSharper disable once UnusedType.Global
    public class Tetris : ConsoleGameEngineBase
    {
        protected override string Name { get; set; } = "Tetris";

        private const float GAME_TICK = 0.5f;

        private readonly List<Sprite> _tetrominos;

        private readonly string[] _shapes;
        private float _gameTimer;
        private int _level;

        public Tetris()
        {
            InitConsole(32, 50);

            _shapes = new string[7];
            _tetrominos = new List<Sprite>();

            _shapes[0] += "  X " + '\n';
            _shapes[0] += "  X " + '\n';
            _shapes[0] += "  X " + '\n';
            _shapes[0] += "  X " + '\n';

            _shapes[1] += "  X " + '\n';
            _shapes[1] += " XX " + '\n';
            _shapes[1] += " X  " + '\n';
            _shapes[1] += "    " + '\n';

            _shapes[2] += " X  " + '\n';
            _shapes[2] += " XX " + '\n';
            _shapes[2] += "  X " + '\n';
            _shapes[2] += "    " + '\n';

            _shapes[3] += " X  " + '\n';
            _shapes[3] += " XX " + '\n';
            _shapes[3] += " X  " + '\n';
            _shapes[3] += "    " + '\n';

            _shapes[4] += " X  " + '\n';
            _shapes[4] += " XXX" + '\n';
            _shapes[4] += "    " + '\n';
            _shapes[4] += "    " + '\n';

            _shapes[5] += "  X " + '\n';
            _shapes[5] += "XXX " + '\n';
            _shapes[5] += "    " + '\n';
            _shapes[5] += "    " + '\n';

            _shapes[6] += " XX " + '\n';
            _shapes[6] += " XX " + '\n';
            _shapes[6] += "    " + '\n';
            _shapes[6] += "    " + '\n';

            // TEMP
            foreach (var shape in _shapes)
            {
                _tetrominos.Add(new Sprite(shape));
            }

            // TODO: Define Boundaries
            // TODO: Rotation functions

        }

        protected override bool Create()
        {
            _level = 0;

            return true;
        }

        protected override bool Update(float elapsedTime)
        {
            if (IsKeyDown(Keys.Esc))
            {
                return false;
            }
            
            // Ticks the game forward every GAME_TICK seconds.
            _gameTimer -= elapsedTime;
            if (_gameTimer <= 0f)
            {
                Fill(ScreenRect, ' ');
                _gameTimer = GAME_TICK; // - _level * 0.02f;
                _tetrominos[_level].Position = ScreenRect.Center;
                
                DrawSprite(_tetrominos[_level]);
                _level = ++_level % _shapes.Length;
            }

            return true;
        }
    }
}