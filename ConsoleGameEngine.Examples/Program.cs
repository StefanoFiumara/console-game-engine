using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using ConsoleGameEngine.Core;
using ConsoleGameEngine.Core.Graphics.Renderers;

namespace ConsoleGameEngine.Runner;

[SupportedOSPlatform("windows")]
public static class Program
{
    private static void Main()
    {
        var games = 
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ConsoleGame)))
                .ToList();

        int choice;
        do
        {
            Console.OutputEncoding = Encoding.UTF8;
            InitConsoleDefaults();

            Console.WriteLine("\n Choose an application to run:\n");

            for (var i = 0; i < games.Count; i++)
            {
                var game = games[i];
                Console.WriteLine($" {i+1}: {game.Name}\n");
            }

            Console.WriteLine($" {games.Count+1}: Quit\n");
                
            Console.Write("\n >> ");
            var selection = Console.ReadLine();
                
            if (int.TryParse(selection, out choice))
            {
                choice--;
                if (choice >= 0 && choice < games.Count)
                {
                    var game = (ConsoleGame) Activator.CreateInstance(games[choice]);
                    Console.Clear();
                    game?.Start();
                }
            }
        } while (choice != games.Count);
    }

    private static void InitConsoleDefaults()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Title = "Main Menu";
        Console.CursorVisible = true;

        Console.SetWindowSize(40, 40);
        Console.SetBufferSize(40, 40);
        ConsoleRenderer.SetCurrentFont("Modern DOS 8x8", 11);
        Console.Clear();
    }
}