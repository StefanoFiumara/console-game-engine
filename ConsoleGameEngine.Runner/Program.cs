using ConsoleGameEngine.Runner.Games;

namespace ConsoleGameEngine.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var exampleGame = new CustomConsoleGameExample();

            var snakeGame = new SnakeGame();
            
            snakeGame.InitConsole(50, 50);
            snakeGame.Start();
        }
    }
    
    
}