using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Symbol
{
    public readonly string id;
    public readonly int strokeCount;
    public readonly List<float[]> templates = new();

    public Symbol(string id, int strokeCount)
    {
        this.id = id;
        this.strokeCount = strokeCount;
    }
}
    
public class SymbolRecognizer
{
    public const int NumPoints = 64;
    public const float SquareSize = 250f;

    private readonly float minScoreThreshold;
    private readonly float scoreMargin;

    private readonly List<Symbol> symbols = new();

    public SymbolRecognizer(float minScoreThreshold = 0.75f, float scoreMargin = 0.05f)
    {
        this.minScoreThreshold = minScoreThreshold;
        this.scoreMargin = scoreMargin;
    }

    public void AddSymbol(string name, List<List<Vector2>> strokes)
    {
        if (strokes == null || strokes.Count == 0)
            return;
        
        var loweredName = name.ToLowerInvariant();
        var symbol = symbols.Find(g => g.id == loweredName);

        if (symbol == null)
        {
            symbol = new Symbol(loweredName, strokes.Count);
            
            symbols.Add(symbol);
        }

        if (symbol.strokeCount != strokes.Count)
        {
            Debug.LogWarning($"Gesture '{name}' has a different stroke count ({strokes.Count}) than existing templates ({symbol.strokeCount}). Skipping.");
            
            return;
        }

        var unistrokes = GenerateUnistrokes(strokes);

        foreach (var unistroke in unistrokes)
        {
            var template = Vectorize(Normalize(unistroke));
            
            symbol.templates.Add(template);
        }
    }

    public string Recognize(List<List<Vector2>> strokes)
    {
        if (strokes == null || strokes.Count == 0 || symbols.Count == 0)
            return null;

        var combined = CombineStrokes(strokes);
        var candidate = Vectorize(Normalize(combined));
        
        var bestDistance = float.MaxValue;
        var secondBestDistance = float.MaxValue;
        string bestMatch = null;
        string secondBestMatch = null;

        foreach (var gesture in symbols)
        {
            if (gesture.strokeCount != strokes.Count)
                continue;

            var bestTemplateDistance = float.MaxValue;
            
            foreach (var template in gesture.templates)
            {
                var distance = OptimalCosineDistance(candidate, template);

                if (distance < bestTemplateDistance)
                    bestTemplateDistance = distance;
            }

            if (bestTemplateDistance < bestDistance)
            {
                secondBestDistance = bestDistance;
                bestDistance = bestTemplateDistance;
                secondBestMatch = bestMatch;
                bestMatch = gesture.id;
            }
            else if (bestTemplateDistance < secondBestDistance)
            {
                secondBestDistance = bestTemplateDistance;
                secondBestMatch = gesture.id;
            }
        }

        if (bestMatch == null)
            return null;
        
        var bestScore = 1f - (bestDistance / (0.5f * Mathf.PI));

        if (secondBestMatch == null)
        {
            Debug.Log($"Best match: {bestMatch} (score: {bestScore:F2}), No second match");
            
            return bestScore >= minScoreThreshold ? bestMatch : null;
        }

        var secondBestScore = 1f - (secondBestDistance / (0.5f * Mathf.PI));

        Debug.Log($"Best match: {bestMatch} (score: {bestScore:F2}), Second best: {secondBestMatch} (score: {secondBestScore:F2})");

        if (bestScore < minScoreThreshold || (bestScore - secondBestScore) < scoreMargin)
            return null;
        
        return bestMatch;
    }

    private List<List<Vector2>> GenerateUnistrokes(List<List<Vector2>> strokes)
    {
        var results = new List<List<Vector2>>();
        var orders = Permute(strokes);

        foreach (var order in orders)
        {
            var n = order.Count;
            var combinations = 1 << n;

            for (var mask = 0; mask < combinations; mask++)
            {
                var uni = new List<Vector2>();

                for (var i = 0; i < n; i++)
                {
                    var stroke = order[i];
                    
                    // reverse
                    if (((mask >> i) & 1) == 1)
                        stroke = stroke.AsEnumerable().Reverse().ToList();

                    uni.AddRange(stroke);
                }

                results.Add(uni);
            }
        }

        return results;
    }

    private List<List<List<Vector2>>> Permute(List<List<Vector2>> strokes)
    {
        var n = strokes.Count;
        var results = new List<List<List<Vector2>>>();
        var indices = Enumerable.Range(0, n).ToArray();

        while (true)
        {
            var permutation = new List<List<Vector2>>(n);
            
            for (var i = 0; i < n; i++)
                permutation.Add(strokes[indices[i]]);

            results.Add(permutation);

            // Generate next lexicographic permutation
            var k = n - 2;

            while (k >= 0 && indices[k] > indices[k + 1]) k--;

            if (k < 0)
                break;

            var l = n - 1;

            while (indices[k] > indices[l]) l--;

            (indices[k], indices[l]) = (indices[l], indices[k]);

            Array.Reverse(indices, k + 1, n - (k + 1));
        }

        return results;
    }

    private List<Vector2> CombineStrokes(List<List<Vector2>> strokes)
    {
        var result = new List<Vector2>();

        foreach (var s in strokes)
            result.AddRange(s);

        return result;
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

        for (var i = 1; i < points.Count; i++)
        {
            var prev = points[i - 1];
            var curr = points[i];
            var distance = Vector2.Distance(prev, curr);

            if (distance < Mathf.Epsilon)
                continue;

            while (accumulatedDistance + distance >= interval)
            {
                var t = (interval - accumulatedDistance) / distance;
                var newPoint = Vector2.Lerp(prev, curr, t);

                newPoints.Add(newPoint);

                prev = newPoint;
                distance = Vector2.Distance(prev, curr);
                accumulatedDistance = 0f;
            }

            accumulatedDistance += distance;
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
        
        var rangeX = maxX - minX;
        var rangeY = maxY - minY;
        
        var scaleX = rangeX > Mathf.Epsilon ? size / rangeX : size / rangeY;
        var scaleY = rangeY > Mathf.Epsilon ? size / rangeY : size / rangeX;
        
        var scaled = new List<Vector2>(points.Count);

        foreach (var point in points)
        {
            scaled.Add(new Vector2(
                (point.x - minX) * scaleX,
                (point.y - minY) * scaleY
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