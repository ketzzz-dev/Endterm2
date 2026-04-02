using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Gesture
{
    public readonly string name;
    public readonly List<Vector2> points;

    public Gesture(string name, List<Vector2> points)
    {
        this.name = name;
        this.points = points;
    }
}
    
public class GestureRecogniser
{
    public const int NumPoints = 64;
    public const float SquareSize = 250f;

    private List<Gesture> templates = new();

    public void AddTemplate(string name, List<Vector2> points)
    {
        templates.Add(new Gesture(name, Normalize(points)));
    }

    public string Recognize(List<Vector2> points)
    {
        if (points == null || points.Count < 2 || templates.Count == 0)
            return null;

        var candidate = Normalize(points);

        var bestDistance = float.MaxValue;
        
        string bestMatch = null;

        foreach (var template in templates)
        {
            var dist = MinPathDistance(candidate, template.points);
            
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestMatch = template.name;
            }
        }

        return bestMatch;
    }

    private List<Vector2> Normalize(List<Vector2> points)
    {
        var resampled = Resample(points, NumPoints);
        var scaled = ScaleToSquare(resampled, SquareSize);
        var translated = TranslateToOrigin(scaled);
        
        return translated;
    }

    private List<Vector2> Resample(List<Vector2> points, int numPoints)
    {
        var pathLength = PathLength(points);
        var interval = pathLength / (numPoints - 1);

        var distanceTraveled = 0f;
        var oldPoints = new List<Vector2>(points);
        var newPoints = new List<Vector2> { oldPoints[0] };

        for (var i = 1; i < oldPoints.Count; i++)
        {
            var distance = Vector2.Distance(oldPoints[i - 1], oldPoints[i]);
            
            if ((distanceTraveled + distance) >= interval)
            {
                var t = (interval - distanceTraveled) / distance;
                var newPoint = Vector2.Lerp(oldPoints[i - 1], oldPoints[i], t);
                
                newPoints.Add(newPoint);
                oldPoints.Insert(i, newPoint);
                
                distanceTraveled = 0f;
            }
            else
            {
                distanceTraveled += distance;
            }
        }

        while (newPoints.Count < numPoints)
            newPoints.Add(oldPoints.Last());

        return newPoints;
    }

    private List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        var minX = points.Min(p => p.x);
        var maxX = points.Max(p => p.x);
        var minY = points.Min(p => p.y);
        var maxY = points.Max(p => p.y);

        var scale = Mathf.Max(maxX - minX, maxY - minY);
        
        if (scale < Mathf.Epsilon)
            return points;

        return points.Select(p => new Vector2(
            (p.x - minX) / scale * size,
            (p.y - minY) / scale * size
        )).ToList();
    }

    private List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        var centroid = new Vector2(
            points.Average(p => p.x),
            points.Average(p => p.y)
        );

        return points.Select(p => p - centroid).ToList();
    }

    private float MinPathDistance(List<Vector2> a, List<Vector2> b)
    {
        var forward = PathDistance(a, b);
        var reversed = PathDistance(a, b.AsEnumerable().Reverse().ToList());
        
        return Mathf.Min(forward, reversed);
    }
    
    private float PathDistance(List<Vector2> a, List<Vector2> b)
    {
        var sum = a.Select((t, i) => Vector2.Distance(t, b[i])).Sum();

        return sum / a.Count;
    }

    private float PathLength(List<Vector2> points)
    {
        var length = 0f;
        
        for (var i = 1; i < points.Count; i++)
        {
            length += Vector2.Distance(points[i - 1], points[i]);
        }
        
        return length;
    }
}