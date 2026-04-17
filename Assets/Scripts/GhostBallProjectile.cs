using UnityEngine;

public class GhostBallProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 20f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(damage, default);

            Destroy(gameObject);
        }
    }
}