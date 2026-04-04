using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class GestureInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer strokeRendererPrefab;

    [Header("Input Settings")]
    [SerializeField] private float minPointDistance = 5f;
    [SerializeField] private float strokeTimeout = 0.5f;
    [SerializeField] private int maxStrokes = 3;
    [SerializeField] [Range(0f, 1f)] private float minScore = 0.8f;

    [Header("Display")]
    [SerializeField] private float castDisplayDuration = 1f;

    private readonly List<List<Vector2>> completedStrokes = new();
    private readonly List<Vector2> currentStroke = new();
    private readonly List<LineRenderer> completedRenderers = new();
    private LineRenderer currentRenderer;

    // The pool recycles LineRenderer GameObjects by deactivating them
    // instead of destroying them, then reactivating on next Get().
    private IObjectPool<LineRenderer> rendererPool;

    private readonly GestureRecognizer recognizer = new();
    private Camera mainCamera;

    private bool isWaiting;
    private float timeoutTimer;

    // isFrozen's ONLY job is to stop LateUpdate from re-projecting screen
    // coords onto the renderers, which is what makes them "stick" in world
    // space. It no longer touches Update or input handling at all.
    private bool isFrozen;

    // Stored so we can cancel specifically this coroutine without
    // using StopAllCoroutines(), which would kill any other timed
    // effects we add to this component later (fades, etc.).
    private Coroutine clearCoroutine;

    private void Awake()
    {
        // ObjectPool is generic over the pooled type. The four callbacks
        // cover the full lifecycle: create, borrow, return, and evict.
        rendererPool = new ObjectPool<LineRenderer>(
            createFunc: () =>
            {
                // Called when the pool needs a brand-new instance.
                var lr = Instantiate(strokeRendererPrefab, transform);
                lr.useWorldSpace = true;
                return lr;
            },
            actionOnGet: lr =>
            {
                // Called each time a renderer is borrowed from the pool.
                lr.positionCount = 0;
                lr.gameObject.SetActive(true);
            },
            actionOnRelease: lr =>
            {
                // Called each time a renderer is returned to the pool.
                // We clear positionCount so stale points don't flash
                // if the renderer is reactivated before being refreshed.
                lr.positionCount = 0;
                lr.gameObject.SetActive(false);
            },
            actionOnDestroy: lr => Destroy(lr.gameObject),
            defaultCapacity: maxStrokes
        );
    }

    private void Start()
    {
        mainCamera = Camera.main;
        
        recognizer.AddGesture("Cross", new()
        {
            new()
            {
                new(-1, 1),
                new(1, -1)
            },
            new()
            {
                new(1, 1),
                new(-1, -1)
            }
        });
        recognizer.AddGesture("Carat", new()
        {
            new()
            {
                new(-1,-1),
                new(0,1),
                new(1,-1)
            }
        });
    }

    private void Update()
    {
        // isFrozen is gone from here entirely. Input always runs so the
        // player can start a new gesture at any moment, even mid-display.
        HandleInput();

        if (isWaiting)
        {
            timeoutTimer -= Time.deltaTime;
            if (timeoutTimer <= 0f)
                TryRecognize();
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // If a cast is currently displaying, starting a new gesture
            // cancels it immediately rather than making the player wait.
            if (isFrozen)
            {
                if (clearCoroutine != null)
                    StopCoroutine(clearCoroutine);

                ClearAll();
            }

            // After ClearAll, stroke count is 0, so this guard is only
            // hit if the player somehow clicks again at max capacity
            // (e.g. recognition is still pending).
            if (completedStrokes.Count >= maxStrokes) return;

            isWaiting = false;
            currentStroke.Clear();
            currentRenderer = rendererPool.Get();
        }

        if (Input.GetMouseButton(0) && currentRenderer)
        {
            var mousePos = (Vector2)Input.mousePosition;

            if (currentStroke.Count == 0 ||
                Vector2.Distance(currentStroke[^1], mousePos) > minPointDistance)
            {
                currentStroke.Add(mousePos);
                currentRenderer.positionCount = currentStroke.Count;
            }
        }

        if (Input.GetMouseButtonUp(0) && currentStroke.Count > 0)
        {
            completedStrokes.Add(new List<Vector2>(currentStroke));
            completedRenderers.Add(currentRenderer);
            currentRenderer = null;
            currentStroke.Clear();

            if (completedStrokes.Count >= maxStrokes)
            {
                TryRecognize();
                return;
            }

            isWaiting = true;
            timeoutTimer = strokeTimeout;
        }
    }

    private void LateUpdate()
    {
        // This is the only place isFrozen matters. Skipping the re-projection
        // here is what causes the renderers to hold their last world-space
        // positions, making the rune appear to "land" on the world.
        if (isFrozen) return;

        for (var s = 0; s < completedStrokes.Count; s++)
            RefreshRenderer(completedRenderers[s], completedStrokes[s]);

        if (currentRenderer)
            RefreshRenderer(currentRenderer, currentStroke);
    }

    private void RefreshRenderer(LineRenderer lr, List<Vector2> screenPoints)
    {
        lr.positionCount = screenPoints.Count;
        for (var i = 0; i < screenPoints.Count; i++)
            lr.SetPosition(i, ScreenToWorld(screenPoints[i]));
    }

    private void TryRecognize()
    {
        isWaiting = false;

        var result = recognizer.Recognize(completedStrokes);

        if (result.score < minScore)
        {
            ClearAll();
            return;
        }

        var worldCenter = GetWorldStrokeCenter(completedStrokes.SelectMany(s => s).ToList());

        // TODO: Pass to SpellCaster
        Debug.Log($"Casting {result.name} ({result.score:F2}) at {worldCenter}");

        isFrozen = true;
        clearCoroutine = StartCoroutine(ClearAfterDelay(castDisplayDuration));
    }

    private IEnumerator ClearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearAll();
    }

    private void ClearAll()
    {
        isFrozen = false;
        isWaiting = false;

        // Release back to pool rather than destroying — the pool deactivates
        // the GameObjects and keeps them ready for the next gesture.
        foreach (var lr in completedRenderers)
            rendererPool.Release(lr);

        completedRenderers.Clear();
        completedStrokes.Clear();

        if (currentRenderer)
        {
            rendererPool.Release(currentRenderer);
            currentRenderer = null;
        }

        currentStroke.Clear();
        clearCoroutine = null;
    }

    private Vector2 ScreenToWorld(Vector2 screen) =>
        mainCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -mainCamera.transform.position.z));

    private Vector2 GetWorldStrokeCenter(List<Vector2> screenPoints)
    {
        var centroid = new Vector2(
            screenPoints.Average(p => p.x),
            screenPoints.Average(p => p.y)
        );
        return ScreenToWorld(centroid);
    }
}