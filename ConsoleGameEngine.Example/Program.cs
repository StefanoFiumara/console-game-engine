namespace ConsoleGameEngine.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new CustomConsoleGameExample();
            
            game.InitConsole(128,64);
            game.Start();
        }
    }
    
    
}