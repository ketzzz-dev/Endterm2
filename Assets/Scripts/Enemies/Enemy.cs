using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Base Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float contactDamage = 10f;

    protected const float DamageCooldown = 1f;

    protected float currentHealth;  
    protected float damageTimer;

    protected bool isDying = false;

    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }
    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (isDying)
            return;
        
        if (!other.CompareTag("Player"))
            return;
        
        if (damageTimer > 0f)
            return;

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(contactDamage, default);

            Debug.Log($"Enemy dealt {contactDamage} contact damage to player.");

            damageTimer = DamageCooldown;
        }
    }

    protected virtual void Update()
    {
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;
    }

    public virtual void TakeDamage(float amount, Vector2 knockback)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        if (currentHealth <= 0f)
        {
            isDying = true;

            animator.SetTrigger("Die");
        }
    }

    protected virtual void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }
}