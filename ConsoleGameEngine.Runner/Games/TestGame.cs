using System;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.Input;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Runner.Games
{
    public class TestGame : ConsoleGameEngineBase
    {
        protected override string Name => "Test Game";

        private const float GAME_TICK = 0.2f;
        private float _gameTimer;

        public TestGame()
        {
            InitConsole(64,64);
        }
        protected override bool Create()
        {
            _gameTimer = GAME_TICK;
            return true;
        }


        protected override bool Update(float elapsedTime, KeyboardInput input)
        {
            if (input.IsKeyDown(KeyCode.Esc))
            {
                return false;
            }

            var start = Vector.Zero;
            var end = ScreenRect.Center * 0.5f;

            DrawLine(start, end, ' ', bgColor: ConsoleColor.Red);

            // Ticks the game forward every GAME_TICK seconds.
            _gameTimer -= elapsedTime;
            if (_gameTimer <= 0f)
            {
                _gameTimer = GAME_TICK;
                // Tick logic here
            }

            return true;
        }
    }
}
