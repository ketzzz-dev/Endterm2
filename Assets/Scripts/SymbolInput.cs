using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SymbolInput : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float strokeWindowDuration = 1f;

    [Header("Sampling")]
    [SerializeField] private float minPointDistance = 5f; // pixels

    [Header("Rendering")]
    [SerializeField] private LineRenderer strokePrefab;

    [Header("Symbols")]
    [SerializeField] private List<SymbolDefinition> symbolDefinitions;

    public static event System.Action<string, Vector3> OnSymbolRecognized;

    private const int MaxStrokes = 4;

    private enum State { Idle, Drawing, Waiting }
    private State currentState = State.Idle;

    private readonly List<List<Vector2>> strokes = new();
    private readonly List<Vector2> currentStroke = new();

    private readonly List<LineRenderer> strokeRenderers = new();
    private LineRenderer currentRenderer;

    private Camera cam;
    private float strokeWindowTimer;

    private readonly SymbolRecognizer recognizer = new();

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        var startTime = Time.timeAsDouble;

        LoadSymbols();

        var elapsed = Time.timeAsDouble - startTime;

        Debug.Log($"Loaded {symbolDefinitions.Count} symbols in {elapsed:F2} seconds.");
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Idle: HandleIdle(); break;
            case State.Drawing: HandleDrawing(); break;
            case State.Waiting: HandleWaiting(); break;
        }
    }

    private void LateUpdate()
    {
        // Update current stroke renderer positions
        if (currentRenderer != null && currentStroke.Count > 0)
        {
            for (var i = 0; i < currentStroke.Count; i++)
            {
                var world = ScreenToWorld(currentStroke[i]);

                currentRenderer.SetPosition(i, world);
            }
        }
        
        for (var i = 0; i < strokes.Count; i++)
        {
            var r = strokeRenderers[i];

            if (r == null)
                continue;

            var stroke = strokes[i];

            for (var j = 0; j < stroke.Count; j++)
            {
                var world = ScreenToWorld(stroke[j]);

                r.SetPosition(j, world);
            }
        }
    }

    #region State Handlers

    private void HandleIdle()
    {
        var pointer = Pointer.current;
        
        if (pointer == null || !pointer.press.wasPressedThisFrame)
            return;

        ResetAll();
        BeginStroke(pointer.position.ReadValue());

        currentState = State.Drawing;
    }

    private void HandleDrawing()
    {
        var pointer = Pointer.current;
        
        if (pointer == null)
            return;
        
        if (pointer.press.isPressed)
        {
            TryAddPoint(pointer.position.ReadValue());
        }

        if (pointer.press.wasReleasedThisFrame)
        {
            if (currentStroke.Count >= 2)
            {
                FinalizeStroke();

                if (strokes.Count >= MaxStrokes)
                {
                    TryCast();
                }
                else
                {
                    strokeWindowTimer = strokeWindowDuration;
                    currentState = State.Waiting;
                }
            }
            else
            {
                ResetAll();

                currentState = State.Idle;
            }
        }
    }

    private void HandleWaiting()
    {
        var pointer = Pointer.current;
        var keyboard = Keyboard.current;
        
        if (pointer == null || keyboard == null)
            return;
        
        if (keyboard.leftShiftKey.wasPressedThisFrame)
        {
            TryCast();

            return;
        }

        if (pointer.press.wasPressedThisFrame)
        {
            BeginStroke(pointer.position.ReadValue());

            currentState = State.Drawing;

            return;
        }

        strokeWindowTimer -= Time.deltaTime;

        if (strokeWindowTimer <= 0f)
        {
            Debug.Log("Rune fizzled, stroke window expired.");
            ResetAll();

            currentState = State.Idle;
        }
    }

    #endregion

    #region Stroke Logic

    private void BeginStroke(Vector2 screenPos)
    {
        if (!strokePrefab)
        {
            Debug.LogError("Stroke prefab not assigned.");

            return;
        }

        currentStroke.Clear();
        currentStroke.Add(screenPos);

        currentRenderer = Instantiate(strokePrefab);
        currentRenderer.positionCount = 1;

        // Active stroke color
        currentRenderer.startColor = Color.red;
        currentRenderer.endColor = Color.red;

        strokeRenderers.Add(currentRenderer);
    }

    private void TryAddPoint(Vector2 screenPos)
    {
        if (currentStroke.Count > 0 && Vector2.Distance(currentStroke[^1], screenPos) < minPointDistance)
            return;

        currentStroke.Add(screenPos);

        currentRenderer.positionCount++;
    }

    private void FinalizeStroke()
    {
        strokes.Add(new List<Vector2>(currentStroke));
        currentStroke.Clear();

        // Completed stroke color
        currentRenderer.startColor = Color.green;
        currentRenderer.endColor = Color.green;
    }

    #endregion

    #region Casting

    private void TryCast()
    {
        var symbolId = recognizer.Recognize(strokes);

        if (!string.IsNullOrEmpty(symbolId))
        {
            var center = GetStrokesCenter();
            var worldPos = ScreenToWorld(center);

            OnSymbolRecognized?.Invoke(symbolId, worldPos);
        }

        ResetAll();

        currentState = State.Idle;
    }

    #endregion

    #region Utilities

    private Vector3 GetStrokesCenter()
    {
        var allPoints = new List<Vector2>();

        foreach (var stroke in strokes)
            allPoints.AddRange(stroke);
        
        if (allPoints.Count == 0)
            return Vector2.zero;

        var avgPoint = Vector2.zero;

        foreach (var p in allPoints)
            avgPoint += p;

        return avgPoint / allPoints.Count;
    }
    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        var z = -cam.transform.position.z;
        var world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));

        world.z = 0f;

        return world;
    }

    private void ResetAll()
    {
        strokes.Clear();
        currentStroke.Clear();

        foreach (var r in strokeRenderers)
        {
            if (r != null)
                Destroy(r.gameObject);
        }

        strokeRenderers.Clear();
        currentRenderer = null;
    }

    #endregion

    #region Symbols

    private void LoadSymbols()
    {
        if (symbolDefinitions == null || symbolDefinitions.Count == 0)
        {
            Debug.LogWarning("No symbol definitions assigned.");

            return;
        }

        foreach (var def in symbolDefinitions)
        {
            if (def == null || string.IsNullOrEmpty(def.symbolId))
            {
                Debug.LogWarning("Invalid symbol definition found, skipping.");

                continue;
            }

            foreach (var template in def.templates)
            {
                var strokes = new List<List<Vector2>>();

                foreach (var stroke in template.strokes)
                    strokes.Add(stroke.points);
                
                recognizer.AddSymbol(def.symbolId, strokes);
            }
        }
    }

    #endregion
}