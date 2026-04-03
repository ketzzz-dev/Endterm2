using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Gesture
{
    public readonly string name;
    public readonly List<List<Vector2>> templates;

    public Gesture(string name)
    {
        this.name = name;
        templates = new List<List<Vector2>>();
    }

    public void AddTemplate(List<Vector2> points)
    {
        templates.Add(points);
    }
}
    
public class GestureRecogniser
{
    public const int NumPoints = 64;
    public const float SquareSize = 250f;
    public const float MinScore = 0.75f;

    private readonly List<Gesture> gestures = new();

    public void AddTemplate(string name, List<Vector2> points)
    {
        var gesture = gestures.Find(g => g.name == name);

        if (gesture == null)
        {
            gesture = new Gesture(name);
            
            gestures.Add(gesture);
        }
        
        gesture.AddTemplate(Normalize(points));
    }

    public string Recognize(List<Vector2> points)
    {
        if (points == null || points.Count < 2 || gestures.Count == 0)
            return null;

        var candidate = Normalize(points);
        var bestDistance = float.MaxValue;
        string bestMatch = null;

        foreach (var gesture in gestures)
        {
            foreach (var template in gesture.templates)
            {
                var dist = PathDistance(candidate, template);

                if (dist > bestDistance)
                    continue;
            
                bestDistance = dist;
                bestMatch = gesture.name;
            }
        }
        
        var score = 1f - bestDistance / (0.5f * Mathf.Sqrt(2) * SquareSize);
        
        return Mathf.Clamp01(score) < MinScore ? null : bestMatch;
    }

    private List<Vector2> Normalize(List<Vector2> points)
    {
        var resampled = Resample(points, NumPoints);
        var rotated = RotateToZero(resampled);
        var scaled = ScaleToSquare(rotated, SquareSize);
        var translated = TranslateToOrigin(scaled);
        
        return translated;
    }

    private List<Vector2> Resample(List<Vector2> points, int numPoints)
    {
        var pathLength = PathLength(points);
        
        if (pathLength < Mathf.Epsilon)
            return Enumerable.Repeat(points[0], numPoints).ToList();

        var interval = pathLength / (numPoints - 1);
        var accumulatedDistance = 0f;

        var oldPoints = new List<Vector2>(points);
        var newPoints = new List<Vector2> { oldPoints[0] };

        for (var i = 1; i < oldPoints.Count; i++)
        {
            var distance = Vector2.Distance(oldPoints[i - 1], oldPoints[i]);
            
            if (distance < Mathf.Epsilon)
                continue;

            if (accumulatedDistance + distance >= interval)
            {
                var t = (interval - accumulatedDistance) / distance;
                var newPoint = Vector2.Lerp(oldPoints[i - 1], oldPoints[i], t);

                newPoints.Add(newPoint);
                oldPoints.Insert(i, newPoint);

                accumulatedDistance = 0f;
                i--;
            }
            else
            {
                accumulatedDistance += distance;
            }
        }

        while (newPoints.Count < numPoints)
            newPoints.Add(oldPoints[^1]);

        return newPoints;
    }

    private List<Vector2> RotateToZero(List<Vector2> points)
    {
        var centroid = new Vector2(
            points.Average(p => p.x),
            points.Average(p => p.y)
        ); 
        
        var theta = Mathf.Atan2(points[0].y - centroid.y, points[0].x - centroid.x);
        var cos = Mathf.Cos(-theta);
        var sin = Mathf.Sin(-theta);
        
        var rotated = new List<Vector2>(points.Count);

        foreach (var point in points)
        {
            var dx = point.x - centroid.x;
            var dy = point.y - centroid.y;

            rotated.Add(new Vector2(
                dx * cos - dy * sin,
                dx * sin + dy * cos
            )); 
        }
        
        return rotated;
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
        
        var squareScale = 1f / scale * size;
        var scaled = new List<Vector2>(points.Count);

        foreach (var point in points)
        {
            scaled.Add(new Vector2(
                (point.x - minX) * squareScale,
                (point.y - minY) * squareScale
            ));
        }
        
        return scaled;
    }

    private List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        var centroid = new Vector2(
            points.Average(p => p.x),
            points.Average(p => p.y)
        );

        var translated = new List<Vector2>(points.Count);

        foreach (var point in points)
        {
            translated.Add(point - centroid);
        }
        
        return translated;
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