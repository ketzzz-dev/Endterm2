using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxMana = 50f;

    public float currentHealth { get; private set; }
    public float currentMana { get; private set; }

    private SpriteBlinker spriteBlinker;

    private void Awake()
    {
        spriteBlinker = GetComponent<SpriteBlinker>();
    }
    private void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        if (spriteBlinker != null)
            spriteBlinker.Blink();
        if (currentHealth <= 0)
            Die();
    }
    
    public void SpendMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, maxMana);
    }

    private void Die()
    {
        // Handle player death (e.g., respawn, game over, etc.)
        Debug.Log("Player has died.");
    }
}
