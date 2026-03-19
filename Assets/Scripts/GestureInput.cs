using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GestureInput : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    
    [SerializeField] private float minPointDistance = 5f;
    
    private List<Vector2> currentStroke = new();
    private GestureRecogniser recognizer = new();
    
    private bool isDrawing;
    
    private Camera mainCamera;

    private void Start()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        mainCamera = Camera.main;
        
        // load templates here
        recognizer.AddTemplate("Fire", new List<Vector2> { // Fire = Triangle
            new(-1, -1),
            new(0, 1),
            new(1, -1)
        });
        recognizer.AddTemplate("Lightning", new List<Vector2> { // Lightning = Zig-Zag
            new(-1, 1),
            new(1, 1),
            new(-1, -1),
            new(1, -1)
        });
        recognizer.AddTemplate("Heal", new List<Vector2> { // Heal = X with a line on top
            new(-1, -1),
            new(1, 1),
            new(-1, 1),
            new(1, -1)
        });
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentStroke.Clear();
            
            lineRenderer.positionCount = 0;
            isDrawing = true;
        }
        if (Input.GetMouseButton(0))
        {
            var mousePosition = (Vector2)Input.mousePosition;
            
            if (currentStroke.Count == 0 || Vector2.Distance(currentStroke.Last(), mousePosition) > minPointDistance)
            {
                currentStroke.Add(mousePosition);
                
                lineRenderer.positionCount = currentStroke.Count;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            
            for (var i = 0; i < Mathf.Min(currentStroke.Count, lineRenderer.positionCount); i++)
            {
                var point = currentStroke[i];
                var depth = Mathf.Abs(mainCamera.transform.position.z);
                var worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(point.x, point.y, depth));

                worldPoint.z = 0;
                    
                lineRenderer.SetPosition(i, worldPoint);
            }
            
            var result = recognizer.Recognize(currentStroke);

            if (result.name == null) return;
            
            var worldCenter = GetWorldStrokeCenter(currentStroke);
                
            CastSpell(result.name, worldCenter);
        }
    }

    private void LateUpdate()
    {
        if (!isDrawing) return;
        
        for (var i = 0; i < Mathf.Min(currentStroke.Count, lineRenderer.positionCount); i++)
        {
            var point = currentStroke[i];
            var depth = Mathf.Abs(mainCamera.transform.position.z);
            var worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(point.x, point.y, depth));

            worldPoint.z = 0;
                    
            lineRenderer.SetPosition(i, worldPoint);
        }
    }

    private void CastSpell(string spellName, Vector3 center)
    {
        Debug.Log($"Casting {spellName} at {center}");
        
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
        
        var depth = Mathf.Abs(mainCamera.transform.position.z);

        return mainCamera.ScreenToWorldPoint(new Vector3(centerX, centerY, depth));
    }
}