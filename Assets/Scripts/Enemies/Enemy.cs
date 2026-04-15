using UnityEngine;

// Enemy.cs
public class Enemy : MonoBehaviour, IDamageable
{
    // Set by the spawner at instantiation time, not in the Inspector
    public EnemyDefinition Definition { get; private set; }

    private float currentHealth;
    private float damageTimer;

    public void Initialize(EnemyDefinition definition)
    {
        Definition = definition;
        currentHealth = definition.maxHealth;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        if (damageTimer > 0f) return;

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(Definition.contactDamage);

            damageTimer = Definition.damageCooldown;
        }
    }

    private void Update()
    {
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, Definition.maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        // Later: drop experience, play VFX, return to pool
        Destroy(gameObject);
    }
}