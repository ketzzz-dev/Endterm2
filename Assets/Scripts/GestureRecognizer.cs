using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class Gesture
{
    public readonly string name;
    public readonly List<float[]> templates = new();

    public Gesture(string name) => this.name = name;

    public void AddTemplate(float[] vectors) => templates.Add(vectors);
}
    
public class GestureRecogniser
{
    public const int NumPoints = 64;
    public const float SquareSize = 250f;

    private readonly List<Gesture> gestures = new();

    public void AddGesture(string name, List<Vector2> points)
    {
        if (points == null || points.Count < 2)
            return;
        
        var gesture = gestures.Find(g => g.name == name);

        if (gesture == null)
        {
            gesture = new Gesture(name);
            
            gestures.Add(gesture);
        }
        
        var normalized = Normalize(points);
        var vectorized = Vectorize(normalized);
        
        gesture.AddTemplate(vectorized);
    }

    public (string name, float score) Recognize(List<Vector2> points)
    {
        if (points == null || points.Count < 2 || gestures.Count == 0)
            return (null, 0f);

        var candidate = Vectorize(Normalize(points));
        var bestDistance = float.MaxValue;
        string bestMatch = null;

        foreach (var gesture in gestures)
        {
            foreach (var template in gesture.templates)
            {
                var dist = OptimalCosineDistance(candidate, template);

                if (dist > bestDistance)
                    continue;
            
                bestDistance = dist;
                bestMatch = gesture.name;
            }
        }
        
        var score = 1f - (bestDistance / (0.5f * Mathf.PI));
        
        return (bestMatch, Mathf.Clamp01(score));
    }

    private List<Vector2> Normalize(List<Vector2> points)
    {
        var resampled = Resample(points, NumPoints);
        var scaled = ScaleToSquare(resampled, SquareSize);
        var translated = TranslateToOrigin(scaled);
        
        return translated;
    }

    private float[] Vectorize(List<Vector2> points)
    {
        var vector = new float[points.Count * 2];
        var sum = 0f;

        for (var i = 0; i < points.Count; i++)
        {
            vector[i * 2] = points[i].x;
            vector[i * 2 + 1] = points[i].y;
            
            sum += points[i].sqrMagnitude;
        }
        
        var magnitude = Mathf.Sqrt(sum);

        if (magnitude > Mathf.Epsilon)
        {
            magnitude = 1f / magnitude;
            
            for (var i = 0; i < vector.Length; i++)
                vector[i] *= magnitude;
        }
        
        return vector;
    }
    
    private float OptimalCosineDistance(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            return float.MaxValue;

        var sum = 0f;
        var cross = 0f;

        for (var i = 0; i < a.Length; i += 2)
        {
            sum += a[i] * b[i] + a[i + 1] * b[i + 1];
            cross += a[i] * b[i + 1] - a[i + 1] * b[i];
        }

        var magnitude = Mathf.Sqrt(sum * sum + cross * cross);
        magnitude = Mathf.Clamp01(magnitude);

        return Mathf.Acos(magnitude);
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

        if (newPoints.Count > numPoints)
            newPoints = newPoints.Take(numPoints).ToList();

        return newPoints;
    }

    private List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        var minX = points.Min(p => p.x);
        var maxX = points.Max(p => p.x);
        var minY = points.Min(p => p.y);
        var maxY = points.Max(p => p.y);
        
        var width = size / (maxX - minX);
        var height = size / (maxY - minY);
        var scaled = new List<Vector2>(points.Count);

        foreach (var point in points)
        {
            scaled.Add(new Vector2(
                (point.x - minX) * width,
                (point.y - minY) * height
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