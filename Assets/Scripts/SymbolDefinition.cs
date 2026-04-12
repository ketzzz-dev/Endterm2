using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SymbolDefinition", menuName = "Symbols/SymbolDefinition")]
public class SymbolDefinition : ScriptableObject
{
    public string symbolId;

    public List<Template> templates = new();
}

[System.Serializable]
public class Template
{
    public List<Stroke> strokes = new();
}

[System.Serializable]
public class Stroke
{
    public List<Vector2> points = new();
}