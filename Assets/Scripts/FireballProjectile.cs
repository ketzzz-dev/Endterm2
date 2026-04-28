using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float explosionRadius = 3.5f;
    [SerializeField] private float explosionKnockback = 8f;
    [SerializeField] private float lifeTime = 4f;

    private bool exploded;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded || other == null || other.CompareTag("Player"))
            return;

        if (other.CompareTag("Enemy"))
            Explode(other.ClosestPoint(transform.position));
        else
            Explode(transform.position);
    }

    private void Explode(Vector2 center)
    {
        if (exploded)
            return;

        exploded = true;

        var hits = Physics2D.OverlapCircleAll(center, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit == null || !hit.CompareTag("Enemy"))
                continue;

            if (!hit.TryGetComponent<IDamageable>(out var target))
                continue;

            var away = (Vector2)hit.transform.position - center;
            if (away.sqrMagnitude < 0.0001f)
                away = Vector2.up;

            target.TakeDamage(damage, away.normalized * explosionKnockback);
        }

        Destroy(gameObject);
    }
}