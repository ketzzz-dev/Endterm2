using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactKnockback = 5f;
    [SerializeField] [Range(0f, 1f)] private float knockbackReduction = 0f;

    [Header("Sensors")]
    [SerializeField] private float enemyDetectionRadius = 3f;


    [Header("Behaviours")]
    [SerializeField] private List<EnemyBehaviour> behaviours;

    private const float DamageCooldown = 1f;

    public EnemyContext context { get; private set; } = new();
    public new Rigidbody2D rigidbody { get; private set; }
    private Animator animator;
    private SpriteBlinker spriteBlinker;

    private Transform playerTransform;

    private float damageTimer = 0f;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteBlinker = GetComponent<SpriteBlinker>();
    }

    private void Start()
    {
        context.currentHealth = maxHealth;
        playerTransform = PlayerReference.Instance;

        foreach (var behaviour in behaviours)
            behaviour.Initialize(this);
    }

    private void Update()
    {
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        UpdateSensors();
        ExecuteBehaviours();
        TriggerAction();

        var targetVelocity = Vector2.zero;

        if (!context.isDying && !context.isActionLocked)
            targetVelocity = Vector2.ClampMagnitude(context.desiredDirection, 1f) * moveSpeed;

        var t = 1f - Mathf.Exp(-acceleration * Time.fixedDeltaTime);

        rigidbody.linearVelocity = Vector2.Lerp(rigidbody.linearVelocity, targetVelocity, t);

        if (!context.isDying)
        {
            animator.SetFloat("DirectionX", context.directionToPlayer.x);
            animator.SetFloat("DirectionY", context.directionToPlayer.y);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (context.isDying)
            return;
        
        if (collision.CompareTag("Player") && damageTimer <= 0f)
        {
            if (collision.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(contactDamage, context.directionToPlayer * contactKnockback);

            damageTimer = DamageCooldown;
        }
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (context.isDying)
            return;

        rigidbody.AddForce(knockback * (1f - knockbackReduction), ForceMode2D.Impulse);
        context.currentHealth = Mathf.Clamp(context.currentHealth - amount, 0f, maxHealth);

        if (spriteBlinker != null)
            spriteBlinker.Blink();
        if (context.currentHealth <= 0f)
        {
            animator.SetTrigger("Die");
            
            context.isDying = true;
        }
    }

    private void UpdateSensors()
    {
        if (playerTransform == null || context.isDying)
            return;
        
        var toPlayer = playerTransform.position - transform.position;
        var distance = toPlayer.magnitude;

        context.playerPosition = playerTransform.position;
        context.directionToPlayer = toPlayer / distance;
        context.distanceToPlayer = distance;

        context.nearbyEnemies.Clear();

        var colliders = Physics2D.OverlapCircleAll(transform.position, enemyDetectionRadius);

        foreach (var collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Enemy"))
                context.nearbyEnemies.Add(collider.transform);
        }
    }

    private void ExecuteBehaviours()
    {
        if (context.isDying)
            return;

        context.desiredDirection = Vector2.zero;
        context.actionTrigger = null;

        var eligible = new List<EnemyBehaviour>();

        foreach (var b in behaviours)
        {
            if (!b.CanExecute(context))
                continue;

            int insertIndex = eligible.Count;

            for (int i = 0; i < eligible.Count; i++)
            {
                if (b.GetPriority(context) < eligible[i].GetPriority(context))
                {
                    insertIndex = i;

                    break;
                }
            }

            eligible.Insert(insertIndex, b);
        }

        foreach (var behaviour in eligible)
            behaviour.Execute(context);
    }

    private void TriggerAction()
    {
        if (context.isDying || context.isActionLocked || string.IsNullOrEmpty(context.actionTrigger))
            return;

        animator.SetTrigger(context.actionTrigger);

        context.isActionLocked = true;
    }

    private void OnActionAnimationFinished()
    {
        context.isActionLocked = false;
    }
    
    private void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }
}