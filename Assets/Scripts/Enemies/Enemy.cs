using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] private float contactDamage = 10f;

    private const float DamageCooldown = 1f;

    protected float currentHealth;
    private float damageTimer;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        if (damageTimer > 0f)
            return;

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(contactDamage);

            damageTimer = DamageCooldown;
        }
    }

    private void Update()
    {
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        if (currentHealth <= 0f)
            animator.SetTrigger("Die");
    }

    private void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }
}