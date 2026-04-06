using System.Collections.Generic;
using UnityEngine;

public class RuneInputCapture : MonoBehaviour
{
    
    // How long the player has between strokes before the rune fizzles.
    [SerializeField] private float strokeWindowDuration = 1f;
    
    // How many points to skip between samples — prevents massive lists
    // from high-framerate input without losing meaningful shape data.
    [SerializeField] private int sampleInterval = 3;
    
    // The maximum number of strokes in a rune.
    private const int MaxStrokes = 3;

    private enum State { Idle, Drawing, Waiting, Casting }
    private State currentState = State.Idle;

    // All strokes completed so far in this rune attempt.
    private List<List<Vector2>> strokes = new();
    
    // The stroke currently being drawn.
    private List<Vector2> currentStroke = new();
    
    // Countdown timer for the between-stroke window.
    private float strokeWindowTimer;
    
    // Tracks how many frames have passed for sub-sampling.
    private int frameSampleCounter;

    // Plug your GestureRecognizer in here. Since it's a plain C# class,
    // you can either instantiate it here or inject it from a GameManager.
    private GestureRecognizer recognizer = new();

    private void Awake()
    {
        // Register your rune templates here (or load them from a data asset).
        // For example:
        // recognizer.AddGesture("Fireball", fireburnTemplateStrokes);
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Drawing:
                HandleDrawing();
                break;
            case State.Waiting:
                HandleWaiting();
                break;
        }
        // Casting is handled inline since it completes in a single frame.
    }

    private void LateUpdate()
    {
        // Optional: visualize the current strokes for debugging.
        // This is a very basic visualization; you can get creative with it.
        for (var i = 0; i < currentStroke.Count - 1; i++)
        {
            var start = Camera.main.ScreenToWorldPoint(currentStroke[i]);
            var end = Camera.main.ScreenToWorldPoint(currentStroke[i + 1]);

            start.z = 0f;
            end.z = 0f;

            Debug.DrawLine(start, end, Color.red);
        }

        foreach (var stroke in strokes)
        {
            for (var i = 0; i < stroke.Count - 1; i++)
            {
                var start = Camera.main.ScreenToWorldPoint(stroke[i]);
                var end = Camera.main.ScreenToWorldPoint(stroke[i + 1]);

                start.z = 0f;
                end.z = 0f;

                Debug.DrawLine(start, end, Color.green);
            }
        }
    }   

    private void HandleIdle()
    {
        // Begin a new rune on left mouse button down.
        if (Input.GetMouseButtonDown(0))
        {
            strokes.Clear();
            currentStroke.Clear();
            currentStroke.Add(Input.mousePosition);

            frameSampleCounter = 0;
            currentState = State.Drawing;
        }
    }

    private void HandleDrawing()
    {
        // Sample the mouse position at a reduced rate to keep point lists lean.
        // We always capture the very first point (counter == 0) above in Idle.
        if (Input.GetMouseButton(0))
        {
            frameSampleCounter++;
            
            if (frameSampleCounter % sampleInterval == 0)
            {
                currentStroke.Add(Input.mousePosition);
            }   
        }

        // Stroke ends when the player releases the left mouse button.
        if (Input.GetMouseButtonUp(0))
        {
            // A stroke needs at least 2 points to mean anything geometrically.
            if (currentStroke.Count >= 2)
            {
                strokes.Add(new List<Vector2>(currentStroke));
                currentStroke.Clear();

                // If the player has hit the stroke cap, go straight to Casting.
                // Otherwise, give them the window to draw another stroke.
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
                // Degenerate stroke (just a tap) — go back to Idle.
                strokes.Clear();

                currentState = State.Idle;
            }
        }
    }

    private void HandleWaiting()
    {
        // Right-click while waiting commits the cast with whatever strokes exist.
        if (Input.GetMouseButtonDown(1))
        {
            TryCast();

            return;
        }

        // Starting a new stroke restarts the drawing process.
        if (Input.GetMouseButtonDown(0))
        {
            currentStroke.Clear();
            currentStroke.Add(Input.mousePosition);

            frameSampleCounter = 0;
            strokeWindowTimer = 0f; // Timer only matters between strokes.
            currentState = State.Drawing;

            return;
        }

        // Tick the window timer. If it expires, the rune fizzles.
        strokeWindowTimer -= Time.deltaTime;
        
        if (strokeWindowTimer <= 0f)
        {
            Debug.Log("Rune fizzled — stroke window expired.");
            strokes.Clear();

            currentState = State.Idle;
        }
    }

    private void TryCast()
    {
        currentState = State.Casting;

        var (name, score) = recognizer.Recognize(strokes);

        if (name != null && score > 0.8f)
        {
            Debug.Log($"Rune recognized: {name} (confidence: {score:P0})");
            // TODO: dispatch a spell cast event here.
        }
        else
        {
            Debug.Log($"Rune not recognized (best match: {name}, score: {score:P0})");
            // TODO: play a fizzle effect.
        }

        strokes.Clear();

        currentState = State.Idle;
    }
}