namespace ConsoleGameEngine.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new CustomConsoleGameExample();
            
            game.InitConsole(64,64);
            game.Start();
        }
    }
    
    
}