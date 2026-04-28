using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningVisual : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.12f;
    [SerializeField] private int segmentCount = 8;
    [SerializeField] private float jaggedness = 0.25f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Render(Vector3 start, Vector3 end)
    {
        StartCoroutine(RenderRoutine(start, end));
    }

    private IEnumerator RenderRoutine(Vector3 start, Vector3 end)
    {
        var points = BuildPoints(start, end);
        lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
            lineRenderer.SetPosition(i, points[i]);

        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    private List<Vector3> BuildPoints(Vector3 start, Vector3 end)
    {
        var points = new List<Vector3>
        {
            start
        };

        var direction = (end - start).normalized;
        var length = Vector3.Distance(start, end);

        for (int i = 1; i < segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            Vector3 point = Vector3.Lerp(start, end, t);

            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
            point += perpendicular * Random.Range(-jaggedness, jaggedness) * length;

            points.Add(point);
        }

        points.Add(end);
        
        return points;
    }
}