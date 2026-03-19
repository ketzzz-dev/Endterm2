using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GestureInput : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    
    [SerializeField] private float minPointDistance = 5f;
    
    private List<Vector2> currentStroke = new();
    private GestureRecogniser recognizer = new();

    private void Start()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        
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

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentStroke.Clear();
            
            lineRenderer.positionCount = 0;
        }
        if (Input.GetMouseButton(0))
        {
            var mousePosition = (Vector2)Input.mousePosition;
            
            if (currentStroke.Count == 0 || Vector2.Distance(currentStroke.Last(), mousePosition) > minPointDistance)
            {
                currentStroke.Add(mousePosition);

                var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                
                worldPosition.z = 0;
                lineRenderer.positionCount = currentStroke.Count;
                
                lineRenderer.SetPosition(currentStroke.Count - 1, worldPosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            var result = recognizer.Recognize(currentStroke);

            if (result.name == null) return;
            
            var worldCenter = GetWorldStrokeCenter(currentStroke);
                
            CastSpell(result.name, worldCenter);
        }
    }

    private void CastSpell(string spellName, Vector2 center)
    {
        Debug.Log("Casting " + spellName);
        
        // TODO: make a spell class and use a dictionary for easy lookup
    }

    private Vector2 GetWorldStrokeCenter(List<Vector2> points)
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