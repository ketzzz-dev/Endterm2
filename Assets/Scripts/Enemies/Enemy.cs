using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    // Set by the spawner at instantiation time, not in the Inspector
    public EnemyDefinition Definition { get; private set; }

    private float currentHealth;
    private float damageTimer;

    private new Rigidbody2D rigidbody;
    private EnemyBehaviourState behaviourState;

    public void Initialize(EnemyDefinition definition)
    {
        Definition = definition;
        currentHealth = definition.maxHealth;

        behaviourState = definition.behaviour.CreateState();
    }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        if (damageTimer > 0f)
            return;

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            if (Definition.behaviour == null)
                return;
            
            target.TakeDamage(Definition.contactDamage);

            damageTimer = Definition.damageCooldown;
        }
    }

    private void Update()
    {
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (Definition.behaviour == null)
            return;

        var context = new EnemyBehaviourContext {};
        var desiredVelocity = Definition.behaviour.GetDesiredVelocity(context, behaviourState);

        rigidbody.linearVelocity = desiredVelocity;
    }

    public void TakeDamage(float amount)
    {
        if (Definition.behaviour == null)
            return;
        
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