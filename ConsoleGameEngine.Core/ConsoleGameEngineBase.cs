using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleGameEngine.Core
{
    public abstract class ConsoleGameEngineBase : ConsoleGameEngineWin32
    {
        private int _screenWidth;
        private int _screenHeight;

        private CharInfo[] _screenBuffer;
        private bool _isInit;

        private bool _gameRunning;

        protected int ScreenWidth => _screenWidth;
        protected int ScreenHeight => _screenHeight;


        protected Dictionary<Keys, KeyStates> Keyboard;
        
        protected ConsoleGameEngineBase()
        {
            _isInit = false;
        }

        public void InitConsole(int height, float aspectRatio)
        {
            // ratio = w / h;
            // ratio * h = w

            var width = (int) (aspectRatio * height);
            
            InitConsole(width, height);
        }
        
        public void InitConsole(int width, int height)
        {
            if (!_isInit)
            {
                Console.OutputEncoding = Encoding.UTF8;
                
                // Clamp width and height to maximum values
                var maxWidth = Console.LargestWindowWidth - 1;
                var maxHeight = Console.LargestWindowHeight - 1; 
                
                if (width  > maxWidth || height > maxHeight)
                {
                    var ratioW = (double) maxWidth / width;
                    var ratioH = (double) maxHeight / (double) height;
                    
                    // use whichever multiplier is smaller
                    double ratio = ratioW < ratioH ? ratioW : ratioH;

                    width = Convert.ToInt32(width * ratio);
                    height = Convert.ToInt32(height * ratio);
                }
                                
                _screenWidth = width;
                _screenHeight = height;
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

        private void GameLoop()
        {
            if (!Create())
            {
                _gameRunning = false;
            }

            Keyboard = Enum.GetValues<Keys>().ToDictionary(k => k, k => KeyStates.None);
            
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
                
                // Draw Screen Buffer
                
                DrawBuffer(_screenBuffer, _screenWidth, _screenHeight);
                Console.Title = $"Fano's Console Game Engine ~ FPS: {1000 / elapsedTime:F}";

                currentTime = timer.Elapsed.TotalMilliseconds;
                elapsedTime = (currentTime - previousTime);
                previousTime = currentTime;
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
        //    * DrawString(x, y, string message) // size?

        public void Draw(int x, int y, char c, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
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
        
        public void Fill(int x1, int y1, int x2, int y2, char c, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
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

        private void Clip(ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (x >= ScreenWidth) x = ScreenWidth;
            if (y < 0) y = 0;
            if (y >= ScreenHeight) y = ScreenHeight;
        }

        public void DrawSprite(int x, int y, List<string> sprite, ConsoleColor fgColor = ConsoleColor.White, ConsoleColor bgColor = ConsoleColor.Black)
        {
            for (var i = 0; i < sprite.Count; i++)
            {
                var line = sprite[i];
                for (int j = 0; j < line.Length; j++)
                {
                    if (line[j] != ' ')
                    {
                        Draw(j + x, i + y, line[j], fgColor, bgColor);
                    }
                }
            }
        }
    }
}