using System.Collections.Generic;
using UnityEngine;

public class GestureInput : MonoBehaviour
{
    private List<Vector2> currentStroke = new();
    private GestureRecogniser recognizer = new();

    private void Start()
    {
        // Add templates (you'll record these yourself)
        recognizer.AddTemplate("Fire", LoadFireTemplate());
        recognizer.AddTemplate("Heal", LoadHealTemplate());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentStroke.Clear();
        }

        if (Input.GetMouseButton(0))
        {
            currentStroke.Add(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            var result = recognizer.Recognize(currentStroke);
            
            if (result.distance > 40f)
            {
                Debug.Log("Unrecognized gesture");
                return;
            }
            
            Debug.Log($"Spell: {result.name}, Score: {result.distance}");

            CastSpell(result.Item1, currentStroke);
        }
    }

    private void CastSpell(string spellName, List<Vector2> stroke)
    {
        Vector2 center = GetStrokeCenter(stroke);

        switch (spellName)
        {
            case "Fire":
                CastAtLocation(center);
                break;

            case "Heal":
                CastAtPlayer();
                break;
        }
    }

    private Vector2 GetStrokeCenter(List<Vector2> points)
    {
        float x = 0, y = 0;
        foreach (var p in points)
        {
            x += p.x;
            y += p.y;
        }
        return new Vector2(x / points.Count, y / points.Count);
    }

    private void CastAtLocation(Vector2 pos)
    {
        Debug.Log($"Casting at {pos}");
    }

    private void CastAtPlayer()
    {
        Debug.Log("Casting on player");
    }

    // Dummy templates (replace with real ones)
    private List<Vector2> LoadFireTemplate() => new()
    {
        new Vector2(0,0), new Vector2(1,1), new Vector2(2,0)
    };

    private List<Vector2> LoadHealTemplate() => new()
    {
        new Vector2(0,0), new Vector2(0,1), new Vector2(1,1)
    };
}