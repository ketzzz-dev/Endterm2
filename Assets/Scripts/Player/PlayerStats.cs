using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteBlinker), typeof(Rigidbody2D))]
public class PlayerStats : MonoBehaviour, IDamageable
{
    public static event Action<float> OnHealthChanged;
    public static event Action<float> OnManaChanged;

    [Header("Stats")]
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] public float maxMana = 50f;
    [SerializeField] private float manaRegenRate = 2.5f;

    public float currentHealth { get; private set; }
    public float currentMana { get; private set; }

    private SpriteBlinker spriteBlinker;
    private new Rigidbody2D rigidbody;
    private ShieldVisual shieldVisual;

    private Coroutine healingRoutine;

    private bool shieldAvailable;
    private float shieldExpiresAt = -1f;

    private void Awake()
    {
        spriteBlinker = GetComponent<SpriteBlinker>();
        rigidbody = GetComponent<Rigidbody2D>();
        shieldVisual = GetComponentInChildren<ShieldVisual>();

        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth);
        OnManaChanged?.Invoke(currentMana);
    }

    private void Update()
    {
        if (currentMana < maxMana)
        {
            currentMana = Mathf.Clamp(currentMana + manaRegenRate * Time.deltaTime, 0f, maxMana);

            OnManaChanged?.Invoke(currentMana);
        }

        if (shieldAvailable && Time.time >= shieldExpiresAt)
        {
            shieldAvailable = false;
            shieldExpiresAt = -1f;

            shieldVisual.BreakShield();

            Debug.Log("Shield has expired.");
        }
    }

    public bool HasShield => shieldAvailable && Time.time < shieldExpiresAt;

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (amount <= 0f)
            return;

        CancelHealingOverTime();

        if (HasShield)
        {
            shieldAvailable = false;
            shieldExpiresAt = -1f;

            shieldVisual.BreakShield();

            Debug.Log("Shield blocked the hit.");

            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);

        rigidbody.AddForce(knockback, ForceMode2D.Impulse);

        if (spriteBlinker != null)
            spriteBlinker.Blink();

        if (currentHealth <= 0f)
            Die();
    }

    public void SpendMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0f, maxMana);
        OnManaChanged?.Invoke(currentMana);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHealth >= maxHealth)
            return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void ActivateShield(float duration)
    {
        if (duration <= 0f)
            return;

        shieldAvailable = true;
        shieldExpiresAt = Time.time + duration;

        shieldVisual.ActivateShield();
    }

    public void StartHealingOverTime(float totalAmount, float duration)
    {
        CancelHealingOverTime();
        healingRoutine = StartCoroutine(HealingRoutine(totalAmount, duration));
    }

    public void CancelHealingOverTime()
    {
        if (healingRoutine != null)
            StopCoroutine(healingRoutine);

        healingRoutine = null;
    }

    private IEnumerator HealingRoutine(float totalAmount, float duration)
    {
        if (totalAmount <= 0f || duration <= 0f)
        {
            healingRoutine = null;
            yield break;
        }

        if (currentHealth >= maxHealth)
        {
            healingRoutine = null;
            yield break;
        }

        var healed = 0f;

        while (healed < totalAmount && currentHealth < maxHealth)
        {
            var delta = (totalAmount / duration) * Time.deltaTime;
            var remaining = totalAmount - healed;

            if (delta > remaining)
                delta = remaining;

            Heal(delta);
            healed += delta;

            yield return null;
        }

        healingRoutine = null;
    }

    private void Die()
    {
        Debug.Log("Player has died.");
    }
}