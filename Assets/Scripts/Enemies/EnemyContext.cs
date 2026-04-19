using UnityEngine;

public class EnemyContext
{
    // Sensor data
    public Vector2 playerPosition;
    public Vector2 directionToPlayer;
    public float distanceToPlayer;

    // Internal state
    public float currentHealth;
    public bool isDying;
    public bool isActionLocked;

    // Output data
    public Vector2 moveDirection;
    public string actionTrigger;
}