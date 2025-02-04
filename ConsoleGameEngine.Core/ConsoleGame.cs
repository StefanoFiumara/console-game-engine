using System;
using System.Diagnostics;
using System.Threading;
using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Graphics.Renderers;
using ConsoleGameEngine.Core.Input;

namespace ConsoleGameEngine.Core;

public abstract class ConsoleGame
{
    private readonly string _name;
    private readonly ConsoleRenderer _renderer;
    private readonly PlayerInput _input;
    private readonly int _targetFps;

    private bool _gameRunning;
    
    /// <summary>
    /// Creates a new instance of the Console Game Engine
    /// </summary>
    /// <param name="width">The desired width (in characters) of the console window.</param>
    /// <param name="height">The desired height (in characters) of the console window.</param>
    /// <param name="pixelSize">The desired size of each character in the console window.</param>
    /// <param name="targetFps">The target framerate for the application</param>
    /// <param name="name">Name of the application, will default to the class name if not provided, show in the window title.</param>
    protected ConsoleGame(int width, int height, short pixelSize = 8, int targetFps = 60, string name = "")
    {
        if (pixelSize < 1) pixelSize = 1;
        _renderer = new ConsoleRenderer(width, height, pixelSize);
        _name = name == "" ? GetType().Name : name;
        
        _targetFps = targetFps;
        if (_targetFps < 30) _targetFps = 30;
                
        _input = new PlayerInput(pixelSize);
    }

    public void Start()
    {
        _gameRunning = true;
        GameLoop();
    }

    private void GameLoop()
    {
        if (!Create(_renderer))
        {
            _gameRunning = false;
        }

        long framesRendered = 0;

        var timer = new Stopwatch();
        timer.Start();

        var previousTime = timer.Elapsed.TotalMilliseconds;
        while (_gameRunning)
        {
            var currentTime = timer.Elapsed.TotalMilliseconds;
            var elapsedTime = currentTime - previousTime;
            previousTime = currentTime;
            
            _input.Update();

            // Game Logic
            if (!Update((float) elapsedTime / 1000f, _renderer, _input))
            {
                _gameRunning = false;
            }
            
            // Draw the screen
            _renderer.Render();

            var averageFps = ++framesRendered / (timer.Elapsed.TotalMilliseconds / 1000f);
            Console.Title = $"{_name} ~ Average FPS: {averageFps:F}";
                
            // Give back some system resources by suspending the thread if update loop takes less time than necessary to hit our target FPS.
            // This vastly reduces CPU usage!
            var waitTime = 1f / _targetFps * 1000f - elapsedTime;
            if (waitTime > 0)
            {
                Thread.Sleep((int)waitTime);
            }
        }
    }

    /// <summary>
    /// Runs at startup, used to set up game data.
    /// This function should return true if setup was successful, false otherwise.
    /// /// <param name="renderer">The renderer used to draw objects on the screen</param>
    /// </summary>
    protected abstract bool Create(IRenderer renderer);

    /// <summary>
    /// The main game loop, runs once per frame.
    /// If this function returns false, the game loop stops and the application is terminated.
    /// </summary>
    /// <param name="elapsedTime">The elapsed time since the last frame (in seconds)</param>
    /// <param name="renderer">The renderer used to draw objects on the screen</param>
    /// <param name="input">The keyboard input state for the current frame</param>
    protected abstract bool Update(float elapsedTime, IRenderer renderer, PlayerInput input);
}