using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;
using ConsoleGameEngine.Core.Win32;

namespace ConsoleGameEngine.Core
{
    public abstract class ConsoleGameEngineBase : ConsoleGameEngineWin32
    {
        private CharInfo[] _screenBuffer;
        private bool _isInit;

        private bool _gameRunning;

        protected int ScreenWidth => (int)ScreenRect.Size.X;
        protected int ScreenHeight => (int)ScreenRect.Size.Y;
        
        protected Rect ScreenRect { get; private set; }
        
        protected int TargetFps { get; set; }
        
        /// <summary>
        /// Enabling performance mode allows the game loop to run as fast as possible
        /// instead of suspending the game loop thread to hit a target framerate.
        /// This uses a lot more CPU.
        /// </summary>
        protected bool PerformanceModeEnabled { get; set; }

        protected ConsoleGameEngineBase()
        {
            _isInit = false;
            PerformanceModeEnabled = false;
        }
        
        public void InitConsole(int width, int height, int targetFps = 60)
        {
            if (!_isInit)
            {
                Console.OutputEncoding = Encoding.UTF8;
                
                // Clamp width and height while maintaining aspect ratio
                var maxWidth = Console.LargestWindowWidth - 1;
                var maxHeight = Console.LargestWindowHeight - 1; 
                
                if (width  > maxWidth || height > maxHeight)
                {
                    var widthRatio = (float) maxWidth / width;
                    var heightRatio = (float) maxHeight / height;
                    
                    // use whichever multiplier is smaller
                    var ratio = widthRatio < heightRatio ? widthRatio : heightRatio;

                    width = (int) (width * ratio);
                    height = (int) (height * ratio);
                }
                
                ScreenRect = new Rect(Vector.Zero, new Vector(width, height));
                
                _screenBuffer = new CharInfo[width * height];
                
                Console.SetWindowSize(width, height);
                Console.SetBufferSize(width, height);

                TargetFps = targetFps;
                if (TargetFps < 30) TargetFps = 30;
                _isInit = true;
            }
            else
            {
                throw new InvalidOperationException("Console is already initialized.");
            }
        }
        
        public void Start()
        {
            _gameRunning = true;
            var gameLoop = Task.Run(GameLoop);
            gameLoop.Wait();
        }
        
        protected bool IsKeyDown(params Keys[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (IsKeyDown(keys[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsKeyDown(Keys k)
        {
            return KeyStates.Down == (GetKeyState(k) & KeyStates.Down);
        }
        
        private void GameLoop()
        {
            if (!Create())
            {
                _gameRunning = false;
            }

            using var consoleStream = new StreamWriter(Console.OpenStandardOutput(ScreenWidth * ScreenHeight));
            var timer = new Stopwatch();
            timer.Start();

            var previousTime = timer.Elapsed.TotalMilliseconds;
            var currentTime = timer.Elapsed.TotalMilliseconds;
            var elapsedTime = (currentTime - previousTime);

            long framesRendered = 0;

            while (_gameRunning)
            {
                if (!Update((float) elapsedTime / 1000f))
                {
                    _gameRunning = false;
                }
                
                DrawBuffer(_screenBuffer, ScreenWidth, ScreenHeight);

                var averageFps = ++framesRendered / (timer.Elapsed.TotalMilliseconds / 1000f);
                Console.Title = $"Fano's Console Game Engine ~ Average FPS: {averageFps:F}";

                currentTime = timer.Elapsed.TotalMilliseconds;
                elapsedTime = (currentTime - previousTime);
                previousTime = currentTime;

                if (!PerformanceModeEnabled)
                {
                    // NOTE: Give back some system resources by suspending the thread if update loop takes less time than necessary to hit our target FPS.
                    //       This vastly reduces CPU usage!
                    var waitTime = 1f / TargetFps * 1000f - elapsedTime;
                    if (waitTime > 0)
                    {
                        Thread.Sleep((int)waitTime);
                    }
                }
            }
        }
        
        private int GetScreenIndex(int x, int y) => y * ScreenWidth + x;

        protected abstract bool Create();

        protected abstract bool Update(float elapsedTime);

        protected void Draw(Vector position, char c, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
        {
            Draw((int)position.X, (int)position.Y, c, fgColor, bgColor);
        }
        
        protected void Draw(int x, int y, char c, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
        {
            if (x >= ScreenWidth || x < 0 ||
                y >= ScreenHeight || y < 0
            )
            {
                return;
            }
         
            var index = GetScreenIndex(x, y);
            var color = (short)((int)fgColor + ((int)bgColor << 4));
            
            _screenBuffer[index].Attributes = color;
            _screenBuffer[index].Char.UnicodeChar = c;
        }

        protected void Fill(Rect rect, char c, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
        {
            Fill(rect.Position, rect.Size, c, fgColor, bgColor);
        }
        
        protected void Fill(Vector position, Vector size, char c, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
        {
            Fill((int)position.X, 
                 (int)position.Y,
                 (int)(size.X + position.X), 
                 (int)(size.Y + position.Y),
                 c, fgColor, bgColor);
        }
        
        protected void Fill(int x1, int y1, int x2, int y2, char c, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
        {
            Clip(ref x1, ref y1);
            Clip(ref x2, ref y2);

            for (int y = y1; y < y2; y++)
            {
                for (int x = x1; x < x2; x++)
                {
                    Draw(x, y, c, fgColor, bgColor);
                }
            }
        }

        protected void DrawSprite(Sprite sprite)
        {
            for (var y = 0; y < sprite.Height; y++)
            {
                for (int x = 0; x < sprite.Width; x++)
                {
                    if (sprite.GetGlyph(x, y) != ' ')
                    {
                        Draw(
                            (int)sprite.Position.X + x, 
                            (int)sprite.Position.Y + y, 
                            sprite.GetGlyph(x, y), 
                            sprite.GetFgColor(x, y),
                            sprite.GetBgColor(x, y));
                    }
                }
            }
        }
        
        protected void DrawString(int x, int y, string msg, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
        {
            for (int i = 0; i < msg.Length; i++)
            {
                Draw(x + i, y, msg[i], fgColor, bgColor);
            }
        }

        private void Clip(ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (x >= ScreenWidth) x = ScreenWidth;
            if (y < 0) y = 0;
            if (y >= ScreenHeight) y = ScreenHeight;
        }
    }
}