using System;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games
{
    public class LineTest : ConsoleGameEngineBase
    {
        protected override string Name => "Test Game";

        private const float GAME_TICK = 0.02f;
        private float _gameTimer;

        private Vector _start;
        private Vector _end;
        private float _degrees;

        public LineTest()
        {
            InitConsole(64,64);
        }
        protected override bool Create()
        {
            _gameTimer = GAME_TICK;
            _start = ScreenRect.Center;
            _end = Vector.Zero;
            _degrees = 0;
            return true;
        }


        protected override bool Update(float elapsedTime, KeyboardInput input)
        {
            if (input.IsKeyDown(KeyCode.Esc))
            {
                return false;
            }

            // Ticks the game forward every GAME_TICK seconds.
            _gameTimer -= elapsedTime;
            if (_gameTimer <= 0f)
            {
                _gameTimer = GAME_TICK;
                // Tick logic here
                Fill(ScreenRect, ' ');

                _end = new Vector(
                    (float) Math.Cos(_degrees * Math.PI/180) * ScreenWidth * 0.5f,
                    (float) Math.Sin(_degrees * Math.PI/180) * ScreenHeight * 0.5f);

                _degrees++;

                DrawLine(_start, _start + _end, ' ', bgColor: ConsoleColor.Red);
                DrawString(0,0, $"Deg: {_degrees}");
            }

            return true;
        }
    }
}
