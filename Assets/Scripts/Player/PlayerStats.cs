using System;
using UnityEngine;

[RequireComponent(typeof(SpriteBlinker), typeof(Rigidbody2D))]
public class PlayerStats : MonoBehaviour, IDamageable
{
    public static event Action<float> OnHealthChanged;
    public static event Action<float> OnManaChanged;

    [SerializeField] public float maxHealth = 100f;
    [SerializeField] public float maxMana = 50f;

    public float currentHealth { get; private set; }
    public float currentMana { get; private set; }

    private SpriteBlinker spriteBlinker;
    private new Rigidbody2D rigidbody;

    private void Awake()
    {
        spriteBlinker = GetComponent<SpriteBlinker>();
        rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth);
        rigidbody.AddForce(knockback, ForceMode2D.Impulse);

        if (currentHealth <= 0)
            Die();
        if (spriteBlinker != null)
            spriteBlinker.Blink();
    }
    
    public void SpendMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, maxMana);

        OnManaChanged?.Invoke(currentMana);
    }

    private void Die()
    {
        // Handle player death (e.g., respawn, game over, etc.)
        Debug.Log("Player has died.");
    }
}
