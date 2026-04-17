using UnityEngine;

public class LightBallProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 20f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(damage, default);

            Destroy(gameObject);
        }
    }
}