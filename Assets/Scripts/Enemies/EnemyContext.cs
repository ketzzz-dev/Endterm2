using System.Collections.Generic;
using UnityEngine;

public class EnemyContext
{
    // Sensor data
    public Vector2 playerPosition;
    public Vector2 directionToPlayer;
    public float distanceToPlayer;
    public List<Transform> nearbyEnemies = new();

    // Internal state
    public float currentHealth;
    public Dictionary<string, float> timers = new();
    public bool isDying;
    public bool isActionLocked;

    // Output data
    public Vector2 desiredDirection;
    public string actionTrigger;
}