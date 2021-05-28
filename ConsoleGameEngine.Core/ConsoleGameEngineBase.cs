using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleGameEngine.Core.GameObjects;

namespace ConsoleGameEngine.Core
{
    public abstract class ConsoleGameEngineBase : ConsoleGameEngineWin32
    {
        private CharInfo[] _screenBuffer;
        private bool _isInit;

        private bool _gameRunning;

        protected int ScreenWidth { get; private set; }
        protected int ScreenHeight { get; private set; }

        protected ConsoleGameEngineBase()
        {
            _isInit = false;
        }

        public void InitConsole(int height, float aspectRatio)
        {
            var width = (int) (aspectRatio * height);
            InitConsole(width, height);
        }
        
        public void InitConsole(int width, int height)
        {
            if (!_isInit)
            {
                Console.OutputEncoding = Encoding.UTF8;
                
                // Clamp width and height while maintaining aspect ratio
                var maxWidth = Console.LargestWindowWidth - 1;
                var maxHeight = Console.LargestWindowHeight - 1; 
                
                if (width  > maxWidth || height > maxHeight)
                {
                    var widthRatio = (double) maxWidth / width;
                    var heightRatio = (double) maxHeight / height;
                    
                    // use whichever multiplier is smaller
                    double ratio = widthRatio < heightRatio ? widthRatio : heightRatio;

                    width = Convert.ToInt32(width * ratio);
                    height = Convert.ToInt32(height * ratio);
                }
                                
                ScreenWidth = width;
                ScreenHeight = height;
                _screenBuffer = new CharInfo[width * height];
                
                Console.SetWindowSize(width, height);
                Console.SetBufferSize(width, height);
                
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
        
        protected bool IsKeyDown(Keys key)
        {
            return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
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
            
            while (_gameRunning)
            {
                if (!Update((float) elapsedTime / 1000f))
                {
                    _gameRunning = false;
                }
                
                DrawBuffer(_screenBuffer, ScreenWidth, ScreenHeight);
                Console.Title = $"Fano's Console Game Engine ~ FPS: {1000f / elapsedTime:F}";

                currentTime = timer.Elapsed.TotalMilliseconds;
                elapsedTime = (currentTime - previousTime);
                previousTime = currentTime;
                
                // NOTE: Give back some system resources by suspending the thread if update loop takes less than 8ms
                //       This caps the game at around ~70 FPS and saves over 90% of its CPU usage!
                var waitTime = 8 - elapsedTime;
                if (waitTime > 0)
                {
                    Thread.Sleep((int)waitTime);
                }
            }
        }
        
        private int GetScreenIndex(int x, int y) => y * ScreenWidth + x;

        protected abstract bool Create();

        protected abstract bool Update(float elapsedTime);
        
        // TODO:
        //    * Circle, Line, triangles? 
        //    * DrawString(x, y, string message)

        protected void Draw(int x, int y, char c, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
        {
            var index = GetScreenIndex(x, y);
            if (index >= _screenBuffer.Length || index < 0)
            {
                return;
            }
            
            var color = (short)((int)fgColor + ((int)bgColor << 4));
            _screenBuffer[index].Attributes = color;
            _screenBuffer[index].Char.UnicodeChar = c;
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
            for (var i = 0; i < sprite.Height; i++)
            {
                for (int j = 0; j < sprite.Width; j++)
                {
                    if (sprite.GetGlyph(j, i) != ' ')
                    {
                        Draw(
                            (int)sprite.Position.X + j, 
                            (int)sprite.Position.Y + i, 
                            sprite.GetGlyph(j, i), 
                            sprite.GetColor(j, i));
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