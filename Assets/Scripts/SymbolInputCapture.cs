using System.Collections.Generic;
using UnityEngine;

public class SymbolInputCapture : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float strokeWindowDuration = 1f;

    [Header("Sampling")]
    [SerializeField] private float minPointDistance = 5f; // pixels

    [Header("Rendering")]
    [SerializeField] private LineRenderer strokePrefab;

    private const int MaxStrokes = 3;

    private enum State { Idle, Drawing, Waiting }
    private State currentState = State.Idle;

    private readonly List<List<Vector2>> strokes = new();
    private readonly List<Vector2> currentStroke = new();

    private readonly List<LineRenderer> strokeRenderers = new();
    private LineRenderer currentRenderer;

    private Camera cam;
    private float strokeWindowTimer;

    private GestureRecognizer recognizer = new();

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        RegisterTestGestures();
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
            for (int i = 0; i < currentStroke.Count; i++)
            {
                var world = ScreenToWorld(currentStroke[i]);

                currentRenderer.SetPosition(i, world);
            }
        }
        
        for (int i = 0; i < strokes.Count; i++)
        {
            var r = strokeRenderers[i];

            if (r == null)
                continue;

            var stroke = strokes[i];

            for (int j = 0; j < stroke.Count; j++)
            {
                var world = ScreenToWorld(stroke[j]);

                r.SetPosition(j, world);
            }
        }
    }

    #region State Handlers

    private void HandleIdle()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        ResetAll();
        BeginStroke(Input.mousePosition);

        currentState = State.Drawing;
    }

    private void HandleDrawing()
    {
        if (Input.GetMouseButton(0))
        {
            TryAddPoint(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
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
        if (Input.GetMouseButtonDown(1))
        {
            TryCast();

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            BeginStroke(Input.mousePosition);

            currentState = State.Drawing;

            return;
        }

        strokeWindowTimer -= Time.deltaTime;

        if (strokeWindowTimer <= 0f)
        {
            Debug.Log("Rune fizzled — stroke window expired.");
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
        var name = recognizer.Recognize(strokes);

        if (name != null)
        {
            Debug.Log($"Rune recognized: {name}");
        }
        else
        {
            Debug.Log("Rune not recognized");
        }

        ResetAll();

        currentState = State.Idle;
    }

    #endregion

    #region Utilities

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        var world = cam.ScreenToWorldPoint(screenPos);

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

    #region Test Gestures

    private void RegisterTestGestures()
    {
        var circlePoints = new List<Vector2>();
        var step = 2f * Mathf.PI / 64;

        for (int i = 0; i < 64; i++)
        {
            float angle = i * step;
            circlePoints.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 50f);
        }

        recognizer.AddGesture("Circle", new List<List<Vector2>> { circlePoints });

        recognizer.AddGesture("Triangle", new List<List<Vector2>>
        {
            new List<Vector2>
            {
                Vector2.zero,
                Vector2.right * 100,
                Vector2.right * 50 + Vector2.up * 86.6f,
                Vector2.zero
            }
        });

        recognizer.AddGesture("Cross", new List<List<Vector2>>
        {
            new List<Vector2>
            {
                Vector2.zero,
                Vector2.right * 100
            },
            new List<Vector2>
            {
                Vector2.right * 50 + Vector2.down * 50,
                Vector2.right * 50 + Vector2.up * 50
            }
        });

        recognizer.AddGesture("Pitchfork", new List<List<Vector2>>
        {
            new List<Vector2>
            {
                Vector2.zero,
                Vector2.up * 100
            },
            new List<Vector2>
            {
                Vector2.left * 25 + Vector2.up * 100,
                Vector2.left * 25 + Vector2.up * 50,
                Vector2.right * 25 + Vector2.up * 50,
                Vector2.right * 25 + Vector2.up * 100
            }
        });
    }

    #endregion
}