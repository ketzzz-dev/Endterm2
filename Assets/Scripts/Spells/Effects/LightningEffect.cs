using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightningEffect", menuName = "Spells/Effects/LightningEffect")]
public class LightningEffect : SpellEffect
{
    [SerializeField] private float initialDamage = 20f;
    [SerializeField] private float damageDecayPerBounce = 0.7f;
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float chainRadius = 4f;
    [SerializeField] private float knockback = 3f;
    [SerializeField] private GameObject lightningPrefab;

    public override void Cast(SpellCastContext context)
    {
        var origin = (Vector2)context.origin;
        var target = (Vector2)context.target;

        var toTarget = target - origin;
        var distance = toTarget.magnitude;

        if (distance <= Mathf.Epsilon)
            return;

        var direction = toTarget / distance;

        var hits = Physics2D.RaycastAll(origin, direction, distance);
        Transform firstEnemy = null;
        Vector2 currentPoint = origin + direction * distance;

        foreach (var hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (!hit.collider.CompareTag("Enemy"))
                continue;

            if (!hit.collider.TryGetComponent<IDamageable>(out var damageable))
                continue;

            firstEnemy = hit.collider.transform;
            currentPoint = hit.point;

            var knock = direction.normalized * knockback;
            damageable.TakeDamage(initialDamage, knock);
            break;
        }

        if (firstEnemy == null)
        {
            RenderChain(new List<Vector3>
            {
                context.origin,
                context.target
            });
            
            return;
        }

        var chainPoints = new List<Vector3>
        {
            context.origin, firstEnemy.position
        };

        var visited = new HashSet<Transform> { firstEnemy };
        var currentDamage = initialDamage * damageDecayPerBounce;

        for (var bounce = 0; bounce < maxBounces; bounce++)
        {
            var next = FindNearestEnemy(currentPoint, chainRadius, visited);
            if (next == null)
                break;
            
            chainPoints.Add(next.position);

            if (next.TryGetComponent<IDamageable>(out var nextDamageable))
            {
                var toNext = (Vector2)next.position - currentPoint;
                if (toNext.sqrMagnitude < 0.0001f)
                    toNext = Vector2.up;

                nextDamageable.TakeDamage(currentDamage, toNext.normalized * knockback);
            }

            visited.Add(next);
            currentPoint = next.position;
            currentDamage *= damageDecayPerBounce;
        }

        RenderChain(chainPoints);
    }

    private Transform FindNearestEnemy(Vector2 center, float radius, HashSet<Transform> visited)
    {
        var hits = Physics2D.OverlapCircleAll(center, radius);

        Transform best = null;
        var bestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit == null || !hit.CompareTag("Enemy"))
                continue;

            var t = hit.transform;
            if (visited.Contains(t))
                continue;

            var dist = ((Vector2)t.position - center).sqrMagnitude;
            if (dist < bestDistance)
            {
                bestDistance = dist;
                best = t;
            }
        }

        return best;
    }

    private void RenderChain(List<Vector3> points)
    {
        if (lightningPrefab == null || points.Count < 2)
            return;

        for (int i = 0; i < points.Count - 1; i++)
        {
            var bolt = Instantiate(lightningPrefab, Vector3.zero, Quaternion.identity);

            if (bolt.TryGetComponent<LightningVisual>(out var visual))
                visual.Render(points[i], points[i + 1]);
        }
    }   
}