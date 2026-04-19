using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LightBallProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float knockback = 5f;

    private new Rigidbody2D rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage, rigidbody.linearVelocity.normalized * knockback);
            }

            Destroy(gameObject);
        }
    }
}