using System;
using System.Collections.Generic;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core
{
    public static class Extensions
    {
        public static Vector NextVector(this Random rng, Rect bounds)
        {
            return new(
                rng.Next((int) bounds.Position.X + 1, (int) (bounds.Position.X + bounds.Size.X - 1)),
                rng.Next((int)bounds.Position.Y+1, (int)(bounds.Position.Y + bounds.Size.Y-1))
            );
        }
        
        public static void Shuffle<T>(this List<T> items, Random rng)
        {
            for (int i = items.Count - 1; i >= 1; i--)
            {
                int j = rng.Next(0, i + 1);

                (items[j], items[i]) = (items[i], items[j]);
            }
        }
    }
}