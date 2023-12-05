using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Core.GameObjects;
using ConsoleGameEngine.Core.Math;

namespace ConsoleGameEngine.Core.Physics;

using static System.Math;
public static class Raycast
{
    public const float MaxRaycastDepth = 20f;

    // DDA Raycast Algorithm
    public static RaycastInfo Send(Sprite map, Vector startPos, Vector direction, char impassable, float boundaryTolerance = 0.0025f)
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

            if (map.GetGlyph((int)mapCheck.X, (int)mapCheck.Y) == impassable)
            {
                result.Hit = true;
            }
        }

        if (result.Hit)
        {
            result.Intersection = startPos + direction * result.Distance;
            result.HitBoundary = DetermineBoundary(startPos, direction, mapCheck.Rounded, boundaryTolerance);
        }
            
        return result;
    }

    private static bool DetermineBoundary(Vector startPos, Vector direction, Vector endPos, float boundaryTolerance)
    {
        // To highlight tile boundaries, cast a ray from each corner
        // of the tile, to the player. The more coincident this ray
        // is to the rendering ray, the closer we are to a tile
        // boundary
        var boundaryRays = new List<(float distance, float dotProduct)>(4);

        // Test each corner of hit tile, storing the distance from
        // the player, and the calculated dot product of the two rays
        int idx = 0;
        for (int cornerX = 0; cornerX < 2; cornerX++)
        {
            for (int cornerY = 0; cornerY < 2; cornerY++)
            {
                // Angle of corner to eye
                var cornerRay = new Vector(
                    endPos.X + cornerX - startPos.X,
                    endPos.Y + cornerY - startPos.Y);

                // TODO: formalize dot product in Vector Class
                float dot = (direction.X * cornerRay.X / cornerRay.Magnitude) + (direction.Y * cornerRay.Y / cornerRay.Magnitude);

                boundaryRays.Add((cornerRay.Magnitude, dot));
            }
        }

        // Sort Pairs from closest to farthest
        boundaryRays = boundaryRays.OrderBy(v => v.distance).ToList();

        // First two/three are closest (we will never see all four)
        if (Acos(boundaryRays[0].dotProduct) < boundaryTolerance) return true;
        if (Acos(boundaryRays[1].dotProduct) < boundaryTolerance) return true;
        //if (Acos(boundaryRays[2].dotProduct) < boundaryTolerance) return true;

        return false;
    }
}

public struct RaycastInfo
{
    // Whether or not the raycast hit an object
    public bool Hit { get; set; }
        
    // Whether or not we hit a tile boundary
    public bool HitBoundary { get; set; }
        
    // Distance between start and end of raycast
    public float Distance { get; set; }
        
    public Vector Intersection { get; set; }
}