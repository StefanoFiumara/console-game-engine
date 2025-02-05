using ConsoleGameEngine.Core.Graphics;
using ConsoleGameEngine.Core.Math;
using static System.Math;

namespace ConsoleGameEngine.Core.Utilities;

public static class Raycast
{
    public const float MaxRaycastDepth = 20f;

    // DDA Raycast Algorithm
    public static RaycastInfo Send(Sprite map, Vector startPos, Vector direction, char impassable)
    {
        var result = new RaycastInfo();

        var unitStepSize = new Vector(
            x: Sqrt(1 + (direction.Y / direction.X) * (direction.Y / direction.X)),
            y: Sqrt(1 + (direction.X / direction.Y) * (direction.X / direction.Y)));

        var mapCheck = startPos.Rounded;

        var step = new Vector();
        var rayLength1D = new Vector();
            
        if (direction.X < 0)
        {
            step.X = -1;
            rayLength1D.X = (startPos.X - mapCheck.X) * unitStepSize.X;
        }
        else
        {
            step.X = 1;
            rayLength1D.X = ((mapCheck.X+1) - startPos.X) * unitStepSize.X;
        }

        if (direction.Y < 0)
        {
            step.Y = -1;
            rayLength1D.Y = (startPos.Y - mapCheck.Y) * unitStepSize.Y;
        }
        else
        {
            step.Y = 1;
            rayLength1D.Y = (mapCheck.Y+1 - startPos.Y) * unitStepSize.Y;
        }

        while (result is { Hit: false, Distance: < MaxRaycastDepth })
        {
            if (rayLength1D.X < rayLength1D.Y)
            {
                mapCheck.X += step.X;
                result.Distance = rayLength1D.X;
                rayLength1D.X += unitStepSize.X;
            }
            else
            {
                mapCheck.Y += step.Y;
                result.Distance = rayLength1D.Y;
                rayLength1D.Y += unitStepSize.Y;
            }

            if (map[(int)mapCheck.X, (int)mapCheck.Y] == impassable)
            {
                result.Hit = true;
            }
        }

        if (result.Hit) 
            result.Intersection = startPos + direction * result.Distance;
            
        return result;
    }
}

public struct RaycastInfo
{
    /// <summary>
    /// True if the raycast hit an object, false otherwise
    /// </summary>
    public bool Hit { get; set; }
    
    /// <summary>
    /// Distance between start and end of raycast
    /// </summary>
    public float Distance { get; set; }
        
    /// <summary>
    /// Point of collision
    /// </summary>
    public Vector Intersection { get; set; }
}