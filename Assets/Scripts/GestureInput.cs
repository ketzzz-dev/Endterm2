using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GestureInput : MonoBehaviour
{
    [SerializeField] private LineRenderer worldStroke;
    [SerializeField] private LineRenderer screenStroke;
    
    [SerializeField] private float minPointDistance = 5f;
    
    private List<Vector2> currentStroke = new();
    private GestureRecogniser recognizer = new();

    private void Start()
    {
        worldStroke.positionCount = 0;
        screenStroke.positionCount = 0;
        worldStroke.useWorldSpace = true;
        screenStroke.useWorldSpace = false;
        
        // load templates here
        recognizer.AddTemplate("Fire", new() { // Fire = Triangle
            new Vector2(-1, -1),
            new Vector2(0, 1),
            new Vector2(1, -1)
        });
        recognizer.AddTemplate("Lightning", new() { // Lightning = Zig-Zag
            new Vector2(-1, 1),
            new Vector2(1, 1),
            new Vector2(-1, -1),
            new Vector2(1, -1)
        });
        recognizer.AddTemplate("Heal", new() { // Heal = X with a line on top
            new Vector2(-1, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(1, -1)
        });
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentStroke.Clear();
            
            screenStroke.positionCount = 0;
        }
        if (Input.GetMouseButton(0))
        {
            var mousePosition = (Vector2)Input.mousePosition;
            
            if (currentStroke.Count == 0 || Vector2.Distance(currentStroke.Last(), mousePosition) > minPointDistance)
            {
                currentStroke.Add(mousePosition);
                
                screenStroke.positionCount = currentStroke.Count;
                
                screenStroke.SetPosition(currentStroke.Count - 1, currentStroke.Last());
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            screenStroke.positionCount = 0;
            
            if (currentStroke.Count < 2)
                return;

            var worldPoints = GetWorldPoints(currentStroke);
            
            worldStroke.positionCount = worldPoints.Length;
            worldStroke.SetPositions(worldPoints);
            
            var result = recognizer.Recognize(currentStroke);

            if (result.name == null) return;
            
            var worldCenter = GetWorldStrokeCenter(worldPoints);
                
            CastSpell(result.name, worldCenter);
        }
    }

    private void CastSpell(string spellName, Vector2 center)
    {
        Debug.Log("Casting " + spellName);
        
        // TODO: make a spell class and use a dictionary for easy lookup
    }

    private Vector3[] GetWorldPoints(List<Vector2> points)
    {
        var worldPoints = currentStroke
            .Select(p => Camera.main.ScreenToWorldPoint(new Vector3(p.x, p.y, Camera.main.nearClipPlane + 1f)))
            .Select(v => new Vector3(v.x, v.y, 0));
        
        return worldPoints.ToArray();
    }

    private Vector2 GetWorldStrokeCenter(Vector3[] points)
    {
        var minX = points.Min(p => p.x);
        var maxX = points.Max(p => p.x);
        var minY = points.Min(p => p.y);
        var maxY = points.Max(p => p.y);
        
        var centerX = (minX + maxX) * 0.5f;
        var centerY = (minY + maxY) * 0.5f;

        return new Vector2(centerX, centerY);
    }
}