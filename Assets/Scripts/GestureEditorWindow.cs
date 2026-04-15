using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GestureEditorWindow : EditorWindow
{
    private const float CanvasPadding = 10f;
    private const float MinPointDistance = 5f;

    private List<List<Vector2>> strokes = new();
    private List<Vector2> currentStroke = new();

    private Stack<List<List<Vector2>>> undoStack = new();
    private Stack<List<List<Vector2>>> redoStack = new();

    private bool isDrawing;

    private SymbolRecognizer recognizer = new();
    private string previewResult;
    private float previewScore;

    private SymbolDefinition targetSymbol;

    private bool useSmoothing = false;
    private int smoothingIterations = 1;

    [MenuItem("Tools/Gesture Editor")]
    public static void Open()
    {
        GetWindow<GestureEditorWindow>("Gesture Editor");
    }

    private void OnGUI()
    {
        DrawTopBar();

        var canvasRect = GUILayoutUtility.GetRect(position.width, position.height - 150);
        
        DrawCanvas(canvasRect);

        DrawControls();
        DrawPreview();

        HandleInput(canvasRect);

        if (Event.current.type == EventType.Repaint)
            Repaint();
    }

    private void OnEnable()
    {
        var guids = AssetDatabase.FindAssets("t:SymbolDefinition");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<SymbolDefinition>(path);

            foreach (var template in def.templates)
            {
                var strokes = new List<List<Vector2>>();

                foreach (var stroke in template.strokes)
                    strokes.Add(stroke.points);

                recognizer.AddSymbol(def.symbolId, strokes);
            }
        }
    }

    #region UI

    private void DrawTopBar()
    {
        GUILayout.BeginHorizontal();

        targetSymbol = (SymbolDefinition)EditorGUILayout.ObjectField(
            "Symbol",
            targetSymbol,
            typeof(SymbolDefinition),
            false
        );

        if (GUILayout.Button("New", GUILayout.Width(50)))
        {
            ClearAll();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawControls()
    {
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Undo")) Undo();
        if (GUILayout.Button("Redo")) Redo();
        if (GUILayout.Button("Clear")) ClearAll();

        if (GUILayout.Button("Save Template"))
            SaveTemplate();

        GUILayout.EndHorizontal();

        useSmoothing = EditorGUILayout.Toggle("Use Smoothing", useSmoothing);
        smoothingIterations = EditorGUILayout.IntSlider("Smooth Iterations", smoothingIterations, 1, 3);
    }

    private void DrawPreview()
    {
        GUILayout.Space(10);

        GUILayout.Label("Preview:");

        if (!string.IsNullOrEmpty(previewResult))
        {
            GUILayout.Label($"Best Match: {previewResult}");
            GUILayout.Label($"Score: {previewScore:F2}");
        }
        else
        {
            GUILayout.Label("No match");
        }
    }

    #endregion

    #region Canvas

    private void DrawCanvas(Rect rect)
    {
        GUI.Box(rect, GUIContent.none);

        Handles.BeginGUI();

        foreach (var stroke in strokes)
            DrawStroke(stroke, Color.white);

        if (currentStroke.Count > 0)
            DrawStroke(currentStroke, Color.yellow);

        Handles.EndGUI();
    }

    private void DrawStroke(List<Vector2> stroke, Color color)
    {
        if (stroke.Count < 2)
            return;

        Handles.color = color;

        for (int i = 0; i < stroke.Count - 1; i++)
        {
            Handles.DrawLine(stroke[i], stroke[i + 1]);
        }
    }

    #endregion

    #region Input

    private void HandleInput(Rect canvas)
    {
        var e = Event.current;

        if (!canvas.Contains(e.mousePosition))
            return;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            BeginStroke(e.mousePosition);
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && isDrawing)
        {
            AddPoint(e.mousePosition);
            e.Use();
        }
        else if (e.type == EventType.MouseUp && isDrawing)
        {
            EndStroke();
            e.Use();
        }
    }

    private void BeginStroke(Vector2 pos)
    {
        SaveUndo();

        currentStroke.Clear();
        currentStroke.Add(pos);

        isDrawing = true;
    }

    private void AddPoint(Vector2 pos)
    {
        if (Vector2.Distance(currentStroke[^1], pos) < MinPointDistance)
            return;

        currentStroke.Add(pos);
    }

    private void EndStroke()
    {
        isDrawing = false;

        if (currentStroke.Count < 2)
        {
            currentStroke.Clear();
            return;
        }

        var finalStroke = new List<Vector2>(currentStroke);

        if (useSmoothing)
        {
            for (int i = 0; i < smoothingIterations; i++)
                finalStroke = Chaikin(finalStroke);
        }

        strokes.Add(finalStroke);
        currentStroke.Clear();

        UpdatePreview();
    }

    #endregion

    #region Undo/Redo

    private void SaveUndo()
    {
        undoStack.Push(Clone(strokes));
        redoStack.Clear();
    }

    private void Undo()
    {
        if (undoStack.Count == 0) return;

        redoStack.Push(Clone(strokes));
        strokes = undoStack.Pop();
    }

    private void Redo()
    {
        if (redoStack.Count == 0) return;

        undoStack.Push(Clone(strokes));
        strokes = redoStack.Pop();
    }

    private List<List<Vector2>> Clone(List<List<Vector2>> src)
    {
        var clone = new List<List<Vector2>>();

        foreach (var stroke in src)
            clone.Add(new List<Vector2>(stroke));

        return clone;
    }

    #endregion

    #region Recognition

    private void UpdatePreview()
    {
        if (strokes.Count == 0)
        {
            previewResult = null;
            return;
        }

        var result = recognizer.Recognize(strokes);

        if (!string.IsNullOrEmpty(result))
        {
            previewResult = result;
            previewScore = 1f; // your recognizer doesn't expose score directly
        }
        else
        {
            previewResult = null;
        }
    }

    #endregion

    #region Smoothing

    private List<Vector2> Chaikin(List<Vector2> points)
    {
        var result = new List<Vector2>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            var p0 = points[i];
            var p1 = points[i + 1];

            result.Add(Vector2.Lerp(p0, p1, 0.25f));
            result.Add(Vector2.Lerp(p0, p1, 0.75f));
        }

        return result;
    }

    #endregion

    #region Save

    private void SaveTemplate()
    {
        if (targetSymbol == null)
        {
            Debug.LogWarning("No SymbolDefinition selected.");

            return;
        }

        if (strokes.Count == 0)
        {
            Debug.LogWarning("No strokes to save.");

            return;
        }

        var template = new Template();

        foreach (var stroke in strokes)
        {
            var s = new Stroke
            {
                points = new List<Vector2>(stroke)
            };

            template.strokes.Add(s);
        }

        targetSymbol.templates.Add(template);

        EditorUtility.SetDirty(targetSymbol);
        AssetDatabase.SaveAssets();

        Debug.Log("Template saved.");

        ClearAll();
    }

    #endregion

    #region Utilities

    private void ClearAll()
    {
        strokes.Clear();
        currentStroke.Clear();

        undoStack.Clear();
        redoStack.Clear();

        previewResult = null;
    }

    #endregion
}